using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DragDropTreeApp
{
    public class ExamBlockViewModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _isFixed;
        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                _isFixed = value;
                OnPropertyChanged(nameof(IsFixed));
            }
        }

        private ObservableCollection<QuestionViewModel> _questions;
        public ObservableCollection<QuestionViewModel> Questions
        {
            get => _questions ?? (_questions = new ObservableCollection<QuestionViewModel>());
            set
            {
                _questions = value;
                OnPropertyChanged(nameof(Questions));
            }
        }

        // Constructor
        public ExamBlockViewModel()
        {
            Questions = new ObservableCollection<QuestionViewModel>();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}