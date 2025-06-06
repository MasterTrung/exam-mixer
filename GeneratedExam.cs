using System.Collections.Generic;

namespace DragDropTreeApp
{
    public class GeneratedExam
    {
        public string ExamCode { get; set; }
        public List<QuestionViewModel> Questions { get; set; }

        public GeneratedExam()
        {
            Questions = new List<QuestionViewModel>();
        }
    }
}