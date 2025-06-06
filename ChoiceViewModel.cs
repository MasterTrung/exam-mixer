using System.ComponentModel;

namespace DragDropTreeApp
{
    public class ChoiceViewModel : INotifyPropertyChanged
    {
        //private string _content;
        //public string Content
        //{
        //    get => _content;
        //    set
        //    {
        //        if (_content != value)
        //        {
        //            _content = value;
        //            OnPropertyChanged(nameof(Content));
        //        }
        //    }
        //}

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

        // Thêm thuộc tính IsSelected
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}