using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace todolist.Models
{
    /// <summary>
    /// Represents the type of operation being tracked.
    /// </summary>
    public enum OperationType
    {
        AddTask,
        CompleteTask,
        SaveTasks,
        LoadTasks
    }

    /// <summary>
    /// Represents a complete operation with multiple progress stages.
    /// </summary>
    public partial class ProgressOperation : ObservableObject
    {
        /// <summary>
        /// Unique identifier for this operation.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The type of operation.
        /// </summary>
        [ObservableProperty]
        private OperationType _type;

        /// <summary>
        /// Human-readable description of the operation.
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Collection of stages in this operation.
        /// </summary>
        public ObservableCollection<ProgressStage> Stages { get; }

        /// <summary>
        /// Index of the currently active stage.
        /// </summary>
        [ObservableProperty]
        private int _currentStageIndex = -1;

        /// <summary>
        /// Whether this operation has been completed.
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted;

        /// <summary>
        /// When this operation started.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Creates a new ProgressOperation with the specified type and description.
        /// </summary>
        public ProgressOperation(OperationType type, string description)
        {
            Id = Guid.NewGuid();
            Type = type;
            Description = description;
            Stages = new ObservableCollection<ProgressStage>();
            StartTime = DateTime.Now;
            IsCompleted = false;

            InitializeStages();
        }

        /// <summary>
        /// Initializes the stages based on the operation type.
        /// </summary>
        private void InitializeStages()
        {
            switch (Type)
            {
                case OperationType.AddTask:
                    Stages.Add(new ProgressStage(ProgressStageType.Analyzing, "Validating"));
                    Stages.Add(new ProgressStage(ProgressStageType.Processing, "Creating"));
                    Stages.Add(new ProgressStage(ProgressStageType.Completing, "Adding"));
                    Stages.Add(new ProgressStage(ProgressStageType.Saved, "Saved"));
                    break;

                case OperationType.CompleteTask:
                    Stages.Add(new ProgressStage(ProgressStageType.Analyzing, "Checking"));
                    Stages.Add(new ProgressStage(ProgressStageType.Processing, "Updating"));
                    Stages.Add(new ProgressStage(ProgressStageType.Saved, "Saved"));
                    break;

                case OperationType.SaveTasks:
                    Stages.Add(new ProgressStage(ProgressStageType.Analyzing, "Preparing"));
                    Stages.Add(new ProgressStage(ProgressStageType.Processing, "Writing"));
                    Stages.Add(new ProgressStage(ProgressStageType.Saved, "Saved"));
                    break;

                case OperationType.LoadTasks:
                    Stages.Add(new ProgressStage(ProgressStageType.Analyzing, "Locating"));
                    Stages.Add(new ProgressStage(ProgressStageType.Processing, "Loading"));
                    Stages.Add(new ProgressStage(ProgressStageType.Completing, "Preparing"));
                    Stages.Add(new ProgressStage(ProgressStageType.Saved, "Ready"));
                    break;
            }
        }

        /// <summary>
        /// Advances to the next stage and updates progress.
        /// </summary>
        public void AdvanceToStage(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= Stages.Count)
                return;

            // Mark previous stages as completed
            for (int i = 0; i < stageIndex; i++)
            {
                Stages[i].IsActive = false;
                Stages[i].IsCompleted = true;
                Stages[i].ProgressValue = 100;
            }

            // Set current stage as active
            Stages[stageIndex].IsActive = true;
            Stages[stageIndex].ProgressValue = 50;

            // Deactivate future stages
            for (int i = stageIndex + 1; i < Stages.Count; i++)
            {
                Stages[i].IsActive = false;
                Stages[i].IsCompleted = false;
                Stages[i].ProgressValue = 0;
            }

            CurrentStageIndex = stageIndex;
        }

        /// <summary>
        /// Marks the operation as completed.
        /// </summary>
        public void MarkCompleted()
        {
            // Mark all stages as completed
            foreach (var stage in Stages)
            {
                stage.IsActive = false;
                stage.IsCompleted = true;
                stage.ProgressValue = 100;
            }

            CurrentStageIndex = Stages.Count - 1;
            IsCompleted = true;
        }

        /// <summary>
        /// Gets the overall progress percentage (0-100).
        /// </summary>
        public double GetOverallProgress()
        {
            if (Stages.Count == 0)
                return 0;

            double total = 0;
            foreach (var stage in Stages)
            {
                total += stage.ProgressValue;
            }

            return total / Stages.Count;
        }
    }
}
