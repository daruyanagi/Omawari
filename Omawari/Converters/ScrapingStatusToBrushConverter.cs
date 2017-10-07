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
            if (value == null) return value;

            if (value is ScrapingStatus)
            {
                switch (((ScrapingStatus)value))
                {
                    case ScrapingStatus.Pending: return Brushes.Gray;
                    case ScrapingStatus.Running: return Brushes.Blue;
                    case ScrapingStatus.Succeeded: return Brushes.Green; // ToDo: 実質廃止
                    case ScrapingStatus.NoChanged: return Brushes.Green;
                    case ScrapingStatus.Updated: return Brushes.Orange;
                    case ScrapingStatus.New: return Brushes.Orange;
                    default: return Brushes.Red;
                }
            }

            if (value is string)
            {
                switch (((string)value))
                {
                    case "No Changed": return Brushes.Green;
                    case "success": return Brushes.Green; // 廃止
                    case "New": return Brushes.Orange;
                    case "Updated": return Brushes.Orange;
                    case "Empty": return Brushes.Red;
                    case "Failed": return Brushes.Red;
                    case "fail": return Brushes.Red; // 廃止
                    default: return Brushes.Black;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
