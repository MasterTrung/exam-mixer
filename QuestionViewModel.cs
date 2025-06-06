using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;



namespace DragDropTreeApp
{
   

    public class AnswerOption : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set { _groupName = value; OnPropertyChanged(nameof(GroupName)); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Statement : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        private bool _userAnswer;
        public bool UserAnswer
        {
            get => _userAnswer;
            set { _userAnswer = value; OnPropertyChanged(nameof(UserAnswer)); }
        }

        private int _difficulty = 1;
        public int Difficulty
        {
            get => _difficulty;
            set { _difficulty = value; OnPropertyChanged(nameof(Difficulty)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OptionItem : INotifyPropertyChanged
    {
        private string _label;
        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        private ObservableCollection<ContentElement> _content = new ObservableCollection<ContentElement>();
        public ObservableCollection<ContentElement> Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public void AddText(string text)
        {
            Content.Add(new ContentElement { Type = "text", Text = text });
        }

        public void AddImage(BitmapImage image, string path = null)
        {
            Content.Add(new ContentElement { Type = "image", Image = image, ImagePath = path });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //public enum QuestionType
    //{
    //    MultipleChoice,
    //    TrueFalse,
    //    ShortAnswer,
    //    Essay
    //}

    public class QuestionViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private string _content;
        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        private ObservableCollection<ContentElement> _richContent = new ObservableCollection<ContentElement>();
        public ObservableCollection<ContentElement> RichContent
        {
            get => _richContent;
            set { _richContent = value; OnPropertyChanged(nameof(RichContent)); }
        }

        private ObservableCollection<OptionItem> _options = new ObservableCollection<OptionItem>();
        public ObservableCollection<OptionItem> Options
        {
            get => _options;
            set { _options = value; OnPropertyChanged(nameof(Options)); }
        }

        private ObservableCollection<AnswerOption> _answers = new ObservableCollection<AnswerOption>();
        public ObservableCollection<AnswerOption> Answers
        {
            get => _answers;
            set { _answers = value; OnPropertyChanged(nameof(Answers)); }
        }

        private ObservableCollection<Statement> _statements = new ObservableCollection<Statement>();
        public ObservableCollection<Statement> Statements
        {
            get => _statements;
            set { _statements = value; OnPropertyChanged(nameof(Statements)); }
        }

        private string _solution;
        public string Solution
        {
            get => _solution;
            set { _solution = value; OnPropertyChanged(nameof(Solution)); }
        }

        private ObservableCollection<ContentElement> _richSolution = new ObservableCollection<ContentElement>();
        public ObservableCollection<ContentElement> RichSolution
        {
            get => _richSolution;
            set { _richSolution = value; OnPropertyChanged(nameof(RichSolution)); }
        }

        private QuestionType _type = QuestionType.MultipleChoice;
        public QuestionType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public bool IsImportedFromWord { get; set; } = false;

        // Đồng bộ Answers dựa trên Options (cho trắc nghiệm)
        public void SyncAnswersFromOptions()
        {
            Answers.Clear();
            string groupName = "group_" + Id.ToString("N");
            foreach (var option in Options)
            {
                string text = string.Join("\n", option.Content?.Where(x => x.Type == "text").Select(x => x.Text));
                Answers.Add(new AnswerOption
                {
                    Text = string.IsNullOrWhiteSpace(option.Label) ? text : $"{option.Label}. {text}",
                    GroupName = groupName,
                    IsSelected = false
                });
            }
        }

        // Đồng bộ Statements dựa trên Options (cho đúng/sai)
        public void SyncStatementsFromOptions()
        {
            Statements.Clear();
            foreach (var option in Options)
            {
                string text = string.Join("\n", option.Content?.Where(x => x.Type == "text").Select(x => x.Text));
                // Nếu bạn có trường nào thể hiện đúng/sai, có thể bổ sung logic nhận biết ở đây.
                // Ví dụ: option.Label == "Đ" => đúng, "S" => sai, hoặc một trường riêng.
                Statements.Add(new Statement
                {
                    Text = string.IsNullOrWhiteSpace(option.Label) ? text : $"{option.Label}. {text}",
                    UserAnswer = false, // mặc định chưa chọn
                    Difficulty = 1     // mặc định dễ, có thể sửa
                });
            }
        }

        public void AddTextContent(string text)
        {
            RichContent.Add(new ContentElement { Type = "text", Text = text });
        }
        public void AddImageContent(BitmapImage image, string path = null)
        {
            RichContent.Add(new ContentElement { Type = "image", Image = image, ImagePath = path });
        }
        public void AddTextSolution(string text)
        {
            RichSolution.Add(new ContentElement { Type = "text", Text = text });
        }
        public void AddImageSolution(BitmapImage image, string path = null)
        {
            RichSolution.Add(new ContentElement { Type = "image", Image = image, ImagePath = path });
        }

        public virtual QuestionViewModel Clone()
        {
            var clone = new QuestionViewModel
            {
                Title = this.Title,
                Content = this.Content,
                Solution = this.Solution,
                Type = this.Type,
                IsImportedFromWord = this.IsImportedFromWord
            };

            foreach (var content in this.RichContent)
            {
                clone.RichContent.Add(new ContentElement
                {
                    Type = content.Type,
                    Text = content.Text,
                    Image = content.Image,
                    ImagePath = content.ImagePath
                });
            }

            foreach (var option in this.Options)
            {
                var clonedOption = new OptionItem { Label = option.Label };
                foreach (var content in option.Content)
                {
                    clonedOption.Content.Add(new ContentElement
                    {
                        Type = content.Type,
                        Text = content.Text,
                        Image = content.Image,
                        ImagePath = content.ImagePath
                    });
                }
                clone.Options.Add(clonedOption);
            }

            foreach (var answer in this.Answers)
            {
                clone.Answers.Add(new AnswerOption
                {
                    Text = answer.Text,
                    GroupName = answer.GroupName,
                    IsSelected = answer.IsSelected
                });
            }

            foreach (var statement in this.Statements)
            {
                clone.Statements.Add(new Statement
                {
                    Text = statement.Text,
                    UserAnswer = statement.UserAnswer,
                    Difficulty = statement.Difficulty
                });
            }

            foreach (var solution in this.RichSolution)
            {
                clone.RichSolution.Add(new ContentElement
                {
                    Type = solution.Type,
                    Text = solution.Text,
                    Image = solution.Image,
                    ImagePath = solution.ImagePath
                });
            }

            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public FlowDocument BuildFlowDocumentFromRichContent(IEnumerable<ContentElement> richContent)
        {
            var doc = new FlowDocument();
            var para = new Paragraph();

            foreach (var item in richContent)
            {
                if (item.Type == "text" && !string.IsNullOrWhiteSpace(item.Text))
                {
                    para.Inlines.Add(new Run(item.Text));
                    para.Inlines.Add(new LineBreak());
                }
                else if (item.Type == "image" && item.Image != null)
                {
                    var img = new Image
                    {
                        Source = item.Image,
                        MaxHeight = 220,
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    para.Inlines.Add(new InlineUIContainer(img));
                    para.Inlines.Add(new LineBreak());
                }
            }
            doc.Blocks.Add(para);
            return doc;
        }

    }

    public class QuestionsCollection : INotifyPropertyChanged
    {
        private ObservableCollection<QuestionViewModel> _questions = new ObservableCollection<QuestionViewModel>();
        public ObservableCollection<QuestionViewModel> Questions
        {
            get => _questions;
            set { _questions = value; OnPropertyChanged(nameof(Questions)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}