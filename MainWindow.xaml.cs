using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media;
using System.Windows.Documents;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using Microsoft.Office.Interop.Word;
using System.Collections.ObjectModel;
using Point = System.Windows.Point;
using Window = System.Windows.Window;
using Border = System.Windows.Controls.Border;
using Application = System.Windows.Application;
using Table = Microsoft.Office.Interop.Word.Table;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;

namespace DragDropTreeApp
{
    public partial class MainWindow : Window
    {

        // Ở đầu class
        private static Dictionary<Guid, Dictionary<string, object>> _globalQuestionProperties =
            new Dictionary<Guid, Dictionary<string, object>>();

        // Thêm vào đầu class MainWindow
        private static Dictionary<string, object> _tempComboBoxValues = new Dictionary<string, object>();


        // Các biến cơ bản cho kéo thả
        private Point _startPoint;
        private bool _isDragging;
        private QuestionViewModel _draggedItem;
        private ListBox _dragSourceListBox;
        private MainViewModel _viewModel;


        private Point _dragStartPoint;
        // Khai báo biến toàn cục cho adorner
        private DragAdorner _dragAdorner;
        private DropTargetAdorner _dropTargetAdorner;
        private AdornerLayer _adornerLayer;
        private ContentPresenter _draggedPresenter;



        // Bổ sung tham chiếu đến hai ListBox
        private ListBox listBox1;
        private ListBox listBox2;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            //DataContext = new MainViewModel();
        }

        #region Câu hỏi ListBox Drag & Drop


