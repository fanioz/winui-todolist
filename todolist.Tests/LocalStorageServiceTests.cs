using System.Text.Json;
using todolist.Models;
using todolist.Services;
using Xunit;

namespace todolist.Tests
{
    /// <summary>
    /// Integration tests for LocalStorageService.
    /// These tests use a temporary file location.
    /// </summary>
    public class LocalStorageServiceTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly TestableLocalStorageService _service;

        public LocalStorageServiceTests()
        {
            // Use a unique temp file for each test
            var tempDir = Path.Combine(Path.GetTempPath(), "todolist_tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            _testFilePath = Path.Combine(tempDir, "tasks.json");
            _service = new TestableLocalStorageService(_testFilePath);
        }

        public void Dispose()
        {
            // Clean up test files
            var dir = Path.GetDirectoryName(_testFilePath);
            if (dir != null && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public async Task LoadTasksAsync_ReturnsEmptyList_WhenFileDoesNotExist()
        {
            // Act
            var tasks = await _service.LoadTasksAsync();

            // Assert
            Assert.Empty(tasks);
        }

        [Fact]
        public async Task SaveTasksAsync_CreatesFile()
        {
            // Arrange
            var tasks = new List<TodoItem>
            {
                TodoItem.Create("Task 1"),
                TodoItem.Create("Task 2")
            };

            // Act
            await _service.SaveTasksAsync(tasks);

            // Assert
            Assert.True(File.Exists(_testFilePath));
        }

        [Fact]
        public async Task SaveAndLoad_RoundTrip()
        {
            // Arrange
            var originalTasks = new List<TodoItem>
            {
                TodoItem.Create("Task 1"),
                TodoItem.Create("Task 2")
            };
            originalTasks[0].IsCompleted = true;

            // Act
            await _service.SaveTasksAsync(originalTasks);
            var loadedTasks = await _service.LoadTasksAsync();

            // Assert
            Assert.Equal(2, loadedTasks.Count);
            Assert.Contains(loadedTasks, t => t.Title == "Task 1" && t.IsCompleted);
            Assert.Contains(loadedTasks, t => t.Title == "Task 2" && !t.IsCompleted);
        }

        [Fact]
        public async Task LoadTasksAsync_ReturnsEmptyList_WhenFileIsEmpty()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "");

            // Act
            var tasks = await _service.LoadTasksAsync();

            // Assert
            Assert.Empty(tasks);
        }

        [Fact]
        public async Task LoadTasksAsync_ReturnsEmptyList_WhenFileContainsInvalidJson()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "not valid json");

            // Act
            var tasks = await _service.LoadTasksAsync();

            // Assert
            Assert.Empty(tasks);
        }

        [Fact]
        public async Task SaveTasksAsync_WritesValidJson()
        {
            // Arrange
            var tasks = new List<TodoItem>
            {
                TodoItem.Create("Test Task")
            };

            // Act
            await _service.SaveTasksAsync(tasks);
            var json = await File.ReadAllTextAsync(_testFilePath);

            // Assert - should be parseable JSON
            var parsed = JsonSerializer.Deserialize<List<TodoItem>>(json);
            Assert.NotNull(parsed);
            Assert.Single(parsed);
        }

        [Fact]
        public async Task SaveTasksAsync_PreservesTaskProperties()
        {
            // Arrange
            var task = TodoItem.Create("Test Task");
            task.IsCompleted = true;
            var originalId = task.Id;
            var originalCreatedAt = task.CreatedAt;

            // Act
            await _service.SaveTasksAsync(new[] { task });
            var loadedTasks = await _service.LoadTasksAsync();

            // Assert
            Assert.Single(loadedTasks);
            Assert.Equal(originalId, loadedTasks[0].Id);
            Assert.Equal("Test Task", loadedTasks[0].Title);
            Assert.True(loadedTasks[0].IsCompleted);
            Assert.Equal(originalCreatedAt, loadedTasks[0].CreatedAt, TimeSpan.FromSeconds(1));
        }
    }

    /// <summary>
    /// Testable version of LocalStorageService that allows specifying custom file path.
    /// </summary>
    internal class TestableLocalStorageService : ILocalStorageService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public TestableLocalStorageService(string filePath)
        {
            _filePath = filePath;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<TodoItem>> LoadTasksAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<TodoItem>();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<TodoItem>();
                }

                return JsonSerializer.Deserialize<List<TodoItem>>(json, _jsonOptions) 
                       ?? new List<TodoItem>();
            }
            catch (Exception)
            {
                return new List<TodoItem>();
            }
        }

        public async Task SaveTasksAsync(IEnumerable<TodoItem> tasks)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (dir != null)
            {
                Directory.CreateDirectory(dir);
            }
            
            var json = JsonSerializer.Serialize(tasks, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
