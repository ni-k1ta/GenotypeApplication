using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GenotypeApplication.Models.Distruct
{
    public class PreviewItem
    {
        public int K { get; init; }
        public BitmapImage Image { get; init; } = null!;
    }
}
