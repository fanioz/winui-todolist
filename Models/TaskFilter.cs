namespace todolist.Models
{
    /// <summary>
    /// Filter options for the task list.
    /// </summary>
    public enum TaskFilter
    {
        /// <summary>
        /// Show all tasks.
        /// </summary>
        All,

        /// <summary>
        /// Show only active (incomplete) tasks.
        /// </summary>
        Active,

        /// <summary>
        /// Show only completed tasks.
        /// </summary>
        Completed
    }
}
