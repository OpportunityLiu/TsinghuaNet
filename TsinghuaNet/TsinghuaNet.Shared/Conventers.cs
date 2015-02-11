using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using System.Globalization;

namespace TsinghuaNet
{
    class DateTimeToYearMonthStringConverter:IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((DateTime)value).ToString(CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern,CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
