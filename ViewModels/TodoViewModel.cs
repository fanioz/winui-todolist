using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using todolist.Models;
using todolist.Services;

namespace todolist.ViewModels
{
    /// <summary>
    /// Main ViewModel for the To-Do List application.
    /// Manages task collection and all user interactions.
    /// </summary>
    public partial class TodoViewModel : ObservableObject
    {
        private readonly ILocalStorageService _storageService;

        /// <summary>
        /// Observable collection of all tasks.
        /// Automatically notifies UI of changes.
        /// </summary>
        public ObservableCollection<TodoItem> Tasks { get; } = new();

        /// <summary>
        /// The currently selected task in the ListView.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCommand))]
        private TodoItem? _selectedTodoItem;

        /// <summary>
        /// Text for new task input.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddTaskCommand))]
        private string _newTaskText = string.Empty;

        /// <summary>
        /// Indicates if tasks are currently being loaded.
        /// </summary>
        [ObservableProperty]
        private bool _isLoading;

        /// <summary>
        /// Current filter selection.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredTasks))]
        private TaskFilter _currentFilter = TaskFilter.All;

        /// <summary>
        /// Indicates if there are any completed tasks.
        /// </summary>
        public bool HasCompletedTasks => Tasks.Any(t => t.IsCompleted);

        /// <summary>
        /// Indicates if there are any tasks at all.
        /// </summary>
        public bool HasTasks => Tasks.Count > 0;

        /// <summary>
        /// Gets the count of active (incomplete) tasks.
        /// </summary>
        public int ActiveCount => Tasks.Count(t => !t.IsCompleted);

        /// <summary>
        /// Gets the count of completed tasks.
        /// </summary>
        public int CompletedCount => Tasks.Count(t => t.IsCompleted);

        /// <summary>
        /// The currently active progress operation being displayed.
        /// </summary>
        [ObservableProperty]
        private ProgressOperation? _currentOperation;

        /// <summary>
        /// Indicates if the progress dashboard should be visible.
        /// </summary>
        [ObservableProperty]
        private bool _isDashboardVisible;

        /// <summary>
        /// Overall progress percentage for the current operation (0-100).
        /// </summary>
        [ObservableProperty]
        private double _overallProgress;

        /// <summary>
        /// Status message displayed in the dashboard.
        /// </summary>
        [ObservableProperty]
        private string _dashboardStatusMessage = "Ready";

        /// <summary>
        /// Collection of active/queued progress operations.
        /// </summary>
        public ObservableCollection<ProgressOperation> ActiveOperations { get; } = new();

        /// <summary>
        /// Gets the filtered list of tasks based on CurrentFilter.
        /// </summary>
        public ObservableCollection<TodoItem> FilteredTasks
        {
            get
            {
                var filtered = CurrentFilter switch
                {
                    TaskFilter.Active => Tasks.Where(t => !t.IsCompleted),
                    TaskFilter.Completed => Tasks.Where(t => t.IsCompleted),
                    _ => Tasks
                };
                return new ObservableCollection<TodoItem>(filtered);
            }
        }

        // Commands
        public IAsyncRelayCommand LoadTasksCommand { get; }
        public IAsyncRelayCommand AddTaskCommand { get; }
        public IAsyncRelayCommand<TodoItem> RemoveTaskCommand { get; }
        public IRelayCommand<TodoItem> ToggleCompleteCommand { get; }
        public IAsyncRelayCommand ClearCompletedCommand { get; }
        public IAsyncRelayCommand RemoveSelectedCommand { get; }
        public IRelayCommand<TaskFilter> SetFilterCommand { get; }
        public IAsyncRelayCommand<(TodoItem task, string newTitle)> UpdateTaskCommand { get; }

