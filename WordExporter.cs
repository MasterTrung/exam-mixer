using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DragDropTreeApp
{
    public class WordExporter
    {
        public void ExportExamToWord(ExamViewModel exam, string filePath, bool includeAnswerSheet = false)
        {
            Console.WriteLine($"Starting export to {filePath}");
            Console.WriteLine($"Exam has {exam.Blocks?.Count ?? 0} blocks");

            try
            {
                using (WordprocessingDocument wordDocument =
                      WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    // Add a main document part
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Thêm tiêu đề và mã đề
                    AddParagraphWithText(body, "ĐỀ THI", true, JustificationValues.Center, 32);
                    AddParagraphWithText(body, $"Mã đề: {exam.Code}", true, JustificationValues.Center, 28);

                    // Thêm dòng trống
                    body.AppendChild(new Paragraph(new Run(new Break())));

                    // Khởi tạo số thứ tự câu hỏi
                    int questionNumber = 1;

                    // Duyệt qua từng khối
                    foreach (var block in exam.Blocks)
                    {
                        Console.WriteLine($"Processing block: {block.Name} with {block.Questions?.Count ?? 0} questions");

                        // Thêm tên khối
                        if (!string.IsNullOrEmpty(block.Name))
                        {
                            AddParagraphWithText(body, block.Name, true, JustificationValues.Left, 28);
                        }

                        // Duyệt qua từng câu hỏi trong khối
                        foreach (var question in block.Questions)
                        {
                            Console.WriteLine($"Processing question #{questionNumber}: {question.GetType().Name}");
                            ProcessQuestion(body, question, ref questionNumber);
                        }
                    }

                    // Sau khi xuất xong phần đề, bổ sung xuất đáp án nếu có
                    if (includeAnswerSheet)
                    {
                        body = mainPart.Document.Body;
                        // Thêm tiêu đề phần đáp án
                        AddParagraphWithText(body, "ĐÁP ÁN", true, JustificationValues.Center, 32);
                        questionNumber = 1;
                        foreach (var block in exam.Blocks)
                        {
                            foreach (var question in block.Questions)
                            {
                                // Xuất đáp án theo loại
                                string answerText = GetAnswerText(question);
                                AddParagraphWithText(body, $"Câu {questionNumber}: {answerText}", false, JustificationValues.Left, 24, 720);
                                questionNumber++;
                            }
                        }
                    }

                    wordDocument.Save();


                    wordDocument.Save();
                }

                MessageBox.Show($"Đề thi đã được xuất thành công tại {filePath}", "Xuất thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Word: {ex.Message}\n{ex.StackTrace}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            }
        }


        // Hàm lấy đáp án đúng cho từng loại câu hỏi
        private string GetAnswerText(QuestionViewModel question)
        {
            if (question is MultipleChoiceQuestionViewModel mcq)
            {
                // Tìm đáp án đúng
                var correct = mcq.Answers?
                    .Select((a, idx) => new { a, idx })
                    .Where(x => x.a.IsCorrect)
                    .Select(x => ((char)('A' + x.idx)).ToString())
                    .ToList();
                return correct != null && correct.Count > 0 ? $"Đáp án: {string.Join(", ", correct)}" : "Không có đáp án";
            }
            else if (question is TrueFalseQuestionViewModel tfq)
            {
                var ans = tfq.Statements?
                    .Select((s, idx) => $"({(char)('a' + idx)}: {(s.IsTrue ? "Đúng" : "Sai")})");
                return "Đáp án: " + string.Join(" ", ans ?? new List<string>());
            }
            else if (question is ShortAnswerQuestionViewModel saq)
            {
                return $"Đáp án: {saq.Solution}";
            }
            else if (question is EssayQuestionViewModel eq)
            {
                return $"Gợi ý: {eq.Solution}";
            }
            else
            {
                return $"Đáp án: {question.Solution}";
            }
        }


        private void ProcessQuestion(Body body, QuestionViewModel question, ref int questionNumber)
        {
            // Lấy nội dung câu hỏi
            string content = !string.IsNullOrEmpty(question.Content)
                ? question.Content
                : (!string.IsNullOrEmpty(question.Title) ? question.Title : $"Câu {questionNumber}");

            // Thêm tiêu đề câu hỏi
            AddParagraphWithText(body, $"Câu {questionNumber}: {content}", true, JustificationValues.Left);

            // Xử lý từng loại câu hỏi theo enum QuestionType thay vì kiểm tra kiểu đối tượng
            if (question is MultipleChoiceQuestionViewModel mcQuestion)
            {
                Console.WriteLine($"Multiple choice with {mcQuestion.Answers?.Count ?? 0} answers");
                ProcessMultipleChoiceQuestion(body, mcQuestion);
            }
            else if (question is TrueFalseQuestionViewModel tfQuestion)
            {
                Console.WriteLine($"True/False with {tfQuestion.Statements?.Count ?? 0} statements");
                ProcessTrueFalseQuestion(body, tfQuestion);
            }
            else if (question.Type == QuestionType.ShortAnswer)
            {
                AddParagraphWithText(body, "_____________________________________________", false, JustificationValues.Left);
            }
            else if (question.Type == QuestionType.Essay)
            {
                for (int i = 0; i < 5; i++)
                {
                    AddParagraphWithText(body, "_____________________________________________", false, JustificationValues.Left);
                }
            }

            // Thêm dòng trống giữa các câu hỏi
            body.AppendChild(new Paragraph(new Run(new Break())));

            // Tăng số thứ tự câu hỏi
            questionNumber++;
        }

        private void ProcessMultipleChoiceQuestion(Body body, MultipleChoiceQuestionViewModel question)
        {
            Console.WriteLine("Processing Multiple Choice question");
            if (question.Answers != null && question.Answers.Count > 0)
            {
                char choiceLetter = 'A';
                foreach (var answer in question.Answers)
                {
                    string answerText = answer.Text ?? "[Nội dung trống]";
                    AddParagraphWithText(body, $"{choiceLetter}. {answerText}", false, JustificationValues.Left, 24, 720);
                    choiceLetter++;
                    Console.WriteLine($"Added answer: {choiceLetter - 1}. {answerText}");
                }
            }
            else
            {
                Console.WriteLine("WARNING: Multiple Choice question has no answers!");
                AddParagraphWithText(body, "[Không có phương án]", false, JustificationValues.Left, 24, 720);
            }
        }

        private void ProcessTrueFalseQuestion(Body body, TrueFalseQuestionViewModel question)
        {
            Console.WriteLine("Processing True/False question");
            if (question.Statements != null && question.Statements.Count > 0)
            {
                int statementNumber = 0;
                foreach (var statement in question.Statements)
                {
                    string statementText = statement.Text ?? "[Nội dung trống]";
                    char label = (char)('a' + statementNumber); // a, b, c, d,...
                    AddParagraphWithText(body, $"{label}) {statementText}", false, JustificationValues.Left, 24, 720);
                    statementNumber++;
                    Console.WriteLine($"Added statement: {label}) {statementText}");
                }
            }
            else
            {
                Console.WriteLine("WARNING: True/False question has no statements!");
                AddParagraphWithText(body, "[Không có mệnh đề]", false, JustificationValues.Left, 24, 720);
            }
        }

        private void AddParagraphWithText(Body body, string text, bool isBold, JustificationValues justification, int fontSize = 24, int indentLeft = 0)
        {
            Paragraph para = body.AppendChild(new Paragraph());

            // Set paragraph properties
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            paragraphProperties.Justification = new Justification { Val = justification };

            if (indentLeft > 0)
            {
                paragraphProperties.Indentation = new Indentation { Left = indentLeft.ToString() };
            }

            para.AppendChild(paragraphProperties);

            // Create run properties
            Run run = new Run();
            RunProperties runProperties = new RunProperties();

            if (isBold)
            {
                runProperties.Bold = new Bold();
            }

            if (fontSize != 24) // 24 half-points = 12pt (default)
            {
                runProperties.FontSize = new FontSize { Val = fontSize.ToString() };
            }

            run.AppendChild(runProperties);
            run.AppendChild(new Text(text));
            para.AppendChild(run);
        }
    }
}