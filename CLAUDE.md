# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Building the Project
```powershell
# Debug build
dotnet build todolist.csproj -c Debug -p:Platform=x64

# Release build (includes PublishReadyToRun and PublishTrimmed)
dotnet build todolist.csproj -c Release -p:Platform=x64

# Build for all supported architectures (x86, x64, ARM64)
dotnet build todolist.csproj -p:Platform=x64
dotnet build todolist.csproj -p:Platform=ARM64
dotnet build todolist.csproj -p:Platform=x86
```

### Running the Application
```powershell
# Run directly
dotnet run --project todolist.csproj

# Or run the built executable
.\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\todolist.exe
```

### Testing
```powershell
# Run all unit tests (37 tests covering models, viewmodels, and persistence)
dotnet test todolist.Tests/todolist.Tests.csproj -p:Platform=x64

# Run a single test
dotnet test --filter "TestMethodName" todolist.Tests/todolist.Tests.csproj
```

### Building MSIX Packages for Store
```powershell
# Build release packages with MSIX
dotnet build todolist.csproj -c Release -p:RuntimeIdentifier=win-x64 -p:GenerateAppxPackageOnBuild=true
dotnet build todolist.csproj -c Release -p:RuntimeIdentifier=win-x86 -p:GenerateAppxPackageOnBuild=true
dotnet build todolist.csproj -c Release -p:RuntimeIdentifier=win-arm64 -p:GenerateAppxPackageOnBuild=true
```

Output location: `AppPackages/todolist_1.0.0.0_<arch>_Test/`

## Architecture Overview

### MVVM Pattern
The application strictly follows Model-View-ViewModel separation:

- **Models** (`Models/`): Plain data classes with `ObservableObject` from CommunityToolkit.Mvvm
  - `TodoItem.cs` - Core task entity with INotifyPropertyChanged
  - `ProgressStage.cs` & `ProgressOperation.cs` - Multi-stage operation tracking

- **ViewModels** (`ViewModels/`): Business logic layer
  - `TodoViewModel.cs` - Central hub managing `ObservableCollection<TodoItem>`, commands, and state
  - Uses `[ObservableProperty]` and `[RelayCommand]` attributes for MVVM source generation
  - No UI code, only business logic

- **Views** (`MainPage.xaml`, `MainWindow.xaml`): Pure UI definition
  - XAML for layout and visual elements
  - Code-behind only for UI event routing (e.g., keyboard shortcuts, dialog setup)
  - Data binding to ViewModel via `x:Bind` for compiled bindings

- **Services** (`Services/`): Infrastructure layer
  - `LocalStorageService.cs` - Async JSON file I/O for persistence
  - Interface `ILocalStorageService` for testability

### Data Flow
1. App startup (`App.xaml.cs`) creates `TodoViewModel` with injected `LocalStorageService`
2. `MainPage` accesses `ViewModel` via static `App.ViewModel` property
3. UI changes trigger Commands in ViewModel
4. ViewModel modifies `ObservableCollection<TodoItem>` or calls Service
5. INotifyPropertyChanged automatically updates UI via compiled bindings
6. Service persists changes to `%LOCALAPPDATA%\todolist\tasks.json`

### Key Implementation Details

**MVVM Source Generation**: The project uses CommunityToolkit.Mvvm's source generators. When you see:
- `[ObservableProperty]` on a private field → public property with INotifyPropertyChanged is auto-generated
- `[RelayCommand]` on a method → Command property is auto-generated
- Never manually implement INotifyPropertyChanged or Command logic

**Compiled Bindings**: XAML uses `x:Bind` instead of `Binding` for:
- Better performance (compile-time instead of runtime)
- Type safety
- Intellisense support

**Async/Await Pattern**: All service operations (load/save) are async with proper error handling. ViewModel commands that call services are `IAsyncRelayCommand`.

**Progress Tracking System**: Recently added progress dashboard tracks multi-stage operations:
- `ProgressStage` represents individual stages (Analyzing, Processing, Completing, Saved)
- `ProgressOperation` orchestrates multiple stages
- `ExecuteWithProgressAsync<T>()` helper wraps operations with automatic stage advancement
- UI updates via property change notifications

## Project Structure

### Entry Points
- **`App.xaml.cs`**: Application startup, dependency injection container, ViewModel initialization
- **`MainWindow.xaml`**: Window shell with Mica backdrop and Frame
- **`MainPage.xaml`**: Primary UI - task list, input, filters, progress dashboard

### Key Directories
- `Models/` - Data models and enums
- `ViewModels/` - MVVM ViewModels
- `Services/` - Business logic and I/O
- `Converters/` - XAML value converters
- `Assets/` - Store logos and images for MSIX packaging

### Build Configuration
- **Target Framework**: `net8.0-windows10.0.19041.0`
- **Platform Min Version**: `10.0.17763.0` (Windows 10 October 2018 Update)
- **Supported Architectures**: x86, x64, ARM64
- **MSIX Packaging**: Single-project MSIX enabled (`EnableMsixTooling=true`)
- **Code Signing**: Configured with certificate thumbprint in `.csproj`

## Development Notes

### Adding New Features
1. **New Task Property**: Add to `TodoItem.cs` with `[ObservableProperty]`
2. **New Command**: Add method to `TodoViewModel.cs` with `[RelayCommand]`
3. **UI Element**: Add to `MainPage.xaml` and bind with `x:Bind ViewModel.PropertyName`
4. **Persistence**: Update `LocalStorageService.cs` read/write if needed

### Modifying the Progress Dashboard
- Progress stages defined in `ProgressOperation.InitializeStages()`
- Stage timing controlled in operations wrapped with `ExecuteWithProgressAsync()`
- `AdvanceStage()` method updates progress and triggers UI notifications

### Testing Strategy
- Tests are in separate `todolist.Tests/` project
- Use Moq to mock `ILocalStorageService` in ViewModel tests
- Tests linked as shared files in `.csproj` (not copied)
- Run tests before committing to catch regressions

### UI Customization
- Accent color: Override in `App.xaml` resources (currently Mint Green `#3EB489`)
- Theme: Mica backdrop enabled in `MainWindow.xaml`
- Animations: Use `AddDeleteThemeTransition` for list changes
- Keyboard shortcuts: Define in `MainPage.xaml.cs` constructor

### Platform Considerations
- **XAML Compiler**: Source generators require AOT-compatible patterns. Use partial properties instead of `[ObservableProperty]` when `x:Bind` is used in XAML.
- **MSIX Dependencies**: Windows App SDK runtime must be installed on target machines
- **Certificate**: Code signing certificate must be valid for MSIX package generation

### Common Gotchas
- **Nullable Reference Types**: Enabled throughout. Use `?` for nullable, `!` for non-null after validation
- **Async Initialization**: `LoadTasksAsync()` called in `MainPage` constructor via `async void` - acceptable for event handlers
- **Shared Test Files**: Tests are linked files, not copies. Changes to main code immediately affect tests
- **RuntimeIdentifier**: Required for builds. Use `-p:RuntimeIdentifier=win-x64` instead of `-p:Platform=x64` when packaging MSIX
