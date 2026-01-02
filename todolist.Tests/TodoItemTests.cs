using Xunit;
using todolist.Models;

namespace todolist.Tests
{
    /// <summary>
    /// Unit tests for TodoItem model.
    /// </summary>
    public class TodoItemTests
    {
        [Fact]
        public void Create_SetsTitle()
        {
            // Act
            var item = TodoItem.Create("Test Task");

            // Assert
            Assert.Equal("Test Task", item.Title);
        }

        [Fact]
        public void Create_SetsIsCompletedToFalse()
        {
            // Act
            var item = TodoItem.Create("Test Task");

            // Assert
            Assert.False(item.IsCompleted);
        }

        [Fact]
        public void Create_GeneratesUniqueId()
        {
            // Act
            var item1 = TodoItem.Create("Task 1");
            var item2 = TodoItem.Create("Task 2");

            // Assert
            Assert.NotEqual(item1.Id, item2.Id);
        }

        [Fact]
        public void Create_SetsCreatedAt()
        {
            // Arrange
            var before = DateTime.Now;

            // Act
            var item = TodoItem.Create("Test Task");

            // Assert
            Assert.True(item.CreatedAt >= before);
            Assert.True(item.CreatedAt <= DateTime.Now);
        }

        [Fact]
        public void Title_CanBeChanged()
        {
            // Arrange
            var item = TodoItem.Create("Original");

            // Act
            item.Title = "Modified";

            // Assert
            Assert.Equal("Modified", item.Title);
        }

        [Fact]
        public void IsCompleted_CanBeChanged()
        {
            // Arrange
            var item = TodoItem.Create("Test Task");

            // Act
            item.IsCompleted = true;

            // Assert
            Assert.True(item.IsCompleted);
        }

        [Fact]
        public void PropertyChanged_RaisedForTitle()
        {
            // Arrange
            var item = TodoItem.Create("Original");
            var propertyChanged = false;
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TodoItem.Title))
                    propertyChanged = true;
            };

            // Act
            item.Title = "Modified";

            // Assert
            Assert.True(propertyChanged);
        }

        [Fact]
        public void PropertyChanged_RaisedForIsCompleted()
        {
            // Arrange
            var item = TodoItem.Create("Test Task");
            var propertyChanged = false;
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TodoItem.IsCompleted))
                    propertyChanged = true;
            };

            // Act
            item.IsCompleted = true;

            // Assert
            Assert.True(propertyChanged);
        }
    }
}
