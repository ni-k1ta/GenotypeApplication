using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GenotypeApplication.MVVM.Converters
{
    public class DefaultIntToPlaceholderStringConverter : IValueConverter
    {
        private const string Placeholder = "Not specified";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int defaultValue && defaultValue == 0)
                return parameter?.ToString() ?? Placeholder;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
