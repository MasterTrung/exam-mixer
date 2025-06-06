using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Controls;
namespace DragDropTreeApp
{
    public class DragAdorner : Adorner
    {
        private readonly FrameworkElement _child;
        private double _offsetX;
        private double _offsetY;
        private readonly Point _initialMousePosition;
        private readonly DropShadowEffect _shadowEffect;

        public DragAdorner(UIElement adornedElement, FrameworkElement dragElement, Point initialMousePosition)
            : base(adornedElement)
        {
            _child = dragElement;
            _initialMousePosition = initialMousePosition;
            IsHitTestVisible = false;

            // Thêm hiệu ứng đổ bóng
            _shadowEffect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 5,
                Opacity = 0.6,
                BlurRadius = 10
            };

            // Áp dụng hiệu ứng cho child element
            _child.Effect = _shadowEffect;

            // Tạo container cho item đang kéo
            Border border = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(4),
                Child = _child,
                Padding = new Thickness(5)
            };

            // Set the child to the border instead
            _child = border;
        }

        public void UpdatePosition(Point currentPosition)
        {
            _offsetX = currentPosition.X - _initialMousePosition.X;
            _offsetY = currentPosition.Y - _initialMousePosition.Y;
            InvalidateVisual();
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException();
            return _child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(new Point(_offsetX, _offsetY), finalSize));
            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.PushOpacity(0.8); // Tăng opacity lên một chút
            base.OnRender(drawingContext);
            drawingContext.Pop();
        }
    }
}