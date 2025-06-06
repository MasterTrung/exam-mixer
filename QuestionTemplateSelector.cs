using System;
using System.Windows;
using System.Windows.Controls;

namespace DragDropTreeApp
{
    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MultipleChoiceTemplate { get; set; }
        public DataTemplate TrueFalseTemplate { get; set; }
        public DataTemplate ShortAnswerTemplate { get; set; }
        public DataTemplate EssayTemplate { get; set; }

        // Thêm template mới cho câu hỏi nhập từ Word
        public DataTemplate ImportedFromWordTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is QuestionViewModel question)
            {
                // Ưu tiên kiểm tra xem câu hỏi có được import từ Word không
                if (question.IsImportedFromWord && ImportedFromWordTemplate != null)
                {
                    return ImportedFromWordTemplate;
                }

                // Nếu không, sử dụng template dựa trên loại câu hỏi
                switch (question.Type)
                {
                    case QuestionType.MultipleChoice:
                        return MultipleChoiceTemplate;
                    case QuestionType.TrueFalse:
                        return TrueFalseTemplate;
                    case QuestionType.ShortAnswer:
                        return ShortAnswerTemplate;
                    case QuestionType.Essay:
                        return EssayTemplate;
                    default:
                        return MultipleChoiceTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}