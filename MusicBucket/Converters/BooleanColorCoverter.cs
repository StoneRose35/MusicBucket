using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
namespace MusicBucket.Converters
{
    [ValueConversion(typeof(bool),typeof(Brush))]
    public class BooleanColorCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
            else 
            {
                return  new SolidColorBrush(Color.FromRgb(180, 0, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((Brush)value) == new SolidColorBrush(Color.FromRgb(0, 255, 0)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
