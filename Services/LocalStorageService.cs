using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using todolist.Models;

namespace todolist.Services
{
    /// <summary>
    /// Implements local storage using JSON file in AppData folder.
    /// </summary>
    public class LocalStorageService : ILocalStorageService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public LocalStorageService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "todolist");
            
            // Ensure directory exists
            Directory.CreateDirectory(appFolder);
            
            _filePath = Path.Combine(appFolder, "tasks.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <inheritdoc/>
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
                // If loading fails, return empty list
                return new List<TodoItem>();
            }
        }

        /// <inheritdoc/>
        public async Task SaveTasksAsync(IEnumerable<TodoItem> tasks)
        {
            try
            {
                var json = JsonSerializer.Serialize(tasks, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception)
            {
                // Silently fail for now - could add logging later
            }
        }
    }
}
