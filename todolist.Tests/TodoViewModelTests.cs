using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using todolist.Models;
using todolist.Services;
using todolist.ViewModels;
using Xunit;

namespace todolist.Tests
{
    /// <summary>
    /// Unit tests for TodoViewModel.
    /// </summary>
    public class TodoViewModelTests
    {
        private readonly Mock<ILocalStorageService> _mockStorage;
        private readonly TodoViewModel _viewModel;

        public TodoViewModelTests()
        {
            _mockStorage = new Mock<ILocalStorageService>();
            _mockStorage.Setup(s => s.LoadTasksAsync())
                .ReturnsAsync(new List<TodoItem>());
            _mockStorage.Setup(s => s.SaveTasksAsync(It.IsAny<IEnumerable<TodoItem>>()))
                .Returns(Task.CompletedTask);
            
            _viewModel = new TodoViewModel(_mockStorage.Object);
        }

        [Fact]
        public void NewTaskText_InitiallyEmpty()
        {
            Assert.Equal(string.Empty, _viewModel.NewTaskText);
        }

        [Fact]
        public void Tasks_InitiallyEmpty()
        {
            Assert.Empty(_viewModel.Tasks);
        }

        [Fact]
        public void SelectedTodoItem_InitiallyNull()
        {
            Assert.Null(_viewModel.SelectedTodoItem);
        }

        [Fact]
        public async Task AddTaskCommand_AddsTaskToCollection()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";

            // Act
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            // Assert
            Assert.Single(_viewModel.Tasks);
            Assert.Equal("Test Task", _viewModel.Tasks[0].Title);
            Assert.False(_viewModel.Tasks[0].IsCompleted);
        }

        [Fact]
        public async Task AddTaskCommand_ClearsNewTaskText()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";

