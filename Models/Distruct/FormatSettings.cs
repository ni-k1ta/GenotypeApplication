using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Distruct
{
    public class FormatSettings
    {
        // значение для -sDEVICE=
        public string Device { get; set; } = string.Empty;

        // расширение выходного файла (.pdf, .png и т.д.)
        public string Extension { get; set; } = string.Empty;

        // нужен ли параметр -r (разрешение) — для растровых форматов
        public bool NeedsResolution { get; set; }
    }
}
