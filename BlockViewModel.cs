using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Input;  // Cho ICommand
using System.Reflection;     // Cho AssemblyName 
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;

namespace DragDropTreeApp
{
    public class BlockViewModel : INotifyPropertyChanged
    {
        // Thêm thuộc tính IsExpanded
        private bool _isExpanded = true; // Mặc định là true để hiển thị tất cả các khối con
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }


        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        private ICommand _startEditCommand;
        public ICommand StartEditCommand => _startEditCommand ??= new RelayCommand(_ => IsEditing = true);

        private ICommand _endEditCommand;
        public ICommand EndEditCommand => _endEditCommand ??= new RelayCommand(_ => IsEditing = false);

        private static int _idCounter = 0;

        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        private bool _isFixed;
        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                if (_isFixed != value)
                {
                    _isFixed = value;
                    OnPropertyChanged(nameof(IsFixed));
                    OnPropertyChanged(nameof(BlockType));
                    OnPropertyChanged(nameof(BlockColor));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        // Trạng thái đang chỉnh sửa tên
        private bool _isRenaming;
        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                if (_isRenaming != value)
                {
                    _isRenaming = value;
                    OnPropertyChanged(nameof(IsRenaming));
                }
            }
        }

        // Tên tạm thời khi đang chỉnh sửa
        private string _tempName;
        public string TempName
        {
            get => _tempName;
            set
            {
                if (_tempName != value)
                {
                    _tempName = value;
                    OnPropertyChanged(nameof(TempName));
                }
            }
        }

        private BlockViewModel _parent;
        public BlockViewModel Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        // Danh sách các khối con
        private ObservableCollection<BlockViewModel> _children;
        public ObservableCollection<BlockViewModel> Children
        {
            get => _children ?? (_children = new ObservableCollection<BlockViewModel>());
            set
            {
                if (_children != value)
                {
                    _children = value;
                    OnPropertyChanged(nameof(Children));
                }
            }
        }

        public string DisplayName => $"{Name} ({(IsFixed ? "Khi trộn: Giữ nguyên thứ tự" : "Khi trộn: Đảo thứ tự")})";

        public string BlockType => IsFixed ? "Khối Cố Định" : "Khối Linh Động";

        public Brush BlockColor => IsFixed ? Brushes.SkyBlue : Brushes.LightGreen;

        private ObservableCollection<QuestionViewModel> _questions;
        public ObservableCollection<QuestionViewModel> Questions
        {
            get => _questions ?? (_questions = new ObservableCollection<QuestionViewModel>());
            set
            {
                if (_questions != value)
                {
                    _questions = value;
                    OnPropertyChanged(nameof(Questions));
                }
            }
        }

        public BlockViewModel(bool isFixed = false, string name = null)
        {
            Id = Guid.NewGuid().ToString();
            Name = name ?? $"{(isFixed ? "Khối cố định" : "Khối linh động")}";
            IsFixed = isFixed;
            Children = new ObservableCollection<BlockViewModel>();
            Questions = new ObservableCollection<QuestionViewModel>();
            IsExpanded = true; // Đặt mặc định là mở rộng

            // Khởi tạo commands
            RenameCommand = new RelayCommand(_ => StartRenaming());
            DeleteCommand = new RelayCommand(_ => OnRequestDelete());
        }

        // Khai báo Commands
        private ICommand _renameCommand;
        public ICommand RenameCommand
        {
            get => _renameCommand;
            set
            {
                _renameCommand = value;
                OnPropertyChanged(nameof(RenameCommand));
            }
        }

        // Bắt đầu quá trình đổi tên
        public void StartRenaming()
        {
            TempName = Name;
            IsRenaming = true;
        }

        // Hoàn thành quá trình đổi tên
        public void FinishRenaming()
        {
            if (!string.IsNullOrEmpty(TempName))
            {
                Name = TempName;
            }
            IsRenaming = false;
        }

        // Hủy quá trình đổi tên
        public void CancelRenaming()
        {
            IsRenaming = false;
        }

        // Sự kiện khi yêu cầu xóa khối
        public event EventHandler DeleteRequested;

        // Yêu cầu xóa khối
        public void RequestDelete()
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Trả về danh sách câu hỏi cho việc trộn đề theo đúng quy tắc cố định/linh động
        /// </summary>
        public ObservableCollection<QuestionViewModel> GetShuffledQuestions(Random random)
        {
            // Tạo bản sao để không ảnh hưởng đến danh sách gốc
            var result = new ObservableCollection<QuestionViewModel>();

            if (IsFixed)
            {
                // Nếu là khối cố định, copy nguyên thứ tự
                foreach (var question in Questions)
                {
                    result.Add(question);
                }
            }
            else
            {
                // Nếu là khối linh động, trộn thứ tự câu hỏi
                var shuffledList = Questions.OrderBy(q => random.Next()).ToList();
                foreach (var question in shuffledList)
                {
                    result.Add(question);
                }
            }

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get => _deleteCommand;
            set
            {
                _deleteCommand = value;
                OnPropertyChanged(nameof(DeleteCommand));
            }
        }

        private ICommand _keyDownCommand;
        public ICommand KeyDownCommand => _keyDownCommand ??= new RelayCommand(HandleKeyDown);

        // Phương thức xử lý xóa khối
        private void OnRequestDelete()
        {
            // Hiển thị hộp thoại xác nhận
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khối '{Name}' không?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Gọi RequestDelete để kích hoạt event
                RequestDelete();
            }
        }

        // Phương thức xử lý phím Enter
        private void HandleKeyDown(object parameter)
        {
            if (parameter is KeyEventArgs e && e.Key == Key.Enter)
            {
                IsEditing = false;
                e.Handled = true;
            }
        }
    }
}