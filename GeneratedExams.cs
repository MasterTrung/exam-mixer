using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DragDropTreeApp
{
    public class GeneratedExams : ObservableCollection<ExamViewModel>
    {
        // Class này kế thừa ObservableCollection để thuận tiện cho việc binding
        public GeneratedExams() : base() { }

        public GeneratedExams(IEnumerable<ExamViewModel> exams) : base(exams) { }
    }
}