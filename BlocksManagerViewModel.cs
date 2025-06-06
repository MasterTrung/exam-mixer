using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace DragDropTreeApp
{
    public class BlocksManagerViewModel : INotifyPropertyChanged
    {
        // Thêm command mới cho tạo khối mẫu
        public ICommand CreateTemplateCommand { get; private set; }


        // Stack lưu trữ lịch sử các thao tác để hoàn tác
        private Stack<UndoAction> _undoStack = new Stack<UndoAction>();
        private Stack<DeletedBlockInfo> _deletedBlocksStack = new Stack<DeletedBlockInfo>();
        private const int MaxUndoLevels = 20; // Giới hạn số lần hoàn tác tối đa

        // Bộ đếm để đánh số cho các khối mới
        private int _fixedBlockCounter = 0;
        private int _dynamicBlockCounter = 0;

        // Collection hiển thị danh sách tất cả khối theo cấu trúc phẳng
        private ObservableCollection<BlockViewModel> _blocks;
        public ObservableCollection<BlockViewModel> Blocks
        {
            get => _blocks;
            set
            {
                _blocks = value;
                OnPropertyChanged(nameof(Blocks));
            }
        }

        // Collection hiển thị danh sách khối gốc (cấu trúc phân cấp)
        private ObservableCollection<BlockViewModel> _rootBlocks;
        public ObservableCollection<BlockViewModel> RootBlocks
        {
            get => _rootBlocks;
            set
            {
                _rootBlocks = value;
                OnPropertyChanged(nameof(RootBlocks));
            }
        }

        // Khối được chọn hiện tại
        private BlockViewModel _selectedBlock;
        public BlockViewModel SelectedBlock
        {
            get => _selectedBlock;
            set
            {
                if (_selectedBlock != value)
                {
                    _selectedBlock = value;
                    OnPropertyChanged(nameof(SelectedBlock));
                }
            }
        }

       



        // Constructor
        public BlocksManagerViewModel()
        {
            // Khởi tạo các collection
            _rootBlocks = new ObservableCollection<BlockViewModel>();
            _blocks = new ObservableCollection<BlockViewModel>();

            // Khởi tạo các command
            AddFixedBlockCommand = new RelayCommand(_ => AddBlock(true));
            AddDynamicBlockCommand = new RelayCommand(_ => AddBlock(false));
            UndoDeleteCommand = new RelayCommand(_ => UndoDelete(), _ => CanUndo);


            // Thêm command mới
            CreateTemplateCommand = new RelayCommand(_ => CreateTemplateStructure());
        }


        private void CreateTemplateStructure()
        {
            // Tạo khối gốc FULL
            BlockViewModel fullBlock = new BlockViewModel(true, "FULL");
            fullBlock.DeleteRequested += Block_DeleteRequested;
            fullBlock.IsExpanded = true;

            // Tạo khối ABCD
            BlockViewModel abcdBlock = new BlockViewModel(true, "ABCD");
            abcdBlock.DeleteRequested += Block_DeleteRequested;
            abcdBlock.Parent = fullBlock;
            abcdBlock.IsExpanded = true;
            fullBlock.Children.Add(abcdBlock);

            // Thêm các khối con cho ABCD
            AddChildBlocks(abcdBlock);

            // Tạo khối DS
            BlockViewModel dsBlock = new BlockViewModel(true, "DS");
            dsBlock.DeleteRequested += Block_DeleteRequested;
            dsBlock.Parent = fullBlock;
            dsBlock.IsExpanded = true;
            fullBlock.Children.Add(dsBlock);

            // Thêm các khối con cho DS
            AddChildBlocks(dsBlock);

            // Tạo khối TLN
            BlockViewModel tlnBlock = new BlockViewModel(true, "TLN");
            tlnBlock.DeleteRequested += Block_DeleteRequested;
            tlnBlock.Parent = fullBlock;
            tlnBlock.IsExpanded = true;
            fullBlock.Children.Add(tlnBlock);

            // Thêm các khối con cho TLN
            AddChildBlocks(tlnBlock);

            // Tạo khối TL
            BlockViewModel tlBlock = new BlockViewModel(true, "TL");
            tlBlock.DeleteRequested += Block_DeleteRequested;
            tlBlock.Parent = fullBlock;
            tlBlock.IsExpanded = true;
            fullBlock.Children.Add(tlBlock);

            // Thêm các khối con cho TL
            AddChildBlocks(tlBlock);

            // Thêm khối FULL vào danh sách gốc
            _rootBlocks.Add(fullBlock);

            // Thêm tất cả các khối vào danh sách phẳng
            AddToFlatList(fullBlock);

            // Làm mới TreeView để áp dụng sự mở rộng
            RefreshTreeViewExpansionState();

            // Thông báo thay đổi
            OnPropertyChanged(nameof(RootBlocks));
            OnPropertyChanged(nameof(Blocks));
        }

        // Thêm phương thức để làm mới trạng thái mở rộng của TreeView
        private void RefreshTreeViewExpansionState()
        {
            foreach (var block in _blocks)
            {
                block.IsExpanded = true;
            }
        }


        private void AddChildBlocks(BlockViewModel parentBlock)
        {
            // Tạo khối con NB (Khối linh động)
            BlockViewModel nbBlock = new BlockViewModel(false, "NB");
            nbBlock.DeleteRequested += Block_DeleteRequested;
            nbBlock.Parent = parentBlock;
            nbBlock.IsExpanded = true;
            parentBlock.Children.Add(nbBlock);

            // Tạo khối con TH (Khối linh động)
            BlockViewModel thBlock = new BlockViewModel(false, "TH");
            thBlock.DeleteRequested += Block_DeleteRequested;
            thBlock.Parent = parentBlock;
            thBlock.IsExpanded = true;
            parentBlock.Children.Add(thBlock);

            // Tạo khối con VD (Khối linh động)
            BlockViewModel vdBlock = new BlockViewModel(false, "VD");
            vdBlock.DeleteRequested += Block_DeleteRequested;
            vdBlock.Parent = parentBlock;
            vdBlock.IsExpanded = true;
            parentBlock.Children.Add(vdBlock);
        }

        // Phương thức đệ quy để thêm tất cả các khối vào danh sách phẳng
        private void AddToFlatList(BlockViewModel block)
        {
            _blocks.Add(block);

            foreach (var child in block.Children)
            {
                AddToFlatList(child);
            }
        }


        // Properties cho commands
        public ICommand AddFixedBlockCommand { get; private set; }
        public ICommand AddDynamicBlockCommand { get; private set; }
        public ICommand UndoDeleteCommand { get; private set; }

        private void AddBlock(bool isFixed)
        {
            // Tăng bộ đếm tương ứng
            if (isFixed)
                _fixedBlockCounter++;
            else
                _dynamicBlockCounter++;

            // Tạo tên khối mới
            string name = isFixed
                ? $"Khối cố định {_fixedBlockCounter}"
                : $"Khối linh động {_dynamicBlockCounter}";

            // Tạo khối mới
            var newBlock = new BlockViewModel
            {
                Name = name,
                IsFixed = isFixed,
                IsExpanded = true // Đảm bảo khối mới tạo cũng được mở rộng
            };

            // Đăng ký sự kiện xóa khối
            newBlock.DeleteRequested += Block_DeleteRequested;

            // Xác định vị trí thêm khối mới
            if (SelectedBlock != null)
            {
                // Thiết lập cha cho khối mới
                newBlock.Parent = SelectedBlock;

                // Thêm vào danh sách con của khối cha
                SelectedBlock.Children.Add(newBlock);

                // Đảm bảo khối cha được mở rộng để hiển thị khối con mới
                SelectedBlock.IsExpanded = true;
            }
            else
            {
                // Thêm như khối gốc (không có cha)
                _rootBlocks.Add(newBlock);
            }

            // Thêm vào danh sách phẳng để dễ quản lý
            _blocks.Add(newBlock);

            // Lưu hành động để hoàn tác
            SaveUndoAction(UndoActionType.AddBlock, newBlock);

            // Thông báo sự thay đổi
            OnPropertyChanged(nameof(RootBlocks));
            OnPropertyChanged(nameof(Blocks));
        }

        // Xử lý sự kiện xóa khối
        private void Block_DeleteRequested(object sender, EventArgs e)
        {
            var block = sender as BlockViewModel;
            if (block == null) return;

            // Thu thập tất cả câu hỏi từ khối và con cháu
            List<QuestionViewModel> recyclableQuestions = new List<QuestionViewModel>();
            CollectQuestionsFromBlock(block, recyclableQuestions);

            // Lưu thông tin chi tiết về khối bị xóa
            var deletedInfo = new DeletedBlockInfo
            {
                Block = block,
                Parent = block.Parent,
                Questions = recyclableQuestions,
                Index = block.Parent?.Children?.IndexOf(block) ?? _rootBlocks.IndexOf(block)
            };

            // Xóa khối khỏi cấu trúc cây
            if (block.Parent != null)
            {
                block.Parent.Children.Remove(block);
            }
            else
            {
                _rootBlocks.Remove(block);
            }

            // Xóa khối khỏi danh sách phẳng
            _blocks.Remove(block);

            // Cập nhật SelectedBlock nếu cần
            if (SelectedBlock == block)
            {
                SelectedBlock = null;
            }

            // Lưu thông tin để hoàn tác
            _deletedBlocksStack.Push(deletedInfo);
            CanUndo = _deletedBlocksStack.Any();

            // Thông báo để trả câu hỏi về Panel 1
            if (recyclableQuestions.Any())
            {
                QuestionsRecycled?.Invoke(this, recyclableQuestions);
            }

            // Thông báo sự thay đổi
            OnPropertyChanged(nameof(RootBlocks));
            OnPropertyChanged(nameof(Blocks));
        }

        // Thu thập đệ quy tất cả câu hỏi từ khối và con cháu
        private void CollectQuestionsFromBlock(BlockViewModel block, List<QuestionViewModel> questions)
        {
            // Thu thập câu hỏi từ khối
            foreach (var question in block.Questions.ToList())
            {
                questions.Add(question);
            }

            // Thu thập câu hỏi từ các khối con (đệ quy)
            if (block.Children != null)
            {
                foreach (var childBlock in block.Children.ToList())
                {
                    CollectQuestionsFromBlock(childBlock, questions);
                }
            }
        }

        // Thông tin về khối đã xóa
        private class DeletedBlockInfo
        {
            public BlockViewModel Block { get; set; }
            public BlockViewModel Parent { get; set; }
            public int Index { get; set; }
            public List<QuestionViewModel> Questions { get; set; }
        }

        // Hoàn tác việc xóa khối
        private void UndoDelete()
        {
            if (!_deletedBlocksStack.Any()) return;

            var deletedInfo = _deletedBlocksStack.Pop();

            // Khôi phục vị trí trong cấu trúc cây
            if (deletedInfo.Parent != null)
            {
                // Là khối con
                int insertIndex = Math.Min(deletedInfo.Index, deletedInfo.Parent.Children.Count);
                deletedInfo.Parent.Children.Insert(insertIndex, deletedInfo.Block);
            }
            else
            {
                // Là khối gốc
                int insertIndex = Math.Min(deletedInfo.Index, _rootBlocks.Count);
                _rootBlocks.Insert(insertIndex, deletedInfo.Block);
            }

            // Thêm lại vào danh sách phẳng
            _blocks.Add(deletedInfo.Block);

            // Chọn lại khối vừa khôi phục
            SelectedBlock = deletedInfo.Block;

            // Thông báo khôi phục câu hỏi vào khối
            if (deletedInfo.Questions.Any())
            {
                QuestionsRestored?.Invoke(this, deletedInfo.Questions);
            }

            // Cập nhật trạng thái hoàn tác
            CanUndo = _deletedBlocksStack.Any();

            // Thông báo sự thay đổi
            OnPropertyChanged(nameof(RootBlocks));
            OnPropertyChanged(nameof(Blocks));
        }

        // Phương thức lưu hành động để hoàn tác
        private void SaveUndoAction(UndoActionType actionType, BlockViewModel block, int index = -1)
        {
            _undoStack.Push(new UndoAction
            {
                ActionType = actionType,
                Block = block,
                Index = index
            });

            // Giới hạn kích thước stack
            if (_undoStack.Count > MaxUndoLevels)
            {
                // Loại bỏ action cũ nhất
                var tempStack = new Stack<UndoAction>();
                for (int i = 0; i < MaxUndoLevels - 1; i++)
                {
                    tempStack.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    _undoStack.Push(tempStack.Pop());
                }
            }

            // Cập nhật trạng thái có thể hoàn tác
            CanUndo = _undoStack.Count > 0 || _deletedBlocksStack.Count > 0;
        }

        // Phương thức lưu hành động di chuyển câu hỏi
        public void SaveQuestionMoveForUndo(ObservableCollection<QuestionViewModel> sourceCollection,
                                          ObservableCollection<QuestionViewModel> destCollection,
                                          QuestionViewModel question, int sourceIndex)
        {
            _undoStack.Push(new UndoAction
            {
                ActionType = UndoActionType.MoveQuestion,
                SourceCollection = sourceCollection,
                DestinationCollection = destCollection,
                Question = question,
                Index = sourceIndex
            });

            // Cập nhật trạng thái có thể hoàn tác
            CanUndo = _undoStack.Count > 0 || _deletedBlocksStack.Count > 0;
        }

        // Sự kiện để thông báo khi cần trả câu hỏi về Panel 1
        public event EventHandler<List<QuestionViewModel>> QuestionsRecycled;

        // Sự kiện để thông báo khi cần khôi phục câu hỏi từ Panel 1 về khối
        public event EventHandler<List<QuestionViewModel>> QuestionsRestored;

        // Property để kiểm tra có thể hoàn tác không
        private bool _canUndo;
        public bool CanUndo
        {
            get => _canUndo;
            set
            {
                if (_canUndo != value)
                {
                    _canUndo = value;
                    OnPropertyChanged(nameof(CanUndo));
                }
            }
        }

        // Triển khai INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Lớp lưu thông tin hành động để hoàn tác
    public class UndoAction
    {
        public UndoActionType ActionType { get; set; }
        public BlockViewModel Block { get; set; }
        public int Index { get; set; }
        public QuestionViewModel Question { get; set; }
        public ObservableCollection<QuestionViewModel> SourceCollection { get; set; }
        public ObservableCollection<QuestionViewModel> DestinationCollection { get; set; }
    }

    // Enum định nghĩa các loại hành động
    public enum UndoActionType
    {
        AddBlock,
        DeleteBlock,
        MoveQuestion
    }
}