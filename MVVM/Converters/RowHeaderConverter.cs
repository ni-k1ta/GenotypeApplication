using GenotypeApplication.Models.Structure.Data_file.Highlights;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GenotypeApplication.MVVM.Converters
{
    public class RowHeaderConverter : IValueConverter
    {
        private readonly HighlightMapModel _map;

        public RowHeaderConverter(HighlightMapModel map)
        {
            _map = map;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DataRowView rowView)
                return DependencyProperty.UnsetValue;

            var table = rowView.Row.Table;
            int rowIndex = table.Rows.IndexOf(rowView.Row);

            if (_map.RowHeaders.TryGetValue(rowIndex, out var headerText))
                return headerText;

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
