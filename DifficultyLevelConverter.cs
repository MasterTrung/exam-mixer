using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DragDropTreeApp
{
    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    public class DifficultyLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DifficultyLevel level)
            {
                switch (level)
                {
                    case DifficultyLevel.Easy: return "Dễ";
                    case DifficultyLevel.Medium: return "TB";
                    case DifficultyLevel.Hard: return "Khó";
                    default: return "TB";
                }
            }
            if (value is int i)
            {
                switch (i)
                {
                    case 1: return "Dễ";
                    case 2: return "TB";
                    case 3: return "Khó";
                }
            }
            return "TB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                switch (strValue)
                {
                    case "Dễ": return DifficultyLevel.Easy;
                    case "TB": return DifficultyLevel.Medium;
                    case "Khó": return DifficultyLevel.Hard;
                    default: return DifficultyLevel.Medium;
                }
            }
            else if (value is ComboBoxItem comboBoxItem && comboBoxItem.Content is string content)
            {
                switch (content)
                {
                    case "Dễ": return DifficultyLevel.Easy;
                    case "TB": return DifficultyLevel.Medium;
                    case "Khó": return DifficultyLevel.Hard;
                    default: return DifficultyLevel.Medium;
                }
            }
            return DifficultyLevel.Medium;
        }
    }
}