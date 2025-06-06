using System;
using System.ComponentModel;

namespace DragDropTreeApp
{
    public class TrueFalseStatementViewModel : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get => _text ?? "";
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private bool _isTrue;
        public bool IsTrue
        {
            get => _isTrue;
            set
            {
                _isTrue = value;
                OnPropertyChanged(nameof(IsTrue));
            }
        }

        private DifficultyLevel? _difficulty;
        public DifficultyLevel? Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                OnPropertyChanged(nameof(Difficulty));
            }
        }



        private bool _userAnswer;
        public bool UserAnswer
        {
            get => _userAnswer;
            set
            {
                _userAnswer = value;
                OnPropertyChanged(nameof(UserAnswer));
            }
        }

        // Constructor
        public TrueFalseStatementViewModel()
        {
            Text = "";
            IsTrue = false;
            Difficulty = DifficultyLevel.Medium;
            UserAnswer = false;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}