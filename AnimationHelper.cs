using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace DragDropTreeApp
{
    public static class AnimationHelper
    {
        // Hiệu ứng thêm một item mới vào danh sách
        public static void AnimateAddItem(FrameworkElement element)
        {
            // Lưu trữ kích thước ban đầu
            double originalHeight = element.ActualHeight;
            double originalWidth = element.ActualWidth;

            element.Opacity = 0;
            element.RenderTransform = new ScaleTransform(0.8, 0.8);

            // Animation cho opacity
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Animation cho scale
            ScaleTransform scale = element.RenderTransform as ScaleTransform;
            DoubleAnimation scaleX = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new ElasticEase
                {
                    Oscillations = 1,
                    Springiness = 3,
                    EasingMode = EasingMode.EaseOut
                }
            };

            DoubleAnimation scaleY = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new ElasticEase
                {
                    Oscillations = 1,
                    Springiness = 3,
                    EasingMode = EasingMode.EaseOut
                }
            };

            // Chạy animation
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
        }

        // Hiệu ứng xóa một item khỏi danh sách
        public static void AnimateRemoveItem(FrameworkElement element, Action completedAction)
        {
            element.RenderTransform = new ScaleTransform(1, 1);

            // Animation cho opacity
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            // Animation cho scale
            ScaleTransform scale = element.RenderTransform as ScaleTransform;
            DoubleAnimation scaleX = new DoubleAnimation
            {
                From = 1,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            DoubleAnimation scaleY = new DoubleAnimation
            {
                From = 1,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            // Đăng ký sự kiện hoàn thành
            fadeOut.Completed += (s, e) => completedAction?.Invoke();

            // Chạy animation
            element.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
        }
    }
}