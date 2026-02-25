using System.Globalization;
using System.Windows.Data;

namespace GenotypeApplication.MVVM.Converters
{
    public class EmptyStringToPlaceholderStringConverter : IValueConverter
    {
        private const string Placeholder = "Not specified";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return parameter?.ToString() ?? Placeholder;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
