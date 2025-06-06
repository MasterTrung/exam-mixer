using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DragDropTreeApp
{
    public static class SampleQuestionsGenerator
    {
        private static readonly Random random = new Random();

        public static ObservableCollection<QuestionViewModel> GenerateSampleQuestions()
        {
            var questions = new ObservableCollection<QuestionViewModel>();

            // 20 câu trắc nghiệm
            for (int i = 1; i <= 20; i++)
            {
                questions.Add(CreateMultipleChoiceQuestion(i));
            }

            // 5 câu đúng sai
            for (int i = 21; i <= 25; i++)
            {
                questions.Add(CreateTrueFalseQuestion(i));
            }

            // 5 câu trả lời ngắn
            for (int i = 26; i <= 30; i++)
            {
                questions.Add(CreateShortAnswerQuestion(i));
            }

            // 2 câu tự luận
            for (int i = 31; i <= 32; i++)
            {
                questions.Add(CreateEssayQuestion(i));
            }

            return questions;
        }

        private static string GetRandomDomain()
        {
            string[] domains = { "Khoa học", "Lịch sử", "Địa lý", "Văn học", "Toán học" };
            return domains[random.Next(domains.Length)];
        }

        private static string GetRandomQuestionText()
        {
            string[] questions = {
                "Trong các phát biểu sau, phát biểu nào là đúng?",
                "Kết quả của phép tính sau là bao nhiêu?",
                "Đâu là đặc điểm chính của hiện tượng này?",
                "Sự kiện nào xảy ra trước trong lịch sử?",
                "Đâu là yếu tố quan trọng nhất trong quá trình này?"
            };
            return questions[random.Next(questions.Length)];
        }

        private static string[] GenerateRandomAnswers(string domain)
        {
            switch (domain)
            {
                case "Khoa học":
                    return new string[] {
                        "Nước sôi ở 90 độ C",
                        "Trái đất phẳng",
                        "Ánh sáng di chuyển chậm hơn âm thanh",
                        "Con người chỉ sử dụng 10% não bộ"
                    };
                case "Lịch sử":
                    return new string[] {
                        "Việt Nam độc lập năm 1945",
                        "Mỹ độc lập năm 1876",
                        "Chiến tranh thế giới thứ hai kết thúc năm 1939",
                        "Liên Xô thành lập năm 1922"
                    };
                default:
                    return new string[] {
                        "Phương án A",
                        "Phương án B",
                        "Phương án C",
                        "Phương án D"
                    };
            }
        }

        private static QuestionViewModel CreateMultipleChoiceQuestion(int index)
        {
            // Tạo đối tượng trực tiếp là MultipleChoiceQuestionViewModel
            var question = new MultipleChoiceQuestionViewModel
            {
                Title = $"Câu {index}: Chọn đáp án đúng",
                Content = GetRandomQuestionText(),
                Type = QuestionType.MultipleChoice
            };

            // Đảm bảo Answers được khởi tạo
            if (question.Answers == null)
            {
                question.Answers = new ObservableCollection<AnswerViewModel>();
            }

            // Tạo các câu trả lời
            string[] answers = GenerateRandomAnswers(GetRandomDomain());
            int correctIndex = random.Next(0, answers.Length);

            for (int i = 0; i < answers.Length; i++)
            {
                question.Answers.Add(new AnswerViewModel
                {
                    Text = answers[i],
                    IsCorrect = (i == correctIndex),
                    GroupName = $"Group_{index}"
                });
            }

            question.Solution = $"Đáp án đúng: {(char)('A' + correctIndex)}";

            return question;
        }

        private static QuestionViewModel CreateTrueFalseQuestion(int index)
        {
            // Tạo đối tượng trực tiếp là TrueFalseQuestionViewModel
            var question = new TrueFalseQuestionViewModel
            {
                Title = $"Câu {index}: Xác định các phát biểu đúng/sai",
                Content = $"Đánh dấu vào các phát biểu ĐÚNG về {GetRandomDomain()}:",
                Type = QuestionType.TrueFalse
            };

            // Đảm bảo Statements được khởi tạo
            if (question.Statements == null)
            {
                question.Statements = new ObservableCollection<TrueFalseStatementViewModel>();
            }

            // Tạo các mệnh đề ngẫu nhiên
            string[] statements = GenerateRandomStatements(GetRandomDomain());
            bool[] truths = { true, false, true, false };

            for (int i = 0; i < 4; i++)
            {
                question.Statements.Add(new TrueFalseStatementViewModel
                {
                    Text = statements[i],
                    IsTrue = truths[i],
                    UserAnswer = false  // Reset câu trả lời người dùng
                });
            }

            question.Solution = $"Đáp án đúng: {string.Join(", ", Enumerable.Range(1, 4).Where(i => truths[i - 1]).Select(i => i))}";

            return question;
        }

        private static QuestionViewModel CreateShortAnswerQuestion(int index)
        {
            string[] subjects = { "Lịch sử", "Địa lý", "Văn học", "Khoa học", "Thể thao" };
            string subject = subjects[random.Next(subjects.Length)];

            // Sử dụng ShortAnswerQuestionViewModel thay vì QuestionViewModel
            var question = new ShortAnswerQuestionViewModel
            {
                Type = QuestionType.ShortAnswer,
                Title = $"Câu {index}: Trả lời ngắn gọn",
                Content = GenerateRandomShortQuestion(subject),
                Solution = GenerateRandomShortAnswer(subject)
            };

            return question;
        }

        private static QuestionViewModel CreateEssayQuestion(int index)
        {
            string[] topics = { "Phân tích văn học", "Bài toán thực tế", "Vấn đề xã hội", "Khoa học môi trường" };
            string topic = topics[random.Next(topics.Length)];

            // Sử dụng EssayQuestionViewModel thay vì QuestionViewModel
            var question = new EssayQuestionViewModel
            {
                Type = QuestionType.Essay,
                Title = $"Câu {index}: Trình bày chi tiết",
                Content = GenerateRandomEssayQuestion(topic),
                Solution = GenerateRandomEssayGuidelines(topic),
                EssayMaxLength = 1000
            };

            return question;
        }

        private static string[] GenerateRandomStatements(string domain)
        {
            switch (domain)
            {
                case "Khoa học":
                    return new string[]
                    {
                        "Nước sôi ở nhiệt độ 100°C ở áp suất tiêu chuẩn.",
                        "Mặt trời quay quanh Trái đất.",
                        "Vận tốc ánh sáng lớn hơn vận tốc âm thanh.",
                        "Con người chỉ sử dụng 10% não bộ."
                    };
                case "Lịch sử":
                    return new string[]
                    {
                        "Việt Nam độc lập năm 1945.",
                        "Mỹ độc lập năm 1876.",
                        "Chiến tranh thế giới thứ hai kết thúc năm 1945.",
                        "Trung Quốc thành lập vào năm 1912."
                    };
                default:
                    return new string[]
                    {
                        "Trái đất có một vệ tinh tự nhiên là mặt trăng.",
                        "Sao Hỏa có kích thước lớn hơn Trái đất.",
                        "Con người đã đặt chân lên mặt trăng vào năm 1969.",
                        "Mặt trời là hành tinh lớn nhất trong hệ mặt trời."
                    };
            }
        }

        private static string GenerateRandomShortQuestion(string subject)
        {
            switch (subject)
            {
                case "Lịch sử":
                    return "Ai là người đọc bản Tuyên ngôn độc lập ngày 2/9/1945?";
                case "Địa lý":
                    return "Đâu là quốc gia có diện tích lớn nhất thế giới?";
                case "Văn học":
                    return "Tác giả của tác phẩm \"Truyện Kiều\" là ai?";
                case "Khoa học":
                    return "H₂O là công thức hóa học của chất gì?";
                default:
                    return "Trong bóng đá, thẻ đỏ có ý nghĩa gì?";
            }
        }

        private static string GenerateRandomShortAnswer(string subject)
        {
            switch (subject)
            {
                case "Lịch sử":
                    return "Chủ tịch Hồ Chí Minh.";
                case "Địa lý":
                    return "Liên bang Nga.";
                case "Văn học":
                    return "Đại thi hào Nguyễn Du.";
                case "Khoa học":
                    return "Nước.";
                default:
                    return "Buộc cầu thủ phải rời sân (truất quyền thi đấu).";
            }
        }

        private static string GenerateRandomEssayQuestion(string topic)
        {
            switch (topic)
            {
                case "Phân tích văn học":
                    return "Phân tích nhân vật Chí Phèo trong tác phẩm \"Chí Phèo\" của Nam Cao, từ đó làm rõ chủ đề của tác phẩm.";
                case "Bài toán thực tế":
                    return "Một công ty có kế hoạch sản xuất 2 loại sản phẩm A và B với lợi nhuận lần lượt là 2 triệu và 3 triệu đồng/sản phẩm. Để sản xuất A cần 1 giờ ở máy 1 và 2 giờ ở máy 2. Để sản xuất B cần 2 giờ ở máy 1 và 1 giờ ở máy 2. Tổng thời gian hoạt động của máy 1 và máy 2 lần lượt là 12 và 8 giờ một ngày. Hãy tìm số sản phẩm A và B cần sản xuất để tối đa hóa lợi nhuận.";
                case "Vấn đề xã hội":
                    return "Phân tích những mặt tích cực và tiêu cực của mạng xã hội đối với giới trẻ hiện nay. Đề xuất các giải pháp để hạn chế tác động tiêu cực.";
                default:
                    return "Phân tích các nguyên nhân chính dẫn đến biến đổi khí hậu và đề xuất các giải pháp để giảm thiểu tác động của nó đối với môi trường.";
            }
        }

        private static string GenerateRandomEssayGuidelines(string topic)
        {
            switch (topic)
            {
                case "Phân tích văn học":
                    return "Gợi ý:\n- Giới thiệu tác giả Nam Cao và tác phẩm \"Chí Phèo\"\n- Phân tích quá trình biến đổi nhân cách của Chí Phèo: từ người lương thiện -> lưu manh -> khát khao hoàn lương -> bi kịch\n- Phân tích nguyên nhân bi kịch của Chí Phèo\n- Làm rõ chủ đề phản ánh xã hội thực dân nửa phong kiến\n- Nghệ thuật xây dựng nhân vật của Nam Cao";
                case "Bài toán thực tế":
                    return "Gợi ý:\n- Đặt x, y lần lượt là số sản phẩm A và B cần sản xuất\n- Lập hệ bất phương trình: x + 2y ≤ 12 (giới hạn máy 1), 2x + y ≤ 8 (giới hạn máy 2), x ≥ 0, y ≥ 0\n- Hàm mục tiêu: f(x,y) = 2x + 3y (triệu đồng)\n- Vẽ đồ thị và tìm điểm tối ưu trong các đỉnh của miền nghiệm\n- Kết quả: sản xuất 3 sản phẩm A và 4.5 sản phẩm B, lợi nhuận tối đa là 19.5 triệu";
                default:
                    return "Gợi ý:\n- Mở bài: Nêu hiện trạng vấn đề và ý nghĩa\n- Thân bài: Phân tích rõ các nguyên nhân, hậu quả, dẫn chứng cụ thể\n- Kết bài: Tóm tắt và đề xuất giải pháp thiết thực\n- Chú ý: Cần có dẫn chứng cụ thể, số liệu chính xác, lập luận chặt chẽ";
            }
        }
    }
}