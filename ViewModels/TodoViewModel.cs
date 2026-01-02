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
        /// Indicates if there are any completed tasks.
        /// </summary>
        public bool HasCompletedTasks => Tasks.Any(t => t.IsCompleted);

        /// <summary>
        /// Indicates if there are any tasks at all.
        /// </summary>
        public bool HasTasks => Tasks.Count > 0;

        // Commands
        public IAsyncRelayCommand LoadTasksCommand { get; }
        public IAsyncRelayCommand AddTaskCommand { get; }
        public IAsyncRelayCommand<TodoItem> RemoveTaskCommand { get; }
        public IRelayCommand<TodoItem> ToggleCompleteCommand { get; }
        public IAsyncRelayCommand ClearCompletedCommand { get; }
        public IAsyncRelayCommand RemoveSelectedCommand { get; }

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

            // Subscribe to collection changes to update HasCompletedTasks
            Tasks.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasCompletedTasks));
                OnPropertyChanged(nameof(HasTasks));
                ClearCompletedCommand.NotifyCanExecuteChanged();
            };
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
                var loadedTasks = await _storageService.LoadTasksAsync();
                
                Tasks.Clear();
                foreach (var task in loadedTasks.OrderByDescending(t => t.CreatedAt))
                {
                    // Subscribe to property changes for auto-save
                    task.PropertyChanged += Task_PropertyChanged;
                    Tasks.Add(task);
                }
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

            var newTask = TodoItem.Create(NewTaskText.Trim());
            newTask.PropertyChanged += Task_PropertyChanged;
            
            // Insert at the beginning for newest-first order
            Tasks.Insert(0, newTask);
            NewTaskText = string.Empty;

            await SaveTasksAsync();
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
                ClearCompletedCommand.NotifyCanExecuteChanged();
            }
            
            await SaveTasksAsync();
        }

        /// <summary>
        /// Saves all tasks to local storage.
        /// </summary>
        private async Task SaveTasksAsync()
        {
            await _storageService.SaveTasksAsync(Tasks);
        }
    }
}
