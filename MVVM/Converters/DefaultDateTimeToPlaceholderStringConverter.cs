using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GenotypeApplication.MVVM.Converters
{
    public class DefaultDateTimeToPlaceholderStringConverter : IValueConverter
    {
        private const string Placeholder = "-";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime && (dateTime == DateTime.MinValue || dateTime == default))
                return parameter?.ToString() ?? Placeholder;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
