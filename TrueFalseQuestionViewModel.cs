using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DragDropTreeApp
{
    public class TrueFalseQuestionViewModel : QuestionViewModel
    {

        private ObservableCollection<ContentElement> _richContent = new ObservableCollection<ContentElement>();
        public ObservableCollection<ContentElement> RichContent
        {
            get => _richContent;
            set
            {
                if (_richContent != value)
                {
                    _richContent = value;
                    OnPropertyChanged(nameof(RichContent));
                }
            }
        }

        // Khai báo các thuộc tính cần thiết
        private ObservableCollection<TrueFalseStatementViewModel> _statements;
        public ObservableCollection<TrueFalseStatementViewModel> Statements
        {
            get => _statements ?? (_statements = new ObservableCollection<TrueFalseStatementViewModel>());
            set
            {
                _statements = value;
                OnPropertyChanged(nameof(Statements));
            }
        }

        // Constructor
        public TrueFalseQuestionViewModel()
        {
            Type = QuestionType.TrueFalse;
            Statements = new ObservableCollection<TrueFalseStatementViewModel>();
        }

        // Override Clone để đảm bảo sao chép đầy đủ
        public override QuestionViewModel Clone()
        {
            var clone = new TrueFalseQuestionViewModel
            {
                Title = this.Title,
                Solution = this.Solution,
                Type = QuestionType.TrueFalse
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
                        Image = el.Image,
                        ImagePath = el.ImagePath
                    });
                }
            }

            // Clone statements
            if (this.Statements != null)
            {
                foreach (var statement in this.Statements)
                {
                    var clonedStatement = new TrueFalseStatementViewModel
                    {
                        Text = statement.Text,
                        IsTrue = statement.IsTrue,
                        Difficulty = statement.Difficulty
                    };
                    clone.Statements.Add(clonedStatement);
                }
            }

            return clone;
        }

        // Phương thức trộn mệnh đề theo độ khó
        public void ShuffleByDifficulty(Random random)
        {
            if (Statements == null || Statements.Count <= 1) return;

            // Nhóm các mệnh đề theo độ khó
            var easyStatements = Statements.Where(s => s.Difficulty == DifficultyLevel.Easy).ToList();
            var mediumStatements = Statements.Where(s => s.Difficulty == DifficultyLevel.Medium).ToList();
            var hardStatements = Statements.Where(s => s.Difficulty == DifficultyLevel.Hard).ToList();

            // Trộn các mệnh đề trong từng nhóm
            ShuffleList(easyStatements, random);
            ShuffleList(mediumStatements, random);
            ShuffleList(hardStatements, random);

            // Tạo danh sách mới với thứ tự đã được trộn
            var shuffledStatements = new ObservableCollection<TrueFalseStatementViewModel>();

            // Thêm theo thứ tự: dễ -> trung bình -> khó
            foreach (var statement in easyStatements)
                shuffledStatements.Add(statement);

            foreach (var statement in mediumStatements)
                shuffledStatements.Add(statement);

            foreach (var statement in hardStatements)
                shuffledStatements.Add(statement);

            // Thay thế danh sách hiện tại bằng danh sách đã trộn
            Statements.Clear();
            foreach (var statement in shuffledStatements)
                Statements.Add(statement);
        }

        // Helper method to shuffle a list
        private void ShuffleList<T>(List<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}