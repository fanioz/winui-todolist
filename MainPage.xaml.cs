using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using todolist.Models;
using todolist.ViewModels;
using Windows.System;

namespace todolist
{
    /// <summary>
    /// Main page for the To-Do List application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Gets the ViewModel from the App class.
        /// </summary>
        public TodoViewModel ViewModel => App.ViewModel;

        public MainPage()
        {
            InitializeComponent();
            
            // Subscribe to ViewModel property changes for UI updates
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.Tasks.CollectionChanged += Tasks_CollectionChanged;
            
            // Load tasks on startup
            LoadTasksOnStartup();
            
            // Update task count initially
            UpdateTaskCount();
        }

        /// <summary>
        /// Loads tasks asynchronously on startup.
        /// </summary>
        private async void LoadTasksOnStartup()
        {
            await ViewModel.LoadTasksCommand.ExecuteAsync(null);
            UpdateTaskCount();
        }

        /// <summary>
        /// Handles ViewModel property changes.
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TodoViewModel.HasTasks) ||
                e.PropertyName == nameof(TodoViewModel.HasCompletedTasks))
            {
                UpdateTaskCount();
            }
        }

        /// <summary>
        /// Handles collection changes for task count updates.
        /// </summary>
        private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTaskCount();
        }

        /// <summary>
        /// Updates the task count display in the status bar.
        /// </summary>
        private void UpdateTaskCount()
        {
            var total = ViewModel.Tasks.Count;
            var completed = 0;
            
            foreach (var task in ViewModel.Tasks)
            {
                if (task.IsCompleted) completed++;
            }
            
            if (total == 0)
            {
                TaskCountText.Text = "No tasks";
            }
            else
            {
                TaskCountText.Text = $"{completed}/{total} completed";
            }
        }

        /// <summary>
        /// Handles Enter key in the new task TextBox.
        /// </summary>
        private async void NewTaskTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && ViewModel.AddTaskCommand.CanExecute(null))
            {
                await ViewModel.AddTaskCommand.ExecuteAsync(null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles "Mark Complete" context menu click.
        /// </summary>
        private void MarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && 
                menuItem.DataContext is TodoItem task)
            {
                ViewModel.ToggleCompleteCommand.Execute(task);
            }
        }

        /// <summary>
        /// Handles "Delete" context menu click with confirmation.
        /// </summary>
        private async void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem &&
                menuItem.DataContext is TodoItem task)
            {
                // Set up confirmation dialog
                DeleteDialogContent.Text = $"Are you sure you want to delete \"{task.Title}\"?";
                DeleteConfirmationDialog.XamlRoot = this.XamlRoot;
                
                var result = await DeleteConfirmationDialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.RemoveTaskCommand.ExecuteAsync(task);
                }
            }
        }

        /// <summary>
        /// Handles "All" filter button click.
        /// </summary>
        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilterCommand.Execute(TaskFilter.All);
        }

        /// <summary>
        /// Handles "Active" filter button click.
        /// </summary>
        private void FilterActive_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilterCommand.Execute(TaskFilter.Active);
        }

        /// <summary>
        /// Handles "Completed" filter button click.
        /// </summary>
        private void FilterCompleted_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilterCommand.Execute(TaskFilter.Completed);
        }

        /// <summary>
        /// Handles "Edit" context menu click with edit dialog.
        /// </summary>
        private async void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem &&
                menuItem.DataContext is TodoItem task)
            {
                // Pre-fill the dialog with current title
                EditTaskTextBox.Text = task.Title;
                EditTaskDialog.XamlRoot = this.XamlRoot;
                
                var result = await EditTaskDialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary && 
                    !string.IsNullOrWhiteSpace(EditTaskTextBox.Text))
                {
                    await ViewModel.UpdateTaskCommand.ExecuteAsync((task, EditTaskTextBox.Text));
                }
            }
        }
    }
}
