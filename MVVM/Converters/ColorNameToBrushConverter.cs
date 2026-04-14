using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GenotypeApplication.MVVM.Converters
{
    public class ColorNameToBrushConverter : IValueConverter
    {
        private static readonly Dictionary<string, Color> ColorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["orange"] = Color.FromRgb(255, 165, 0),
            ["blue"] = Color.FromRgb(0, 0, 255),
            ["yellow"] = Color.FromRgb(255, 255, 0),
            ["pink"] = Color.FromRgb(255, 192, 203),
            ["green"] = Color.FromRgb(0, 128, 0),
            ["purple"] = Color.FromRgb(128, 0, 128),
            ["red"] = Color.FromRgb(255, 0, 0),
            ["light_green"] = Color.FromRgb(144, 238, 144),
            ["dark_blue"] = Color.FromRgb(0, 0, 139),
            ["light_purple"] = Color.FromRgb(200, 162, 200),
            ["light_yellow"] = Color.FromRgb(255, 255, 224),
            ["brown"] = Color.FromRgb(139, 69, 19),
            ["light_blue"] = Color.FromRgb(173, 216, 230),
            ["olive_green"] = Color.FromRgb(107, 142, 35),
            ["peach"] = Color.FromRgb(255, 218, 185),
            ["sea_green"] = Color.FromRgb(46, 139, 87),
            ["yellow_green"] = Color.FromRgb(154, 205, 50),
            ["blue_purple"] = Color.FromRgb(104, 58, 183),
            ["blue_green"] = Color.FromRgb(0, 149, 182),
            ["gray"] = Color.FromRgb(128, 128, 128),
            ["dark_green"] = Color.FromRgb(0, 100, 0),
            ["light_gray"] = Color.FromRgb(211, 211, 211),
            ["red2"] = Color.FromRgb(220, 20, 60),
            ["light_blue2"] = Color.FromRgb(135, 206, 250),
            ["light_orange"] = Color.FromRgb(255, 200, 100),
            ["dark_gray"] = Color.FromRgb(80, 80, 80),
            ["light_pink"] = Color.FromRgb(255, 182, 193),
            ["dark_brown"] = Color.FromRgb(101, 67, 33),
            ["dark_orange"] = Color.FromRgb(255, 140, 0),
            ["dark_purple"] = Color.FromRgb(75, 0, 130),
            ["white"] = Color.FromRgb(255, 255, 255),
            ["black"] = Color.FromRgb(0, 0, 0),
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name && ColorMap.TryGetValue(name, out var color))
                return new SolidColorBrush(color);
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
