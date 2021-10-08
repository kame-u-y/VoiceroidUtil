using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace VoiceroidUtil.View
{
    /// <summary>
    /// TRT's拡張：オブジェクトのActualWidth値をバインディングするための添付ビヘイビア
    /// プレビューのテキスト幅の取得に利用
    /// </summary>
    public static class ActualWidthObserver
    {
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached(
                "Observe",
                typeof(bool),
                typeof(ActualWidthObserver),
                new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedWidthProperty =
            DependencyProperty.RegisterAttached(
                "ObservedWidth",
                typeof(double),
                typeof(ActualWidthObserver));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                return (bool)frameworkElement.GetValue(ObserveProperty);
            }
            else
            {
                return false;
            }
        }
            
        public static void SetObserve(FrameworkElement frameworkElement, bool value)
        {
            if (frameworkElement != null)
            {
                frameworkElement.SetValue(ObserveProperty, value);
            }
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                return (double)frameworkElement.GetValue(ObservedWidthProperty);
            }
            else
            {
                return 0;
            }
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double value)
        {
            if (frameworkElement != null)
            {
                frameworkElement.SetValue(ObservedWidthProperty, value);
            }
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs ev)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)ev.NewValue) {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedWidthForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs ev)
        {
            UpdateObservedWidthForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedWidthForFrameworkElement(FrameworkElement frameworkElement)
        {
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
        }
    }
}
