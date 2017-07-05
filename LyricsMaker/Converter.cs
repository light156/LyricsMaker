using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Controls;

namespace LyricsMaker
{
    class Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int planned = (int)(MainWindow.duration.TotalSeconds * (double)value / 10);
                string plannedstr = planned / 60 + ":" + (planned % 60) / 10 + (planned % 60) % 10;
                return plannedstr.PadLeft(5, '0');
            }
            catch (Exception ex)
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();  
        }
    }
}
