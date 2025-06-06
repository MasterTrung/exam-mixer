using System;
using System.Windows;
using System.IO;

namespace DragDropTreeApp
{
    public partial class App : Application
    {
        // Thêm vào App.xaml.cs
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    // Tạo ViewModel chính cho ứng dụng
        //    var questionsCollection = new QuestionsCollection();

        //    // Kiểm tra tham số dòng lệnh
        //    if (e.Args.Length > 0)
        //    {
        //        string jsonFilePath = e.Args[0];

        //        if (File.Exists(jsonFilePath))
        //        {
        //            // Import câu hỏi từ file JSON
        //            var importedQuestions = WordImporter.ImportFromJson(jsonFilePath);

        //            // Thêm vào collection
        //            foreach (var question in importedQuestions)
        //            {
        //                questionsCollection.Questions.Add(question);
        //            }
        //        }
        //    }

        //    //// Khởi tạo MainWindow với DataContext
        //    //MainWindow mainWindow = new MainWindow();
        //    //mainWindow.DataContext = questionsCollection;
        //    //mainWindow.Show();
        //}
    }
}