using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DragDropTreeApp
{
    public class ShortAnswerQuestionViewModel : QuestionViewModel
    {

        private ObservableCollection<ContentElement> _richContent = new ObservableCollection<ContentElement>();
        public ObservableCollection<ContentElement> RichContent
        {
            get => _richContent;
            set
            {
                _richContent = value;
                OnPropertyChanged(nameof(RichContent));
            }
        }

        // Đổi từ Answer thành ShortAnswer
        private string _shortAnswer;
        public string ShortAnswer
        {
            get => _shortAnswer;
            set
            {
                _shortAnswer = value;
                OnPropertyChanged(nameof(ShortAnswer));
            }
        }

        public ShortAnswerQuestionViewModel()
        {
            Type = QuestionType.ShortAnswer;
        }

        public override QuestionViewModel Clone()
        {
            var clone = new ShortAnswerQuestionViewModel
            {
                Title = this.Title,
                Solution = this.Solution,
                ShortAnswer = this.ShortAnswer
            };

            // Clone sâu RichContent
            foreach (var el in this.RichContent)
            {
                clone.RichContent.Add(new ContentElement
                {
                    Type = el.Type,
                    Text = el.Text,
                    Image = el.Image,
                    ImagePath = el.ImagePath
                });
            }

            return clone;
        }
    }
}