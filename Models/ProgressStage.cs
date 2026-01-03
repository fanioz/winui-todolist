using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace todolist.Models
{
    /// <summary>
    /// Represents the type of progress stage in an operation.
    /// </summary>
    public enum ProgressStageType
    {
        Analyzing,
        Processing,
        Completing,
        Saved
    }

    /// <summary>
    /// Represents a single stage in a progress operation with visual state.
    /// </summary>
    public partial class ProgressStage : ObservableObject
    {
        /// <summary>
        /// The type of this progress stage.
        /// </summary>
        public ProgressStageType StageType { get; set; }

        /// <summary>
        /// Display label for this stage.
        /// </summary>
        [ObservableProperty]
        private string _label = string.Empty;

        /// <summary>
        /// Whether this stage is currently active.
        /// </summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>
        /// Whether this stage has been completed.
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted;

        /// <summary>
        /// Progress value for this stage (0-100).
        /// </summary>
        [ObservableProperty]
        private double _progressValue;

        /// <summary>
        /// Gets the icon glyph for this stage.
        /// </summary>
        public string Icon => StageType switch
        {
            ProgressStageType.Analyzing => "\uE721",  // Search
            ProgressStageType.Processing => "\uE72C", // More/Processing
            ProgressStageType.Completing => "\uE73E", // CheckMark
            ProgressStageType.Saved => "\uE74B",      // Save
            _ => "\uE721"
        };

        /// <summary>
        /// Gets the brush color for this stage based on its state.
        /// </summary>
        public Brush Brush => (Application.Current.Resources) switch
        {
            var r when IsCompleted => (Brush)r["SuccessBrush"],
            var r when IsActive => (Brush)r["AccentBrush"],
            _ => (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"]
        };

        /// <summary>
        /// Creates a new ProgressStage with the specified type and label.
        /// </summary>
        public ProgressStage(ProgressStageType stageType, string label)
        {
            StageType = stageType;
            Label = label;
            IsActive = false;
            IsCompleted = false;
            ProgressValue = 0;
        }
    }
}
