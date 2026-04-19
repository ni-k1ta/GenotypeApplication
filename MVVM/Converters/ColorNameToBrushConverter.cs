using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GenotypeApplication.MVVM.Converters
{
    public class ColorNameToBrushConverter : IValueConverter
    {
        private static readonly Dictionary<string, Color> ColorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["orange"] = Color.FromRgb(241, 158, 92),
            ["blue"] = Color.FromRgb(67, 151, 224),
            ["yellow"] = Color.FromRgb(235, 254, 83),
            ["pink"] = Color.FromRgb(241, 158, 226),
            ["green"] = Color.FromRgb(82, 151, 66),
            ["purple"] = Color.FromRgb(117, 20, 124),
            ["red"] = Color.FromRgb(234, 51, 82),
            ["light_green"] = Color.FromRgb(117, 251, 76),
            ["dark_blue"] = Color.FromRgb(0, 0, 245),
            ["light_purple"] = Color.FromRgb(234, 51, 247),
            ["light_yellow"] = Color.FromRgb(251, 231, 163),
            ["brown"] = Color.FromRgb(166, 83, 30),
            ["light_blue"] = Color.FromRgb(117, 251, 253),
            ["olive_green"] = Color.FromRgb(128, 128, 38),
            ["peach"] = Color.FromRgb(241, 158, 156),
            ["sea_green"] = Color.FromRgb(55, 126, 127),
            ["yellow_green"] = Color.FromRgb(161, 190, 70),
            ["blue_purple"] = Color.FromRgb(106, 43, 221),
            ["blue_green"] = Color.FromRgb(93, 188, 155),
            ["gray"] = Color.FromRgb(128, 128, 128),
            ["dark_green"] = Color.FromRgb(46, 100, 31),
            ["light_gray"] = Color.FromRgb(191, 191, 191),
            ["red2"] = Color.FromRgb(234, 51, 35),
            ["light_blue2"] = Color.FromRgb(170, 228, 252),
            ["light_orange"] = Color.FromRgb(241, 158, 112),
            ["dark_gray"] = Color.FromRgb(64, 64, 64),
            ["light_pink"] = Color.FromRgb(251, 231, 230),
            ["dark_brown"] = Color.FromRgb(141, 58, 55),
            ["dark_orange"] = Color.FromRgb(237, 112, 45),
            ["dark_purple"] = Color.FromRgb(46, 4, 74),
            ["white"] = Color.FromRgb(255, 255, 255),
            ["black"] = Color.FromRgb(0, 0, 0),

            ["color32"] = Color.FromRgb(236, 91, 41),
            ["color33"] = Color.FromRgb(241, 158, 56),
            ["color34"] = Color.FromRgb(236, 91, 152),
            ["color35"] = Color.FromRgb(241, 158, 226),
            ["color36"] = Color.FromRgb(255, 255, 166),
            ["color37"] = Color.FromRgb(255, 255, 232),
            ["color38"] = Color.FromRgb(251, 231, 230),
            ["color39"] = Color.FromRgb(235, 254, 109),
            ["color40"] = Color.FromRgb(230, 230, 253),
            ["color41"] = Color.FromRgb(219, 156, 249),
            ["color42"] = Color.FromRgb(177, 252, 106),
            ["color43"] = Color.FromRgb(177, 252, 163),
            ["color44"] = Color.FromRgb(177, 252, 231),
            ["color45"] = Color.FromRgb(177, 252, 254),
            ["color46"] = Color.FromRgb(170, 228, 252),
            ["color47"] = Color.FromRgb(153, 153, 248),
            ["color48"] = Color.FromRgb(143, 81, 246),
            ["color49"] = Color.FromRgb(96, 151, 248),
            ["color50"] = Color.FromRgb(117, 251, 162),
            ["color51"] = Color.FromRgb(117, 251, 230),
            ["color52"] = Color.FromRgb(105, 227, 251),
            ["color53"] = Color.FromRgb(187, 48, 39),
            ["color54"] = Color.FromRgb(166, 83, 30),
            ["color55"] = Color.FromRgb(164, 61, 38),
            ["color56"] = Color.FromRgb(163, 44, 55),
            ["color57"] = Color.FromRgb(146, 104, 33),
            ["color58"] = Color.FromRgb(143, 81, 39),
            ["color59"] = Color.FromRgb(82, 101, 79),
            ["color60"] = Color.FromRgb(140, 39, 77),

            ["color101"] = Color.FromRgb(243, 180, 179),
            ["color102"] = Color.FromRgb(214, 131, 129),
            ["color103"] = Color.FromRgb(189, 89, 85),
            ["color104"] = Color.FromRgb(163, 56, 50),
            ["color105"] = Color.FromRgb(139, 33, 26),
            ["color106"] = Color.FromRgb(226, 215, 181),
            ["color107"] = Color.FromRgb(191, 177, 132),
            ["color108"] = Color.FromRgb(159, 142, 89),
            ["color109"] = Color.FromRgb(129, 112, 55),
            ["color110"] = Color.FromRgb(103, 85, 30),
            ["color111"] = Color.FromRgb(233, 253, 185),
            ["color112"] = Color.FromRgb(201, 227, 138),
            ["color113"] = Color.FromRgb(170, 202, 99),
            ["color114"] = Color.FromRgb(141, 176, 69),
            ["color115"] = Color.FromRgb(117, 151, 48),
            ["color116"] = Color.FromRgb(187, 227, 251),
            ["color117"] = Color.FromRgb(141, 192, 224),
            ["color118"] = Color.FromRgb(102, 160, 199),
            ["color119"] = Color.FromRgb(70, 130, 173),
            ["color120"] = Color.FromRgb(48, 105, 148),
            ["color121"] = Color.FromRgb(188, 177, 249),
            ["color122"] = Color.FromRgb(139, 127, 221),
            ["color123"] = Color.FromRgb(98, 82, 196),
            ["color124"] = Color.FromRgb(63, 45, 170),
            ["color125"] = Color.FromRgb(35, 16, 146),
            ["color126"] = Color.FromRgb(254, 254, 165),
            ["color127"] = Color.FromRgb(254, 254, 84),
            ["color128"] = Color.FromRgb(246, 205, 69),
            ["color129"] = Color.FromRgb(240, 157, 56),
            ["color130"] = Color.FromRgb(236, 112, 45),
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
