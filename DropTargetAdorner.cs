using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DragDropTreeApp
{
    public class DropTargetAdorner : Adorner
    {
        private readonly Brush _brush;
        private readonly Pen _pen;

        public DropTargetAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _brush = new SolidColorBrush(Color.FromArgb(30, 0, 120, 215)); // Semi-transparent light blue
            _pen = new Pen(new SolidColorBrush(Colors.DodgerBlue), 2);
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect rect = new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(_brush, _pen, rect);
        }
    }
}