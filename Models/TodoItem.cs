using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace todolist.Models
{
    /// <summary>
    /// Represents a single to-do item with completion state tracking.
    /// </summary>
    public partial class TodoItem : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the task.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The title/description of the task.
        /// </summary>
        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// Whether the task has been completed.
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted;

        /// <summary>
        /// When the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Creates a new TodoItem with the specified title.
        /// </summary>
        public static TodoItem Create(string title)
        {
            return new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };
        }
    }
}
