using System.Collections.ObjectModel;

namespace DragDropTreeApp
{
    public class ExamViewModel
    {
        // Cả hai thuộc tính cho khả năng tương thích
        public string ExamCode { get; set; }
        public string Code { get; set; }

        public string Title { get; set; }

        // Lưu trữ các khối câu hỏi
        public ObservableCollection<ExamBlockViewModel> Blocks { get; set; }

        public ExamViewModel()
        {
            Blocks = new ObservableCollection<ExamBlockViewModel>();
        }
    }
}