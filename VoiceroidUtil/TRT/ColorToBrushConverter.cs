using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VoiceroidUtil.TRToys
{
    /// <summary>
    /// 
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        => (value is Color c) ?
            new SolidColorBrush(c) : DependencyProperty.UnsetValue;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        => (value is SolidColorBrush sc) ?
            sc.Color : DependencyProperty.UnsetValue;
    }
}
