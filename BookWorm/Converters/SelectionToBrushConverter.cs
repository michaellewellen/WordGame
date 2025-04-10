using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Data;

namespace BookWorm.Converters
{
    public class SelectionToBrushConverter : IValueConverter
    {
        private static readonly Brush SelectedBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0));
        private static readonly Brush DefaultBrush = new SolidColorBrush(Color.FromRgb(40, 40, 40));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? SelectedBrush : DefaultBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
