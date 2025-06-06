using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DragDropTreeApp
{
    public static class ComboBoxHelper
    {
        // Định nghĩa Attached Property
        public static readonly DependencyProperty PreserveSelectionProperty =
            DependencyProperty.RegisterAttached(
                "PreserveSelection",
                typeof(bool),
                typeof(ComboBoxHelper),
                new PropertyMetadata(false, OnPreserveSelectionChanged));

        public static bool GetPreserveSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(PreserveSelectionProperty);
        }

        public static void SetPreserveSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(PreserveSelectionProperty, value);
        }

        // Dictionary để lưu trữ các giá trị
        private static readonly Dictionary<string, int> _selectionStore = new Dictionary<string, int>();

        private static void OnPreserveSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                if ((bool)e.NewValue)
                {
                    // Đăng ký các handler khi bật tính năng
                    comboBox.SelectionChanged += ComboBox_SelectionChanged;
                    comboBox.Loaded += ComboBox_Loaded;
                    comboBox.Unloaded += ComboBox_Unloaded;
                }
                else
                {
                    // Hủy đăng ký khi tắt
                    comboBox.SelectionChanged -= ComboBox_SelectionChanged;
                    comboBox.Loaded -= ComboBox_Loaded;
                    comboBox.Unloaded -= ComboBox_Unloaded;
                }
            }
        }

        private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var key = GetComboBoxKey(comboBox);
                _selectionStore[key] = comboBox.SelectedIndex;
            }
        }

        private static void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var key = GetComboBoxKey(comboBox);
                if (_selectionStore.TryGetValue(key, out int selectedIndex))
                {
                    comboBox.SelectedIndex = selectedIndex;
                }
            }
        }

        private static void ComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            // Có thể làm gì đó khi ComboBox bị unload
        }

        private static string GetComboBoxKey(ComboBox comboBox)
        {
            // Tạo key dựa trên ComboBox và DataContext của nó
            var dataContext = comboBox.DataContext;
            string dataContextId = "unknown";

            if (dataContext != null)
            {
                if (dataContext is TrueFalseStatementViewModel statement)
                {
                    // Nếu DataContext là một statement, lấy Id của câu hỏi cha
                    var parent = FindParentObject<QuestionViewModel>(comboBox);
                    if (parent != null && parent is QuestionViewModel question)
                    {
                        var index = GetStatementIndex(question, statement);
                        dataContextId = $"{question.Id}_{index}";
                    }
                    else
                    {
                        dataContextId = statement.GetHashCode().ToString();
                    }
                }
                else
                {
                    dataContextId = dataContext.GetHashCode().ToString();
                }
            }

            return $"{dataContextId}";
        }

        // Helper method to find parent in the logical tree
        private static T FindParentObject<T>(DependencyObject child) where T : class
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T result)
                {
                    return result;
                }
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static int GetStatementIndex(QuestionViewModel question, TrueFalseStatementViewModel statement)
        {
            if (question is TrueFalseQuestionViewModel tfQuestion && tfQuestion.Statements != null)
            {
                for (int i = 0; i < tfQuestion.Statements.Count; i++)
                {
                    if (tfQuestion.Statements[i] == statement)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}