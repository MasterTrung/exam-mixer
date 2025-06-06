using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace DragDropTreeApp
{
    public partial class ExamMixerDialog : Window
    {
        public int NumberOfExams { get; private set; }
        public bool UseAutoCode { get; private set; }
        public string Prefix { get; private set; }
        public List<string> CustomCodes { get; private set; }

        public ExamMixerDialog()
        {
            // Khởi tạo các giá trị trước khi gọi InitializeComponent
            UseAutoCode = true;
            NumberOfExams = 4; // Mặc định 4 đề

            try
            {
                InitializeComponent();

                // Thiết lập giá trị mặc định sau khi InitializeComponent
                NumberOfExamsSlider.Value = NumberOfExams;
                AutoCodeRadio.IsChecked = true;
                CustomCodeRadio.IsChecked = false;
                PrefixTextBox.Text = "MĐ";
                CustomCodesTextBox.Text = "MĐ01, MĐ02, MĐ03, MĐ04";

                // Kiểm tra xem các control đã được khởi tạo đúng chưa
                Debug.WriteLine($"Control initialization: " +
                    $"NumberOfExamsSlider: {NumberOfExamsSlider != null}, " +
                    $"AutoCodeRadio: {AutoCodeRadio != null}, " +
                    $"CustomCodeRadio: {CustomCodeRadio != null}, " +
                    $"PrefixTextBox: {PrefixTextBox != null}, " +
                    $"CustomCodesTextBox: {CustomCodesTextBox != null}, " +
                    $"WarningTextBlock: {WarningTextBlock != null}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo cửa sổ: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Lỗi khởi tạo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoCodeRadio_Checked(object sender, RoutedEventArgs e)
        {
            UseAutoCode = true;
            SafeValidateInput();
        }

        private void CustomCodeRadio_Checked(object sender, RoutedEventArgs e)
        {
            UseAutoCode = false;
            SafeValidateInput();
        }

        private void NumberOfExamsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SafeValidateInput();
        }

        private void CustomCodesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SafeValidateInput();
        }

        // Phương thức bọc an toàn cho ValidateInput
        private void SafeValidateInput()
        {
            try
            {
                ValidateInput();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi xác thực đầu vào: {ex.Message}");
                // Không hiển thị MessageBox ở đây để tránh làm gián đoạn trải nghiệm người dùng
            }
        }

        private void ValidateInput()
        {
            // Kiểm tra xem WarningTextBlock có tồn tại không
            if (WarningTextBlock == null)
            {
                Debug.WriteLine("WarningTextBlock là null");
                return;
            }

            if (!UseAutoCode)
            {
                // Kiểm tra xem NumberOfExamsSlider và CustomCodesTextBox có tồn tại không
                if (NumberOfExamsSlider == null || CustomCodesTextBox == null)
                {
                    Debug.WriteLine("NumberOfExamsSlider hoặc CustomCodesTextBox là null");
                    return;
                }

                // Kiểm tra số lượng mã đề với số lượng đề đã chọn
                int numberOfExams = (int)NumberOfExamsSlider.Value;

                // Sử dụng chuỗi rỗng nếu CustomCodesTextBox.Text là null
                string codesText = CustomCodesTextBox.Text ?? string.Empty;

                string[] codes = codesText.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(c => c.Trim())
                                                  .ToArray();

                if (codes.Length != numberOfExams)
                {
                    WarningTextBlock.Text = $"Cảnh báo: Bạn đã chọn {numberOfExams} đề nhưng chỉ có {codes.Length} mã đề!";
                    WarningTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    WarningTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                WarningTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy số lượng đề
                NumberOfExams = (int)NumberOfExamsSlider.Value;

                if (UseAutoCode)
                {
                    // Lấy tiền tố nếu sử dụng chế độ tự động
                    Prefix = PrefixTextBox.Text;
                    CustomCodes = null;
                }
                else
                {
                    // Lấy mã đề tùy chỉnh
                    string[] codes = CustomCodesTextBox.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(c => c.Trim())
                                                       .ToArray();

                    // Nếu số lượng mã đề không khớp với số lượng đề
                    if (codes.Length != NumberOfExams)
                    {
                        MessageBox.Show($"Số lượng mã đề ({codes.Length}) không khớp với số lượng đề đã chọn ({NumberOfExams}).\nVui lòng điều chỉnh lại.",
                                        "Lỗi cấu hình",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return;
                    }

                    CustomCodes = new List<string>(codes);
                    Prefix = null;
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}