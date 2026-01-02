# To-Do List (WinUI 3)

A professional, desktop-first To-Do List application built with WinUI 3 and .NET 8, featuring Fluent Design, MVVM architecture, and full unit test coverage.

## ✨ Features

- **Fluent Design**: Implements Windows 11 aesthetics with Mica backdrop and Mint Green accenting.
- **Desktop-First UX**: Responsive layout optimized for desktop productivity (min 400x400, scales to 4K+).
- **Keyboard-First Interaction**: 
  - `Enter` to add a task.
  - `Delete` key to remove selected task.
  - `Ctrl + L` to clear all completed tasks.
  - Full Tab navigation and arrow key support.
- **Task Management**:
  - Context menu (right-click) for quick actions.
  - Checkbox toggles for task completion with strikethrough styling.
  - Delete confirmation dialogs to prevent accidental loss.
  - Status bar showing completion progress.
- **Task Filtering**: Filter tasks by All, Active, or Completed with live counts.
- **Persistence**: Automatically saves tasks to `%LOCALAPPDATA%\todolist\tasks.json`.
- **Accessibility**: Full AutomationProperties support for screen readers and high-contrast theme compatibility.

## 🛠 Technology Stack

- **UI Framework**: WinUI 3 (Windows App SDK)
- **Runtime**: .NET 8
- **Pattern**: MVVM (Model-View-ViewModel)
- **MVVM Toolkit**: `CommunityToolkit.Mvvm`
- **Data Persistence**: JSON serialization via `System.Text.Json`
- **Unit Testing**: xUnit with Moq for ViewModel and Service testing

## 🏗 Architecture

The app follows a clean MVVM pattern:

- **Models**: `TodoItem` - Data structure for a task.
- **ViewModels**: `TodoViewModel` - Orchestrates logic, task collection, and commands.
- **Services**: `LocalStorageService` - Handles async file I/O for persistence.
- **Views**: `MainPage` & `MainWindow` - UI definition and event routing.

## 🚀 Getting Started

### Prerequisites

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 with the "Windows application development" workload (optional, for IDE experience)

### How to Run

1.  **Clone or Open** the project directory.
2.  **Build** the project:
    ```powershell
    dotnet build todolist.csproj -c Debug -p:Platform=x64
    ```
3.  **Run** the application:
    ```powershell
    .\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\todolist.exe
    ```

### Running Tests

The solution includes 37 unit tests covering models, viewmodels, and persistence logic.

```powershell
dotnet test todolist.Tests\todolist.Tests.csproj -p:Platform=x64
```

## 🎨 Visual Style

- **Primary Accent**: Mint Green (`#3EB489`)
- **Backdrop**: Mica (follows system theme Windows 11)
- **Animations**: native `AddDeleteThemeTransition` (250ms)

---

Built with ❤️ using WinUI 3.