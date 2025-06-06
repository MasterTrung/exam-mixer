using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DragDropTreeApp
{
    public class ContentElement : INotifyPropertyChanged
    {
        public bool IsImportedFromWord { get; set; } = false;

        private string _type;  // "text" hoặc "image"
        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged(nameof(Image)); }
        }

        public string ImagePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public BitmapImage Image { get; set; };
    }
}