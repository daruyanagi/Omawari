using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Omawari.Converters
{
    public class TimeSpanToEasyReadStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;

            if (!(value is TimeSpan)) return value;

            var span = (TimeSpan)value;

            if (span.Minutes > 0) return $"{span.Minutes} min {span.Seconds} sec";
            else return $"{span.Seconds} sec {span.Milliseconds}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
