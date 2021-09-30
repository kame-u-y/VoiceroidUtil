using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RucheHome.Windows.Mvvm.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public class BooleanAndConverter : IMultiValueConverter
    {
        /// <summary>
        /// 複数の条件によってIsEnabledを指定するためのConverter
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            foreach (object v in values)
            {
                if ((v is bool) && (bool)v == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(
            object value, 
            Type[] targetType, 
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
