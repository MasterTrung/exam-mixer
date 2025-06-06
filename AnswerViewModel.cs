using System.ComponentModel;

namespace DragDropTreeApp
{
    public class AnswerViewModel : INotifyPropertyChanged
    {
        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        // Thêm thuộc tính Text để tương thích
        public string Text
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        private bool _isCorrect;
        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                if (_isCorrect != value)
                {
                    _isCorrect = value;
                    OnPropertyChanged(nameof(IsCorrect));
                }
            }
        }

        // Thêm thuộc tính IsSelected để theo dõi lựa chọn người dùng
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        // Thêm thuộc tính GroupName cho các câu hỏi radio button
        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    OnPropertyChanged(nameof(GroupName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}