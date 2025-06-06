using System;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System.Reflection;     // Cho AssemblyName 


namespace DragDropTreeApp
{
    public class MainViewModel : INotifyPropertyChanged
    {




        // Thuộc tính và constructor

      //  private ExamMixerService _examMixerService2;
        // Khai báo các dịch vụ mới
        private readonly ExamMixerService _examMixerService;
        private readonly WordExporter _wordExporter;

        private GeneratedExams _generatedExams;
        public GeneratedExams GeneratedExams
        {
            get => _generatedExams;
            set
            {
                _generatedExams = value;
                OnPropertyChanged(nameof(GeneratedExams));
            }
        }

        // Thêm commands
        public ICommand ShowExamMixerDialogCommand { get; }
        public ICommand ExportExamsToWordCommand { get; }

        public MainViewModel()
        {
            // Khởi tạo các dịch vụ
            _examMixerService = new ExamMixerService();
            _wordExporter = new WordExporter();
            _generatedExams = new GeneratedExams();

            // Khởi tạo dữ liệu mẫu
            Questions1 = SampleQuestionsGenerator.GenerateSampleQuestions();
            Questions2 = new ObservableCollection<QuestionViewModel>();

            // Khởi tạo quản lý khối
            BlocksManager = new BlocksManagerViewModel();
            // Đăng ký các sự kiện
            BlocksManager.QuestionsRecycled += OnQuestionsRecycled;
            BlocksManager.QuestionsRestored += OnQuestionsRestored;

            // Khởi tạo commands
            ShowExamMixerDialogCommand = new RelayCommand(_ => ShowExamMixerDialog());
            // Thành:
            ExportExamsToWordCommand = new RelayCommand(_ => ExportExamsToWord(), _ => _generatedExams != null && _generatedExams.Count > 0);
        }

        


        private ObservableCollection<QuestionViewModel> _questions1;
        public ObservableCollection<QuestionViewModel> Questions1
        {
            get => _questions1;
            set
            {
                if (_questions1 != value)
                {
                    _questions1 = value;
                    OnPropertyChanged(nameof(Questions1));
                }
            }
        }

        private ObservableCollection<QuestionViewModel> _questions2;
        public ObservableCollection<QuestionViewModel> Questions2
        {
            get => _questions2;
            set
            {
                if (_questions2 != value)
                {
                    _questions2 = value;
                    OnPropertyChanged(nameof(Questions2));
                }
            }
        }

        private BlocksManagerViewModel _blocksManager;
        public BlocksManagerViewModel BlocksManager
        {
            get => _blocksManager;
            set
            {
                if (_blocksManager != value)
                {
                    _blocksManager = value;
                    OnPropertyChanged(nameof(BlocksManager));
                }
            }
        }

