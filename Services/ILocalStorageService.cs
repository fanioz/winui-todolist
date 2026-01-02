using System.Collections.Generic;
using System.Threading.Tasks;
using todolist.Models;

namespace todolist.Services
{
    /// <summary>
    /// Interface for local storage operations.
    /// Provides abstraction for task persistence.
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        /// Loads all tasks from local storage.
        /// </summary>
        /// <returns>List of TodoItem objects, or empty list if none exist.</returns>
        Task<List<TodoItem>> LoadTasksAsync();

        /// <summary>
        /// Saves all tasks to local storage.
        /// </summary>
        /// <param name="tasks">The collection of tasks to save.</param>
        Task SaveTasksAsync(IEnumerable<TodoItem> tasks);
    }
}
