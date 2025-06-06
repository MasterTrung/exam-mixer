using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DragDropTreeApp
{
    public class EssayQuestionViewModel : QuestionViewModel
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

        // Đổi từ Answer thành EssayAnswer
        private string _essayAnswer;
        public string EssayAnswer
        {
            get => _essayAnswer;
            set
            {
                _essayAnswer = value;
                OnPropertyChanged(nameof(EssayAnswer));
            }
        }

        private int _essayMaxLength = 1000;
        public int EssayMaxLength
        {
            get => _essayMaxLength;
            set
            {
                _essayMaxLength = value;
                OnPropertyChanged(nameof(EssayMaxLength));
            }
        }

        public EssayQuestionViewModel()
        {
            Type = QuestionType.Essay;
        }

        public override QuestionViewModel Clone()
        {
            var clone = new EssayQuestionViewModel
            {
                Title = this.Title,
                Solution = this.Solution,
                EssayAnswer = this.EssayAnswer,
                EssayMaxLength = this.EssayMaxLength
            };

            // Clone sâu RichContent
            if (this.RichContent != null)
            {
                foreach (var el in this.RichContent)
                {
                    clone.RichContent.Add(new ContentElement
                    {
                        Type = el.Type,
                        Text = el.Text,
                        Image = el.Image, // Nếu Image là BitmapImage, có thể dùng chung (immutable)
                        ImagePath = el.ImagePath
                    });
                }
            }

            return clone;
        }
    }
}