        // BƯỚC 1: Xử lý khi người dùng nhấn chuột xuống
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _dragSourceListBox = sender as ListBox;
        }

        // Helper: Tìm phần tử cha trong cây visual
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                    return (T)current;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        // BƯỚC 2: Xử lý khi di chuyển chuột
        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    ListBox listBox = sender as ListBox;
                    ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                    if (listBoxItem != null)
                    {
                        _draggedItem = listBoxItem.DataContext as QuestionViewModel;
                        if (_draggedItem != null)
                        {
                            SaveQuestionProperties(_draggedItem);
                            //// Lưu giá trị ComboBox trước khi kéo
                            //_tempComboBoxValues.Clear();
                            //SaveComboBoxValues(_draggedItem);

                            _isDragging = true;
                            DragDrop.DoDragDrop(listBox, _draggedItem, DragDropEffects.Move);
                            _isDragging = false;
                            _draggedItem = null;
                        }
                    }
                }
            }
        }

        private void SaveQuestionProperties(QuestionViewModel question)
        {
            if (!_globalQuestionProperties.ContainsKey(question.Id))
            {
                _globalQuestionProperties[question.Id] = new Dictionary<string, object>();
            }

            if (question is TrueFalseQuestionViewModel tfQuestion && tfQuestion.Statements != null)
            {
                var difficultyList = new List<DifficultyLevel?>();
                foreach (var statement in tfQuestion.Statements)
                {
                    difficultyList.Add(statement.Difficulty);
                }
                _globalQuestionProperties[question.Id]["DifficultyList"] = difficultyList;
            }
        }

        // Phương thức lưu các giá trị ComboBox
        private void SaveComboBoxValues(QuestionViewModel question)
        {
            // Tạo ID duy nhất cho câu hỏi
            string questionId = question.GetHashCode().ToString();

            if (question is TrueFalseQuestionViewModel tfQuestion && tfQuestion.Statements != null)
            {
                var difficultyValues = new List<DifficultyLevel?>();
                foreach (var statement in tfQuestion.Statements)
                {
                    difficultyValues.Add(statement.Difficulty);
                }
                _tempComboBoxValues[questionId] = difficultyValues;
                Console.WriteLine($"Saved {difficultyValues.Count} difficulty values for question {questionId}");
            }
        }

        private void StartDrag(ListBox listBox, MouseEventArgs e)
        {
            try
            {
                // Tạo DataObject chứa dữ liệu kéo
                DataObject dragData = new DataObject("questionFormat", _draggedItem);

                // Tạo hiệu ứng visual khi kéo
                if (_adornerLayer == null)
                {
                    _adornerLayer = AdornerLayer.GetAdornerLayer(listBox);
                }

                if (_adornerLayer != null)
                {
                    // Tìm template mẫu từ ListBox
                    DataTemplate template = listBox.ItemTemplate;

                    // Tạo container cho visual
                    Border container = new Border
                    {
                        Background = new SolidColorBrush(Colors.White),
                        BorderBrush = new SolidColorBrush(Colors.LightGray),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        Width = 300 // Đặt chiều rộng cố định hoặc theo item gốc
                    };

                    // Tạo nội dung cho container
                    ContentPresenter contentPresenter = new ContentPresenter
                    {
                        Content = _draggedItem,
                        ContentTemplate = template
                    };

                    // Thêm content vào container
                    container.Child = contentPresenter;

                    // Tạo transform cho hiệu ứng scale nhẹ khi kéo
                    TransformGroup transforms = new TransformGroup();
                    ScaleTransform scaleTransform = new ScaleTransform(1.05, 1.05);
                    transforms.Children.Add(scaleTransform);
                    container.RenderTransform = transforms;

                    // Tạo adorner với container đã set up
                    _dragAdorner = new DragAdorner(listBox, container, _startPoint);
                    _adornerLayer.Add(_dragAdorner);

                    // Cập nhật vị trí adorner khi di chuyển
                    _dragAdorner.UpdatePosition(e.GetPosition(null));
                }

                // Bắt đầu thao tác kéo thả
                DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Move);

                // Dọn dẹp adorners sau khi kéo
                CleanupAdorners();

                // Reset trạng thái
                _isDragging = false;
                _draggedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi kéo thả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                CleanupAdorners();
                _isDragging = false;
                _draggedItem = null;
            }
        }


        // Thêm phương thức xử lý sự kiện PreviewMouseMove để ngăn bôi đen khi di chuyển chuột
        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                // Ngăn chặn hành vi mặc định của ListBox
                e.Handled = true;
            }
        }

        // Trong phương thức ListBox_DragOver, thêm hiệu ứng tooltip
        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            if (_draggedItem != null)
            {
                ListBox targetListBox = sender as ListBox;

                // Nếu đang kéo từ Panel 1 sang Panel 2, kiểm tra xem đã chọn khối chưa
                if (_dragSourceListBox == ListBox1 && targetListBox == BlockQuestionsList)
                {
                    if (_viewModel.BlocksManager.SelectedBlock == null)
                    {
                        // Nếu chưa chọn khối, hiển thị "không cho phép kéo"
                        e.Effects = DragDropEffects.None;

                        // Hiển thị tooltip thông báo
                        ShowDragNotAllowedTooltip(targetListBox, "Vui lòng chọn một khối trước");
                    }
                    else
                    {
                        // Nếu đã chọn khối, cho phép kéo
                        e.Effects = DragDropEffects.Move;
                        HideDragNotAllowedTooltip();
                    }
                }
                else
                {
                    // Các trường hợp khác (kéo từ Panel 2 sang Panel 1)
                    e.Effects = DragDropEffects.Move;
                    HideDragNotAllowedTooltip();
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        // Phương thức hiển thị tooltip thông báo
        private void ShowDragNotAllowedTooltip(UIElement element, string message)
        {
            if (_dragNotAllowedToolTip == null)
            {
                _dragNotAllowedToolTip = new ToolTip
                {
                    Content = message,
                    Background = new SolidColorBrush(Colors.LightYellow),
                    BorderBrush = new SolidColorBrush(Colors.Orange),
                    BorderThickness = new Thickness(1),
                    Placement = PlacementMode.Mouse
                };
            }

            _dragNotAllowedToolTip.IsOpen = true;
        }

        // Phương thức ẩn tooltip
        private void HideDragNotAllowedTooltip()
        {
            if (_dragNotAllowedToolTip != null)
            {
                _dragNotAllowedToolTip.IsOpen = false;
            }
        }


        // Thêm biến trạng thái để theo dõi tooltip
        private ToolTip _dragNotAllowedToolTip;


        // Phương thức kéo thả - SỬA LẠI
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (_draggedItem != null)
            {
                // QUAN TRỌNG: Lưu tham chiếu đến _draggedItem vì nó sẽ bị đổi thành null sau khi xử lý xong
                var draggedQuestion = _draggedItem;

                ListBox targetListBox = sender as ListBox;
                MainViewModel viewModel = DataContext as MainViewModel;

                // Panel 1 -> Panel 2
                if (_dragSourceListBox == ListBox1 && targetListBox == BlockQuestionsList)
                {
                    // Kiểm tra xem đã chọn khối chưa
                    if (viewModel.BlocksManager.SelectedBlock == null)
                    {
                        MessageBox.Show("Vui lòng chọn một khối trước khi thêm câu hỏi.",
                                       "Chưa chọn khối",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Information);
                        return;
                    }

                    // Ghi nhớ vị trí và collection nguồn
                    int sourceIndex = viewModel.Questions1.IndexOf(draggedQuestion);
                    var sourceCollection = viewModel.Questions1;
                    var destCollection = viewModel.BlocksManager.SelectedBlock.Questions;

                    // Di chuyển câu hỏi
                    viewModel.Questions1.Remove(draggedQuestion);
                    viewModel.BlocksManager.SelectedBlock.Questions.Add(draggedQuestion);

                    // Lưu thao tác để hoàn tác
                    viewModel.BlocksManager.SaveQuestionMoveForUndo(sourceCollection, destCollection, draggedQuestion, sourceIndex);

                    // QUAN TRỌNG: Khôi phục thuộc tính SAU KHI đã thêm vào collection mới
                    RestoreQuestionProperties(draggedQuestion);
                }
                // Panel 2 -> Panel 1
                else if (_dragSourceListBox == BlockQuestionsList && targetListBox == ListBox1)
                {
                    if (viewModel.BlocksManager.SelectedBlock != null)
                    {
                        // Ghi nhớ vị trí và collection nguồn
                        int sourceIndex = viewModel.BlocksManager.SelectedBlock.Questions.IndexOf(draggedQuestion);
                        var sourceCollection = viewModel.BlocksManager.SelectedBlock.Questions;
                        var destCollection = viewModel.Questions1;

                        // Di chuyển câu hỏi
                        viewModel.BlocksManager.SelectedBlock.Questions.Remove(draggedQuestion);
                        viewModel.Questions1.Add(draggedQuestion);

                        // Lưu thao tác để hoàn tác
                        viewModel.BlocksManager.SaveQuestionMoveForUndo(sourceCollection, destCollection, draggedQuestion, sourceIndex);

                        // QUAN TRỌNG: Khôi phục thuộc tính SAU KHI đã thêm vào collection mới
                        RestoreQuestionProperties(draggedQuestion);
                    }
                }

                // Force cập nhật UI để hiển thị giá trị ComboBox mới
                if (draggedQuestion is TrueFalseQuestionViewModel tfQuestion && tfQuestion.Statements != null)
                {
                    foreach (var statement in tfQuestion.Statements)
                    {
                        // Đảm bảo phương thức này tồn tại trong mô hình của bạn
                        statement.OnPropertyChanged(nameof(statement.Difficulty));
                    }
                }
            }
        }

        // Phương thức khôi phục thuộc tính - ĐÃ SỬA ĐỂ THÊM DEBUG
        private void RestoreQuestionProperties(QuestionViewModel question)
        {
            Console.WriteLine($"Restoring properties for question {question.Id}");

            if (_globalQuestionProperties.TryGetValue(question.Id, out var properties))
            {
                Console.WriteLine("Found properties in dictionary");

                if (question is TrueFalseQuestionViewModel tfQuestion &&
                    tfQuestion.Statements != null &&
                    properties.TryGetValue("DifficultyList", out var diffObj) &&
                    diffObj is List<DifficultyLevel> diffList)
                {
                    Console.WriteLine($"Found difficulty list with {diffList.Count} items");

                    for (int i = 0; i < Math.Min(tfQuestion.Statements.Count, diffList.Count); i++)
                    {
                        tfQuestion.Statements[i].Difficulty = diffList[i];
                        Console.WriteLine($"Set statement {i} difficulty to {diffList[i]}");

                        // Force UI update
                        tfQuestion.Statements[i].OnPropertyChanged(nameof(TrueFalseStatementViewModel.Difficulty));
                    }
                }
                else
                {
                    Console.WriteLine("Did not find difficulty list or question type mismatch");
                }
            }
            else
            {
                Console.WriteLine("Question properties not found in dictionary");
            }
        }

        // Phương thức khôi phục các giá trị ComboBox
        private void RestoreComboBoxValues(QuestionViewModel question)
        {
            string questionId = question.GetHashCode().ToString();

            if (_tempComboBoxValues.ContainsKey(questionId))
            {
                if (question is TrueFalseQuestionViewModel tfQuestion &&
                    _tempComboBoxValues[questionId] is List<DifficultyLevel> difficultyValues)
                {
                    for (int i = 0; i < Math.Min(tfQuestion.Statements?.Count ?? 0, difficultyValues.Count); i++)
                    {
                        tfQuestion.Statements[i].Difficulty = difficultyValues[i];
                        Console.WriteLine($"Restored difficulty value {difficultyValues[i]} for statement {i}");
                    }
                }
                // Thêm code cho các loại câu hỏi khác nếu cần
            }
        }



        // Xử lý logic chuyển dữ liệu
        private void ProcessDrop(ListBox targetListBox, QuestionViewModel droppedItem, MainViewModel viewModel)
        {
            // Panel 1 -> Panel 2
            if (_dragSourceListBox == ListBox1 && targetListBox == BlockQuestionsList)
            {
                viewModel.Questions1.Remove(droppedItem);

                // Thêm vào khối được chọn
                if (viewModel.BlocksManager.SelectedBlock != null)
                {
                    viewModel.BlocksManager.SelectedBlock.Questions.Add(droppedItem);

                    // Áp dụng animation sau khi cập nhật UI
                    targetListBox.UpdateLayout();
                    ListBoxItem newItem = targetListBox.ItemContainerGenerator.ContainerFromItem(droppedItem) as ListBoxItem;
                    if (newItem != null)
                    {
                        AnimateAddItem(newItem);
                    }
                }
            }
            // Panel 2 -> Panel 1
            else if (_dragSourceListBox == BlockQuestionsList && targetListBox == ListBox1)
            {
                // Xóa khỏi khối được chọn
                if (viewModel.BlocksManager.SelectedBlock != null)
                {
                    viewModel.BlocksManager.SelectedBlock.Questions.Remove(droppedItem);
                }

                // Thêm vào Panel 1
                viewModel.Questions1.Add(droppedItem);

                // Áp dụng animation sau khi cập nhật UI
                targetListBox.UpdateLayout();
                ListBoxItem newItem = targetListBox.ItemContainerGenerator.ContainerFromItem(droppedItem) as ListBoxItem;
                if (newItem != null)
                {
                    AnimateAddItem(newItem);
                }
            }
        }

        // Thay thế cho AnimationHelper.AnimateAddItem nếu chưa tạo lớp đó
        private void AnimateAddItem(FrameworkElement element)
        {
            // Lưu trữ kích thước ban đầu
            element.Opacity = 0;

            // Thiết lập transform
            ScaleTransform scale = new ScaleTransform(0.8, 0.8);
            element.RenderTransform = scale;

            // Animation opacity
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            // Animation scale X
            DoubleAnimation scaleX = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350)
            };

            // Animation scale Y
            DoubleAnimation scaleY = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350)
            };

            // Chạy animation
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
        }

        // Dọn dẹp adorners
        private void CleanupAdorners()
        {
            if (_adornerLayer != null)
            {
                if (_dragAdorner != null)
                {
                    _adornerLayer.Remove(_dragAdorner);
                    _dragAdorner = null;
                }

                if (_dropTargetAdorner != null)
                {
                    _adornerLayer.Remove(_dropTargetAdorner);
                    _dropTargetAdorner = null;
                }
            }
        }

        // Helper: Tìm phần tử cha trong cây visual
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        #endregion

        #region Khối Drag & Drop

        private void BlocksTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is BlockViewModel block)
            {
                _viewModel.BlocksManager.SelectedBlock = block;
            }
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Kiểm tra xem có đang nhấp vào TreeViewItem hay không
            var treeViewItem = FindVisualParent<TreeViewItem>(e.OriginalSource as DependencyObject);

            // Nếu không nhấp vào TreeViewItem nào, bỏ chọn khối
            if (treeViewItem == null && DataContext is BlocksManagerViewModel viewModel)
            {
                viewModel.SelectedBlock = null;

                // Thay vì gán trực tiếp cho SelectedItem, sử dụng phương thức này để bỏ chọn
                UnselectAllTreeViewItems(BlocksTreeView);
            }
        }

        // Phương thức để bỏ chọn tất cả các items trong TreeView
        private void UnselectAllTreeViewItems(TreeView treeView)
        {
            foreach (var item in GetAllTreeViewItems(treeView))
            {
                item.IsSelected = false;
            }
        }

        // Helper method để lấy tất cả TreeViewItem trong TreeView
        private IEnumerable<TreeViewItem> GetAllTreeViewItems(ItemsControl container)
        {
            for (int i = 0; i < container.Items.Count; i++)
            {
                var item = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item == null)
                    continue;

                yield return item;

                // Đệ quy để lấy các items con
                foreach (var childItem in GetAllTreeViewItems(item))
                {
                    yield return childItem;
                }
            }
        }



        private void BlockQuestions_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void BlockQuestions_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(null);
                if ((Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    ListBox listBox = sender as ListBox;
                    if (listBox?.SelectedItem != null)
                    {
                        DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void BlockQuestions_Drop(object sender, DragEventArgs e)
        {
            // Cho phép kéo thả trong mọi khối (cả cố định và linh động)
            // chỉ kiểm tra xem có khối nào đang được chọn không
            if (_viewModel.BlocksManager.SelectedBlock == null)
                return;

            if (e.Data.GetDataPresent(typeof(QuestionViewModel)))
            {
                var droppedData = e.Data.GetData(typeof(QuestionViewModel)) as QuestionViewModel;
                var listBox = sender as ListBox;
                var targetList = _viewModel.BlocksManager.SelectedBlock.Questions;

                if (droppedData == null || listBox == null) return;

                // Xác định nguồn
                var sourceList = _viewModel.Questions1.Contains(droppedData)
                    ? _viewModel.Questions1
                    : _viewModel.Questions2.Contains(droppedData)
                        ? _viewModel.Questions2
                        : targetList;

                // Nếu nguồn khác đích, xóa khỏi nguồn
                if (sourceList != targetList)
                {
                    sourceList.Remove(droppedData);
                }

                int insertIndex = GetDropIndex(listBox, e);

                if (!targetList.Contains(droppedData))
                {
                    targetList.Insert(insertIndex, droppedData);
                }
                else if (sourceList == targetList)
                {
                    int currentIndex = targetList.IndexOf(droppedData);
                    if (currentIndex < insertIndex)
                        insertIndex--;

                    if (currentIndex != insertIndex)
                    {
                        targetList.Move(currentIndex, insertIndex);
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        private int GetDropIndex(ListBox listBox, DragEventArgs e)
        {
            int index = listBox.Items.Count;
            Point mousePos = e.GetPosition(listBox);

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item != null)
                {
                    Point itemPos = item.TransformToAncestor(listBox).Transform(new Point(0, 0));
                    double itemHeight = item.ActualHeight;

                    if (mousePos.Y < itemPos.Y + (itemHeight / 2))
                    {
                        index = i;
                        break;
                    }
                    else if (i == listBox.Items.Count - 1 && mousePos.Y < itemPos.Y + itemHeight)
                    {
                        index = i + 1;
                        break;
                    }
                }
            }

            return index;
        }

        #endregion

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    BlockViewModel block = textBox.DataContext as BlockViewModel;
                    if (block != null)
                    {
                        block.IsEditing = false;
                        e.Handled = true;
                    }
                }
            }
        }

        // Thêm phương thức này
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Gọi Dispose để hủy đăng ký sự kiện
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.Dispose();
            }
        }

        // Phương thức xử lý khi nhấn nút đóng
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận trước khi đóng
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng chương trình không?",
                "Xác nhận đóng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Đóng chương trình
                Application.Current.Shutdown();
            }
        }

        // Sửa lại hàm xử lý sự kiện PreviewMouseWheel
        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Đảm bảo biên độ cuộn nhỏ và đồng nhất cả khi lăn lên và lăn xuống
            if (sender is ListBox listBox && e.Delta != 0)
            {
                ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(listBox);
                if (scrollViewer != null)
                {
                    // Sửa lại công thức tính delta
                    // e.Delta > 0 có nghĩa là cuộn lên (giảm offset)
                    // e.Delta < 0 có nghĩa là cuộn xuống (tăng offset)
                    double smallScrollAmount = 1; // Đặt biên độ cuộn nhỏ hơn

                    // Điều chỉnh biên độ cuộn dựa trên hướng cuộn
                    double newOffset = scrollViewer.VerticalOffset - (e.Delta / 120.0) * smallScrollAmount;

                    // Giới hạn offset trong phạm vi hợp lệ
                    newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));

                    scrollViewer.ScrollToVerticalOffset(newOffset);
                    e.Handled = true; // Ngăn không cho sự kiện truyền tiếp
                }
            }
        }

        // Phương thức hỗ trợ tìm kiếm control con
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void ClearPanel1_Click(object sender, RoutedEventArgs e)
        {
            // DataContext là ViewModel chứa Questions1 (ObservableCollection<QuestionViewModel>)
            dynamic vm = this.DataContext;
            if (vm != null && vm.Questions1 != null)
            {
                // Xác nhận trước khi xóa
                if (MessageBox.Show("Bạn chắc chắn muốn xóa toàn bộ câu hỏi trong Panel 1?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    vm.Questions1.Clear();
                }
            }
        }

        //---------------------------------------5/6/2025---------------------------------
        // Gắn vào sự kiện Click nút "Chọn file Word"
        private void ImportWord_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                Title = "Chọn file Word để import câu hỏi"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    dynamic vm = this.DataContext;
                    ObservableCollection<QuestionViewModel> questions = vm?.Questions1;
                    if (questions == null)
                    {
                        MessageBox.Show("Không tìm thấy danh sách câu hỏi Panel1!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    //questions.Clear();
                    OpenXmlWordImporter.ImportQuestionsFromWord(openFileDialog.FileName, questions);
                    MessageBox.Show("Đã import xong câu hỏi từ file Word!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Lỗi import Word: " + ex.Message);
                }
            }
        }



    }
}