using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DragDropTreeApp
{
    public class MultipleChoiceQuestionViewModel : QuestionViewModel
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

        private ObservableCollection<AnswerViewModel> _answers;
        public ObservableCollection<AnswerViewModel> Answers
        {
            get => _answers ?? (_answers = new ObservableCollection<AnswerViewModel>());
            set
            {
                _answers = value;
                OnPropertyChanged(nameof(Answers));
            }
        }

        // Constructor
        public MultipleChoiceQuestionViewModel()
        {
            Type = QuestionType.MultipleChoice;
            Answers = new ObservableCollection<AnswerViewModel>();
        }

        // Override Clone để đảm bảo sao chép đầy đủ
        public override QuestionViewModel Clone()
        {
            // Tạo instance mới
            var clone = new MultipleChoiceQuestionViewModel
            {
                Title = this.Title,
                Solution = this.Solution,
                Type = QuestionType.MultipleChoice
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
                        Image = el.Image, // Nếu Image là BitmapImage, có thể dùng chung, nếu cần clone sâu hơn thì xử lý thêm
                        ImagePath = el.ImagePath
                    });
                }
            }

            // Debug
            Console.WriteLine($"Cloning MC Question: {this.Title} with {this.Answers?.Count ?? 0} answers");

            // Clone answers
            clone.Answers = new ObservableCollection<AnswerViewModel>();
            if (this.Answers != null)
            {
                foreach (var answer in this.Answers)
                {
                    var clonedAnswer = new AnswerViewModel
                    {
                        Text = answer.Text,
                        IsCorrect = answer.IsCorrect,
                        GroupName = answer.GroupName
                    };
                    clone.Answers.Add(clonedAnswer);
                    Console.WriteLine($"  - Cloned answer: {clonedAnswer.Text}");
                }
            }

            return clone;
        }

        // Phương thức trộn các phương án
        public void ShuffleAnswers(Random random)
        {
            if (Answers == null || Answers.Count <= 1) return;

            var answersList = Answers.ToList();

            int n = answersList.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                var temp = answersList[k];
                answersList[k] = answersList[n];
                answersList[n] = temp;
            }

            Answers.Clear();
            foreach (var answer in answersList)
            {
                Answers.Add(answer);
            }
        }
    }
}