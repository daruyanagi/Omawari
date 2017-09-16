using Omawari.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Omawari.Converters
{
    public class ScrapingStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ScrapingStatus))
                throw new ArgumentException("not ScrapingStatus", "value");
            
            switch (((ScrapingStatus)value))
            {
                case ScrapingStatus.Failed: return Brushes.Red;
                case ScrapingStatus.Running: return Brushes.Gray;
                case ScrapingStatus.Succeeded: return Brushes.Green;
                default: break;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