        // Phương thức ShowExamMixerDialog sửa lại
        private void ShowExamMixerDialog()
        {
            try
            {
                ExamMixerDialog dialog = new ExamMixerDialog();
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    // Lấy các thông số cài đặt
                    int numberOfExams = dialog.NumberOfExams;
                    bool useAutoCode = dialog.UseAutoCode;
                    string prefix = dialog.Prefix;
                    List<string> customCodes = dialog.CustomCodes;

                    // Tạo đề thi
                    List<ExamViewModel> exams;

                    if (useAutoCode)
                    {
                        exams = _examMixerService.GenerateExamsWithAutoCode(
                            BlocksManager,
                            numberOfExams,
                            prefix);
                    }
                    else
                    {
                        exams = _examMixerService.GenerateExamsWithCustomCodes(
                            BlocksManager,
                            numberOfExams,
                            customCodes);
                    }

                    // Lưu đề thi đã tạo (dùng constructor với tham số)
                    GeneratedExams = new GeneratedExams(exams);

                    // Hiển thị thông báo
                    MessageBox.Show($"Đã tạo thành công {numberOfExams} đề thi.",
                                  "Thành công",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo đề thi: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void ExportExamsToWord()
        {
            try
            {
                // Kiểm tra xem có đề thi nào chưa
                if (_generatedExams == null || _generatedExams.Count == 0)
                {
                    MessageBox.Show("Chưa có đề thi nào được tạo. Vui lòng trộn đề thi trước.",
                                   "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Sử dụng WPF SaveFileDialog thay vì FolderBrowserDialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Chọn vị trí lưu đề thi",
                    FileName = "DeThi", // Tên mặc định
                    DefaultExt = ".docx", // Định dạng mặc định
                    Filter = "Word Document (.docx)|*.docx" // Lọc file
                };

                if (dialog.ShowDialog() == true)
                {
                    string selectedPath = dialog.FileName;
                    string folderPath = System.IO.Path.GetDirectoryName(selectedPath);
                    int exported = 0;

                    foreach (var exam in _generatedExams)
                    {
                        // Tạo tên file theo mã đề
                        string fileName = $"De_{exam.Code}.docx";
                        string filePath = System.IO.Path.Combine(folderPath, fileName);

                        // Xuất đề thi ra Word
                        _wordExporter.ExportExamToWord(exam, filePath);
                        exported++;
                    }

                    if (exported > 0)
                    {
                        MessageBox.Show($"Đã xuất {exported} đề thi thành công!",
                                      "Thành công",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);

                        // Mở thư mục chứa đề thi
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                    }
                    else
                    {
                        MessageBox.Show("Không có đề thi nào được xuất!",
                                      "Thông báo",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất đề thi: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // Phương thức xử lý khi có câu hỏi được trả về từ khối bị xóa
        private void OnQuestionsRecycled(object sender, List<QuestionViewModel> questions)
        {
            if (questions == null || questions.Count == 0) return;

            foreach (var question in questions)
            {
                Questions1.Add(question);
            }

            // Thông báo (có thể bỏ hoặc giữ)
            MessageBox.Show($"Đã trả {questions.Count} câu hỏi về Panel 1.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Phương thức mới xử lý khi hoàn tác xóa khối - đã sửa
        private void OnQuestionsRestored(object sender, List<QuestionViewModel> questions)
        {
            if (questions == null || questions.Count == 0) return;

            // Chỉ xóa câu hỏi khỏi Panel 1, không thêm vào khối
            // vì khối được hoàn tác đã có sẵn các câu hỏi
            foreach (var question in questions)
            {
                // Tìm và xóa câu hỏi từ Questions1
                for (int i = Questions1.Count - 1; i >= 0; i--)
                {
                    if (Questions1[i] == question)
                    {
                        Questions1.RemoveAt(i);
                        break;
                    }
                }
            }

            // Không cần thêm lại vào khối được chọn vì:
            // 1. Khối đã được phục hồi hoàn toàn với các câu hỏi ban đầu
            // 2. Việc thêm lại dẫn đến nhân đôi câu hỏi
        }

        // Khi đối tượng bị hủy, hãy hủy đăng ký sự kiện
        public void Dispose()
        {
            if (BlocksManager != null)
            {
                BlocksManager.QuestionsRecycled -= OnQuestionsRecycled;
                BlocksManager.QuestionsRestored -= OnQuestionsRestored;
            }
        }

        // Trong MainViewModel, cập nhật phương thức để thêm hiệu ứng khi thêm/xóa câu hỏi

        public void MoveQuestionsToBlock(IEnumerable<QuestionViewModel> questions, BlockViewModel targetBlock, Action<FrameworkElement> animateAction = null)
        {
            if (targetBlock == null || questions == null) return;

            foreach (var question in questions.ToList())
            {
                // Tìm và xóa khỏi Panel1
                Questions1.Remove(question);

                // Thêm vào khối
                targetBlock.Questions.Add(question);
            }

            // UI sẽ được cập nhật thông qua binding
            // animateAction có thể được gọi từ code-behind để thêm animation
        }

        public void MoveQuestionsFromBlockToPanel1(IEnumerable<QuestionViewModel> questions, Action<FrameworkElement> animateAction = null)
        {
            if (questions == null) return;

            foreach (var question in questions.ToList())
            {
                // Thêm trước vào Panel1
                Questions1.Add(question);

                // Tìm và xóa khỏi khối
                if (BlocksManager.SelectedBlock != null)
                {
                    BlocksManager.SelectedBlock.Questions.Remove(question);
                }
            }

            // UI sẽ được cập nhật thông qua binding
            // animateAction có thể được gọi từ code-behind để thêm animation
        }


    }
}