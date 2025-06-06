using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.ObjectModel;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using TableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
namespace DragDropTreeApp
{
    public class OpenXmlWordImporter
    {
        public static void ImportQuestionsFromWord(string filePath, ObservableCollection<QuestionViewModel> questions)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var tables = wordDoc.MainDocumentPart.Document.Body.Elements<Table>().ToList();

                foreach (var tbl in tables)
                {
                    var rows = tbl.Elements<TableRow>().ToList();

                    int i = 0;
                    while (i < rows.Count)
                    {
                        var cells = rows[i].Elements<TableCell>().ToList();
                        if (cells.Count >= 4)
                        {
                            string style = GetCellText(rows[i], 3).Trim();
                            if (style == "ABCD" && i + 6 < rows.Count)
                            {
                                // Trắc nghiệm - tạo MultipleChoiceQuestionViewModel
                                var q = new MultipleChoiceQuestionViewModel
                                {
                                    Type = QuestionType.MultipleChoice,
                                    Title = "Câu hỏi trắc nghiệm",
                                    //Content = GetCellText(rows[i + 1], 3),
                                    Solution = GetCellText(rows[i + 6], 3),
                                    IsImportedFromWord = true
                                };

                                q.RichContent = new ObservableCollection<ContentElement>();
                                GetCellContentWithImages(wordDoc, rows[i + 1].Elements<TableCell>().ElementAt(3), q.RichContent);
                               // q.Content = string.Join("\n", q.RichContent.Where(x => x.Type == "text").Select(x => x.Text));
                                q.Solution = GetCellText(rows[i + 6], 3);


                                string groupName = "group_" + System.Guid.NewGuid().ToString("N");
                                for (int j = 0; j < 4; j++)
                                {
                                    string answerText = GetCellText(rows[i + 2 + j], 3);
                                    bool isCorrect = GetCellText(rows[i + 2 + j], 1).Trim().ToLower() == "x";
                                    q.Answers.Add(new AnswerViewModel
                                    {
                                        Text = answerText,
                                        GroupName = groupName,
                                        IsCorrect = isCorrect,
                                        IsSelected = isCorrect       // <-- Quan trọng
                                    });
                                }
                                questions.Add(q);
                                i += 7;
                                continue;
                            }
                            else if (style == "DS" && i + 6 < rows.Count)
                            {
                                // Đúng/Sai - tạo TrueFalseQuestionViewModel
                                var q = new TrueFalseQuestionViewModel
                                {
                                    Type = QuestionType.TrueFalse,
                                    Title = "Câu hỏi Đúng/Sai",
                                  //  Content = GetCellText(rows[i + 1], 3),
                                    Solution = GetCellText(rows[i + 6], 3),
                                    IsImportedFromWord = true
                                };

                                q.RichContent = new ObservableCollection<ContentElement>();
                                GetCellContentWithImages(wordDoc, rows[i + 1].Elements<TableCell>().ElementAt(3), q.RichContent);
                                // Sau khi gọi GetCellContentWithImages(...)
                                if (q.RichContent.Any(x => x.Type == "image" && x.Image != null))
                                {
                                    System.Diagnostics.Debug.WriteLine(">>> ĐÃ LẤY ĐƯỢC ẢNH VÀO RichContent <<<");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine(">>> KHÔNG LẤY ĐƯỢC ẢNH NÀO <<<");
                                }
                               // q.Content = string.Join("\n", q.RichContent.Where(x => x.Type == "text").Select(x => x.Text));
                                q.Solution = GetCellText(rows[i + 6], 3);

                                for (int j = 0; j < 4; j++)
                                {
                                    string statementText = GetCellText(rows[i + 2 + j], 3);
                                    bool isTrue = GetCellText(rows[i + 2 + j], 1).Trim().ToLower() == "x";
                                    string difficultyStr = GetCellText(rows[i + 2 + j], 2).Trim();
                                    DifficultyLevel? difficulty = null;
                                    if (difficultyStr == "1")
                                        difficulty = DifficultyLevel.Easy;
                                    else if (difficultyStr == "2")
                                        difficulty = DifficultyLevel.Medium;
                                    else if (difficultyStr == "3")
                                        difficulty = DifficultyLevel.Hard;
                                    // nếu trống thì giữ null

                                    q.Statements.Add(new TrueFalseStatementViewModel
                                    {
                                        Text = statementText,
                                        IsTrue = isTrue,
                                        UserAnswer = isTrue,
                                        Difficulty = difficulty
                                    });
                                }
                                questions.Add(q);
                                i += 7;
                                continue;
                            }
                            else if (style == "TL" && i + 2 < rows.Count)
                            {
                                // Tự luận ngắn - tạo ShortAnswerQuestionViewModel
                                var q = new ShortAnswerQuestionViewModel
                                {
                                    Type = QuestionType.ShortAnswer,
                                    Title = "Câu hỏi tự luận",
                                   // Content = GetCellText(rows[i + 1], 3),
                                    Solution = GetCellText(rows[i + 2], 3),
                                    IsImportedFromWord = true
                                };

                                q.RichContent = new ObservableCollection<ContentElement>();
                                GetCellContentWithImages(wordDoc, rows[i + 1].Elements<TableCell>().ElementAt(3), q.RichContent);
                               // q.Content = string.Join("\n", q.RichContent.Where(x => x.Type == "text").Select(x => x.Text));
                                q.Solution = GetCellText(rows[i + 2], 3);

                                questions.Add(q);
                                i += 3;
                                continue;
                            }
                        }
                        i++;
                    }
                }
            }
        }

        private static string GetCellText(TableRow row, int cellIndex)
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cellIndex >= cells.Count) return "";
            return string.Concat(cells[cellIndex].Descendants<Text>().Select(t => t.Text)).Trim();
        }


        private static void GetCellContentWithImages(WordprocessingDocument wordDoc, TableCell cell, ObservableCollection<ContentElement> collection)
        {
            // Nếu cell không có Paragraph, vẫn lấy text thuần
            var allPara = cell.Elements<Paragraph>().ToList();
            if (allPara.Count == 0)
            {
                string cellText = string.Concat(cell.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text)).Trim();
                if (!string.IsNullOrWhiteSpace(cellText))
                    collection.Add(new ContentElement { Type = "text", Text = cellText });
            }
            else
            {
                foreach (var para in allPara)
                {
                    // Lấy toàn bộ text trong paragraph
                    string paraText = string.Concat(para.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text)).Trim();
                    if (!string.IsNullOrWhiteSpace(paraText))
                        collection.Add(new ContentElement { Type = "text", Text = paraText });

                    // Lấy ảnh trong paragraph
                    foreach (var run in para.Elements<Run>())
                    {
                        foreach (var drawing in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Drawing>())
                        {
                            foreach (var blip in drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>())
                            {
                                string embedId = blip.Embed.Value;
                                var imgPart = (ImagePart)wordDoc.MainDocumentPart.GetPartById(embedId);

                                using (var stream = imgPart.GetStream())
                                {
                                    var image = new BitmapImage();
                                    using (var ms = new MemoryStream())
                                    {
                                        stream.CopyTo(ms);
                                        ms.Position = 0;
                                        image.BeginInit();
                                        image.StreamSource = ms;
                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                        image.EndInit();
                                        image.Freeze();
                                    }
                                    collection.Add(new ContentElement { Type = "image", Image = image });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}