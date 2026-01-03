using CommunityToolkit.Mvvm.ComponentModel;
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
        /// Returns null in non-UI contexts (e.g., unit tests).
        /// Uses reflection to avoid UI type dependencies in test projects.
        /// </summary>
        public object? Brush
        {
            get
            {
                try
                {
                    var appType = System.Type.GetType("Microsoft.UI.Xaml.Application, Microsoft.WinUI.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null");
                    if (appType == null) return null;

                    var currentProp = appType.GetProperty("Current");
                    if (currentProp == null) return null;

                    var current = currentProp.GetValue(null);
                    if (current == null) return null;

                    var resourcesProp = current.GetType().GetProperty("Resources");
                    if (resourcesProp == null) return null;

                    var resources = resourcesProp.GetValue(current);
                    if (resources == null) return null;

                    var indexGetter = resources.GetType().GetProperty("Item");
                    if (indexGetter == null) return null;

                    string brushKey = IsCompleted ? "SuccessBrush" :
                                     IsActive ? "AccentBrush" :
                                     "TextFillColorTertiaryBrush";

                    return indexGetter.GetValue(resources, new object[] { brushKey });
                }
                catch
                {
                    return null;
                }
            }
        }

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