            // Act
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(string.Empty, _viewModel.NewTaskText);
        }

        [Fact]
        public async Task AddTaskCommand_SavesTasks()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";

            // Act
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            // Assert
            _mockStorage.Verify(s => s.SaveTasksAsync(It.IsAny<IEnumerable<TodoItem>>()), Times.Once);
        }

        [Fact]
        public void AddTaskCommand_CannotExecute_WhenTextIsEmpty()
        {
            // Arrange
            _viewModel.NewTaskText = "";

            // Assert
            Assert.False(_viewModel.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public void AddTaskCommand_CannotExecute_WhenTextIsWhitespace()
        {
            // Arrange
            _viewModel.NewTaskText = "   ";

            // Assert
            Assert.False(_viewModel.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public void AddTaskCommand_CanExecute_WhenTextHasContent()
        {
            // Arrange
            _viewModel.NewTaskText = "Valid Task";

            // Assert
            Assert.True(_viewModel.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public async Task RemoveTaskCommand_RemovesTaskFromCollection()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            var task = _viewModel.Tasks[0];

            // Act
            await _viewModel.RemoveTaskCommand.ExecuteAsync(task);

            // Assert
            Assert.Empty(_viewModel.Tasks);
        }

        [Fact]
        public async Task RemoveTaskCommand_SavesTasks()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            var task = _viewModel.Tasks[0];
            _mockStorage.ResetCalls();

            // Act
            await _viewModel.RemoveTaskCommand.ExecuteAsync(task);

            // Assert
            _mockStorage.Verify(s => s.SaveTasksAsync(It.IsAny<IEnumerable<TodoItem>>()), Times.Once);
        }

        [Fact]
        public void ToggleCompleteCommand_TogglesCompletionState()
        {
            // Arrange
            var task = TodoItem.Create("Test Task");
            _viewModel.Tasks.Add(task);
            Assert.False(task.IsCompleted);

            // Act
            _viewModel.ToggleCompleteCommand.Execute(task);

            // Assert
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void ToggleCompleteCommand_TogglesBack()
        {
            // Arrange
            var task = TodoItem.Create("Test Task");
            task.IsCompleted = true;
            _viewModel.Tasks.Add(task);

            // Act
            _viewModel.ToggleCompleteCommand.Execute(task);

            // Assert
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public async Task ClearCompletedCommand_RemovesOnlyCompletedTasks()
        {
            // Arrange
            _viewModel.NewTaskText = "Task 1";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 2";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 3";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            _viewModel.Tasks[0].IsCompleted = true; // Task 3 (newest first)
            _viewModel.Tasks[2].IsCompleted = true; // Task 1

            // Act
            await _viewModel.ClearCompletedCommand.ExecuteAsync(null);

            // Assert
            Assert.Single(_viewModel.Tasks);
            Assert.Equal("Task 2", _viewModel.Tasks[0].Title);
        }

        [Fact]
        public async Task LoadTasksCommand_LoadsTasksFromStorage()
        {
            // Arrange
            var storedTasks = new List<TodoItem>
            {
                TodoItem.Create("Stored Task 1"),
                TodoItem.Create("Stored Task 2")
            };
            _mockStorage.Setup(s => s.LoadTasksAsync()).ReturnsAsync(storedTasks);

            // Act
            await _viewModel.LoadTasksCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, _viewModel.Tasks.Count);
        }

        [Fact]
        public void HasTasks_ReturnsFalse_WhenEmpty()
        {
            Assert.False(_viewModel.HasTasks);
        }

        [Fact]
        public async Task HasTasks_ReturnsTrue_WhenHasTasks()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.HasTasks);
        }

        [Fact]
        public void HasCompletedTasks_ReturnsFalse_WhenNoCompletedTasks()
        {
            // Arrange
            var task = TodoItem.Create("Test Task");
            _viewModel.Tasks.Add(task);

            // Assert
            Assert.False(_viewModel.HasCompletedTasks);
        }

        [Fact]
        public void HasCompletedTasks_ReturnsTrue_WhenHasCompletedTasks()
        {
            // Arrange
            var task = TodoItem.Create("Test Task");
            task.IsCompleted = true;
            _viewModel.Tasks.Add(task);

            // Assert
            Assert.True(_viewModel.HasCompletedTasks);
        }

        [Fact]
        public async Task RemoveSelectedCommand_RemovesSelectedTask()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.SelectedTodoItem = _viewModel.Tasks[0];

            // Act
            await _viewModel.RemoveSelectedCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.Tasks);
            Assert.Null(_viewModel.SelectedTodoItem);
        }

        [Fact]
        public void RemoveSelectedCommand_CannotExecute_WhenNoSelection()
        {
            // Arrange
            _viewModel.SelectedTodoItem = null;

            // Assert
            Assert.False(_viewModel.RemoveSelectedCommand.CanExecute(null));
        }

        [Fact]
        public async Task RemoveSelectedCommand_CanExecute_WhenHasSelection()
        {
            // Arrange
            _viewModel.NewTaskText = "Test Task";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.SelectedTodoItem = _viewModel.Tasks[0];

            // Assert
            Assert.True(_viewModel.RemoveSelectedCommand.CanExecute(null));
        }

        #region Filter Tests

        [Fact]
        public void CurrentFilter_DefaultsToAll()
        {
            Assert.Equal(TaskFilter.All, _viewModel.CurrentFilter);
        }

        [Fact]
        public void SetFilterCommand_ChangesCurrentFilter()
        {
            // Act
            _viewModel.SetFilterCommand.Execute(TaskFilter.Active);

            // Assert
            Assert.Equal(TaskFilter.Active, _viewModel.CurrentFilter);
        }

        [Fact]
        public async Task FilteredTasks_ShowsAll_WhenFilterIsAll()
        {
            // Arrange
            _viewModel.NewTaskText = "Task 1";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 2";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.Tasks[0].IsCompleted = true;

            _viewModel.SetFilterCommand.Execute(TaskFilter.All);

            // Assert
            Assert.Equal(2, _viewModel.FilteredTasks.Count);
        }

        [Fact]
        public async Task FilteredTasks_ShowsOnlyActive_WhenFilterIsActive()
        {
            // Arrange
            _viewModel.NewTaskText = "Task 1";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 2";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 3";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            _viewModel.Tasks[0].IsCompleted = true;
            _viewModel.Tasks[2].IsCompleted = true;

            // Act
            _viewModel.SetFilterCommand.Execute(TaskFilter.Active);

            // Assert
            Assert.Single(_viewModel.FilteredTasks);
            Assert.Equal("Task 2", _viewModel.FilteredTasks[0].Title);
        }

        [Fact]
        public async Task FilteredTasks_ShowsOnlyCompleted_WhenFilterIsCompleted()
        {
            // Arrange
            _viewModel.NewTaskText = "Task 1";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 2";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 3";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            _viewModel.Tasks[0].IsCompleted = true;
            _viewModel.Tasks[2].IsCompleted = true;

            // Act
            _viewModel.SetFilterCommand.Execute(TaskFilter.Completed);

            // Assert
            Assert.Equal(2, _viewModel.FilteredTasks.Count);
        }

        [Fact]
        public async Task ActiveCount_ReturnsCorrectCount()
        {
            // Arrange
            _viewModel.NewTaskText = "Task 1";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 2";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 3";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            _viewModel.Tasks[0].IsCompleted = true;

            // Assert
            Assert.Equal(2, _viewModel.ActiveCount);
        }

        [Fact]
        public async Task CompletedCount_ReturnsCorrectCount()
        {
            // Arrange
            _viewModel.NewTaskText = "Task 1";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 2";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);
            _viewModel.NewTaskText = "Task 3";
            await _viewModel.AddTaskCommand.ExecuteAsync(null);

            _viewModel.Tasks[0].IsCompleted = true;
            _viewModel.Tasks[1].IsCompleted = true;

            // Assert
            Assert.Equal(2, _viewModel.CompletedCount);
        }

        [Fact]
        public void ActiveCount_ReturnsZero_WhenEmpty()
        {
            Assert.Equal(0, _viewModel.ActiveCount);
        }

        [Fact]
        public void CompletedCount_ReturnsZero_WhenEmpty()
        {
            Assert.Equal(0, _viewModel.CompletedCount);
        }

        [Fact]
        public void FilteredTasks_ReturnsEmpty_WhenNoTasks()
        {
            Assert.Empty(_viewModel.FilteredTasks);
        }

        #endregion
    }
}
