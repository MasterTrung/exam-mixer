using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DragDropTreeApp
{
    public class ExamMixerService
    {
        /// <summary>
        /// Phương thức chính để tạo đề thi
        /// </summary>
        public List<ExamViewModel> MixExams(BlocksManagerViewModel blocksManager,
                                          int numberOfExams,
                                          bool useAutoCode,
                                          string prefix = null,
                                          List<string> customCodes = null)
        {
            if (useAutoCode)
                return GenerateExamsWithAutoCode(blocksManager, numberOfExams, prefix);
            else
                return GenerateExamsWithCustomCodes(blocksManager, numberOfExams, customCodes);
        }

        /// <summary>
        /// Tạo đề thi với mã đề tự động
        /// </summary>
        public List<ExamViewModel> GenerateExamsWithAutoCode(
            BlocksManagerViewModel blocksManager,
            int numberOfExams,
            string prefix)
        {
            var result = new List<ExamViewModel>();

            // Tạo Random với seed dựa trên thời gian hiện tại
            var random = new Random(DateTime.Now.Millisecond);

            // Tạo các mã đề tự động
            List<string> examCodes = GenerateAutoCodes(numberOfExams, prefix);

            for (int i = 0; i < numberOfExams; i++)
            {
                var exam = new ExamViewModel
                {
                    Code = examCodes[i],
                    Title = $"Đề thi {examCodes[i]}",
                    Blocks = new ObservableCollection<ExamBlockViewModel>()
                };

                // Xử lý các khối và các khối con
                ProcessBlocks(blocksManager.RootBlocks, exam, new Random(random.Next()));

                result.Add(exam);
            }

            return result;
        }

        /// <summary>
        /// Tạo đề thi với mã đề tùy chỉnh
        /// </summary>
        public List<ExamViewModel> GenerateExamsWithCustomCodes(
    BlocksManagerViewModel blocksManager,
    int numberOfExams,
    List<string> customCodes)
        {
            var result = new List<ExamViewModel>();

            // Tạo Random với seed dựa trên thời gian hiện tại
            var random = new Random(DateTime.Now.Millisecond);

            // Đảm bảo số lượng mã đề đủ
            if (customCodes.Count < numberOfExams)
            {
                throw new ArgumentException($"Số lượng mã đề tùy chỉnh ({customCodes.Count}) ít hơn số đề cần tạo ({numberOfExams})");
            }

            for (int i = 0; i < numberOfExams; i++)
            {
                var exam = new ExamViewModel
                {
                    Code = customCodes[i],
                    Title = $"Đề thi {customCodes[i]}",
                    Blocks = new ObservableCollection<ExamBlockViewModel>()
                };

                // Xử lý các khối và các khối con
                ProcessBlocks(blocksManager.RootBlocks, exam, new Random(random.Next()));

                result.Add(exam);
            }

            return result;
        }

        /// <summary>
        /// Xử lý từng khối và thêm vào đề thi
        /// </summary>
        private void ProcessBlocks(ObservableCollection<BlockViewModel> blocks,
                                 ExamViewModel exam,
                                 Random random)
        {
            foreach (var block in blocks)
            {
                var examBlock = new ExamBlockViewModel
                {
                    Name = block.Name,
                    IsFixed = block.IsFixed,
                    Questions = new ObservableCollection<QuestionViewModel>()
                };

                // Clone các câu hỏi để không ảnh hưởng đến gốc
                var questionsCopy = block.Questions.Select(q => q.Clone()).ToList();

                // Nếu là khối linh động, trộn thứ tự các câu hỏi
                if (!block.IsFixed)
                {
                    ShuffleList(questionsCopy, random);
                }

                // Trộn nội dung từng câu hỏi
                foreach (var question in questionsCopy)
                {
                    // Trộn mệnh đề trong câu hỏi đúng/sai theo độ khó
                    if (question is TrueFalseQuestionViewModel tfq)
                    {
                        // Nếu tất cả Difficulty == null thì KHÔNG trộn
                        if (tfq.Statements != null && tfq.Statements.All(s => s.Difficulty == null))
                        {
                            // Không làm gì cả, giữ nguyên thứ tự gốc
                        }
                        else
                        {
                            tfq.ShuffleByDifficulty(random); // Trộn nếu có ít nhất 1 mệnh đề có độ khó
                        }
                    }
                    // Trộn ngẫu nhiên các phương án trong câu hỏi trắc nghiệm
                    else if (question is MultipleChoiceQuestionViewModel mcq)
                    {
                        mcq.ShuffleAnswers(random);
                    }

                    examBlock.Questions.Add(question);
                }

                exam.Blocks.Add(examBlock);

                // Xử lý các khối con (nếu có)
                if (block.Children.Count > 0)
                {
                    ProcessBlocks(block.Children, exam, random);
                }
            }

            // Kiểm tra câu hỏi TrueFalse
            foreach (var block in exam.Blocks)
            {
                foreach (var question in block.Questions)
                {
                    if (question is TrueFalseQuestionViewModel tf)
                    {
                        Console.WriteLine($"Block {block.Name}: TrueFalseQuestion has {tf.Statements?.Count ?? 0} statements");
                        if (tf.Statements != null)
                        {
                            foreach (var stmt in tf.Statements)
                            {
                                Console.WriteLine($"  - Statement: {stmt.Text}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tạo các mã đề tự động
        /// </summary>
        private List<string> GenerateAutoCodes(int count, string prefix)
        {
            var result = new List<string>();
            prefix = prefix ?? "MĐ";

            for (int i = 1; i <= count; i++)
            {
                result.Add($"{prefix}{i:D2}");
            }

            return result;
        }

        /// <summary>
        /// Trộn ngẫu nhiên một danh sách
        /// </summary>
        private void ShuffleList<T>(List<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}