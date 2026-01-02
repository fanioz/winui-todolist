using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace todolist.Converters
{
    /// <summary>
    /// Converts a boolean value to Visibility, with inversion.
    /// True = Collapsed, False = Visible (opposite of standard converter).
    /// </summary>
    public class BoolToVisibilityInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Standard bool to visibility converter.
    /// True = Visible, False = Collapsed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts completed status to opacity (completed = 0.6, not completed = 1.0).
    /// </summary>
    public class CompletedOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isCompleted)
            {
                return isCompleted ? 0.6 : 1.0;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts completed status to text decorations (completed = Strikethrough, not completed = None).
    /// </summary>
    public class CompletedTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isCompleted && isCompleted)
            {
                return Windows.UI.Text.TextDecorations.Strikethrough;
            }
            return Windows.UI.Text.TextDecorations.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