        public TodoViewModel(ILocalStorageService storageService)
        {
            _storageService = storageService;

            // Initialize commands
            LoadTasksCommand = new AsyncRelayCommand(LoadTasksAsync);
            AddTaskCommand = new AsyncRelayCommand(AddTaskAsync, CanAddTask);
            RemoveTaskCommand = new AsyncRelayCommand<TodoItem>(RemoveTaskAsync);
            ToggleCompleteCommand = new RelayCommand<TodoItem>(ToggleComplete);
            ClearCompletedCommand = new AsyncRelayCommand(ClearCompletedAsync, () => HasCompletedTasks);
            RemoveSelectedCommand = new AsyncRelayCommand(RemoveSelectedAsync, CanRemoveSelected);
            SetFilterCommand = new RelayCommand<TaskFilter>(SetFilter);
            UpdateTaskCommand = new AsyncRelayCommand<(TodoItem task, string newTitle)>(UpdateTaskAsync);

            // Subscribe to collection changes to update computed properties
            Tasks.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasCompletedTasks));
                OnPropertyChanged(nameof(HasTasks));
                OnPropertyChanged(nameof(ActiveCount));
                OnPropertyChanged(nameof(CompletedCount));
                OnPropertyChanged(nameof(FilteredTasks));
                ClearCompletedCommand.NotifyCanExecuteChanged();
            };
        }

        private void SetFilter(TaskFilter filter)
        {
            CurrentFilter = filter;
        }

        /// <summary>
        /// Updates a task's title.
        /// </summary>
        private async Task UpdateTaskAsync((TodoItem task, string newTitle) args)
        {
            if (args.task == null || string.IsNullOrWhiteSpace(args.newTitle)) return;
            
            args.task.Title = args.newTitle.Trim();
            await SaveTasksAsync();
            
            // Refresh filtered list in case title affects display
            OnPropertyChanged(nameof(FilteredTasks));
        }

        private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskText);
        private bool CanRemoveSelected() => SelectedTodoItem != null;

        /// <summary>
        /// Loads tasks from local storage on startup.
        /// </summary>
        private async Task LoadTasksAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                await ExecuteWithProgressAsync(
                    OperationType.LoadTasks,
                    "Loading tasks...",
                    async (progressOp) =>
                    {
                        // Stage 0: Locating
                        AdvanceStage(progressOp, 0);
                        await Task.Delay(300);

                        // Stage 1: Loading
                        AdvanceStage(progressOp, 1);
                        await Task.Delay(400);
                        var loadedTasks = await _storageService.LoadTasksAsync();

                        // Stage 2: Preparing
                        AdvanceStage(progressOp, 2);
                        await Task.Delay(300);
                        Tasks.Clear();
                        foreach (var task in loadedTasks.OrderByDescending(t => t.CreatedAt))
                        {
                            task.PropertyChanged += Task_PropertyChanged;
                            Tasks.Add(task);
                        }

                        // Stage 3: Ready
                        AdvanceStage(progressOp, 3);
                        await Task.Delay(200);

                        return true;
                    });
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new task with the current NewTaskText.
        /// </summary>
        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskText)) return;

            await ExecuteWithProgressAsync(
                OperationType.AddTask,
                "Adding new task...",
                async (progressOp) =>
                {
                    // Stage 0: Validating
                    AdvanceStage(progressOp, 0);
                    await Task.Delay(300); // Brief validation simulation

                    var taskTitle = NewTaskText.Trim();

                    // Stage 1: Creating
                    AdvanceStage(progressOp, 1);
                    await Task.Delay(400);
                    var newTask = TodoItem.Create(taskTitle);
                    newTask.PropertyChanged += Task_PropertyChanged;

                    // Stage 2: Adding to list
                    AdvanceStage(progressOp, 2);
                    await Task.Delay(300);
                    Tasks.Insert(0, newTask);
                    NewTaskText = string.Empty;

                    // Stage 3: Saving
                    AdvanceStage(progressOp, 3);
                    await SaveTasksInternalAsync();

                    return true;
                });
        }

        /// <summary>
        /// Internal save method without progress tracking.
        /// </summary>
        private async Task SaveTasksInternalAsync()
        {
            await _storageService.SaveTasksAsync(Tasks);
        }

        /// <summary>
        /// Removes a specific task.
        /// </summary>
        private async Task RemoveTaskAsync(TodoItem? task)
        {
            if (task == null) return;

            task.PropertyChanged -= Task_PropertyChanged;
            Tasks.Remove(task);
            
            if (SelectedTodoItem == task)
            {
                SelectedTodoItem = null;
            }

            await SaveTasksAsync();
        }

        /// <summary>
        /// Removes the currently selected task.
        /// </summary>
        private async Task RemoveSelectedAsync()
        {
            if (SelectedTodoItem == null) return;
            await RemoveTaskAsync(SelectedTodoItem);
        }

        /// <summary>
        /// Toggles the completion state of a task.
        /// </summary>
        private void ToggleComplete(TodoItem? task)
        {
            if (task == null) return;
            task.IsCompleted = !task.IsCompleted;
            // Auto-save is handled by PropertyChanged event
        }

        /// <summary>
        /// Removes all completed tasks.
        /// </summary>
        private async Task ClearCompletedAsync()
        {
            var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
            
            foreach (var task in completedTasks)
            {
                task.PropertyChanged -= Task_PropertyChanged;
                Tasks.Remove(task);
            }

            if (SelectedTodoItem != null && !Tasks.Contains(SelectedTodoItem))
            {
                SelectedTodoItem = null;
            }

            await SaveTasksAsync();
        }

        /// <summary>
        /// Handles property changes on individual tasks for auto-save.
        /// </summary>
        private async void Task_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TodoItem.IsCompleted))
            {
                OnPropertyChanged(nameof(HasCompletedTasks));
                OnPropertyChanged(nameof(ActiveCount));
                OnPropertyChanged(nameof(CompletedCount));
                OnPropertyChanged(nameof(FilteredTasks));
                ClearCompletedCommand.NotifyCanExecuteChanged();
            }
            
            await SaveTasksAsync();
        }

        /// <summary>
        /// Saves all tasks to local storage with progress tracking.
        /// </summary>
        private async Task SaveTasksAsync()
        {
            await ExecuteWithProgressAsync(
                OperationType.SaveTasks,
                "Saving changes...",
                async (progressOp) =>
                {
                    // Stage 0: Preparing
                    AdvanceStage(progressOp, 0);
                    await Task.Delay(300);

                    // Stage 1: Writing
                    AdvanceStage(progressOp, 1);
                    await Task.Delay(300);
                    await SaveTasksInternalAsync();

                    // Stage 2: Saved
                    AdvanceStage(progressOp, 2);
                    await Task.Delay(200);

                    return true;
                });
        }

        /// <summary>
        /// Executes an operation with progress tracking visualization.
        /// </summary>
        private async Task<T> ExecuteWithProgressAsync<T>(
            OperationType operationType,
            string description,
            Func<ProgressOperation, Task<T>> operation)
        {
            var progressOp = new ProgressOperation(operationType, description);

            CurrentOperation = progressOp;
            IsDashboardVisible = true;
            DashboardStatusMessage = description;

            try
            {
                var result = await operation(progressOp);

                // Mark as completed and show final state
                progressOp.MarkCompleted();
                OverallProgress = 100;

                // Show completion for 2 seconds before dismissing
                await Task.Delay(2000);

                return result;
            }
            finally
            {
                IsDashboardVisible = false;
                CurrentOperation = null;
                OverallProgress = 0;
                DashboardStatusMessage = "Ready";
            }
        }

        /// <summary>
        /// Advances the progress operation to a specific stage.
        /// </summary>
        private void AdvanceStage(ProgressOperation operation, int stageIndex)
        {
            operation.AdvanceToStage(stageIndex);
            OverallProgress = operation.GetOverallProgress();
            OnPropertyChanged(nameof(CurrentOperation));
        }
    }
}
