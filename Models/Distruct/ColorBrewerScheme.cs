using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Distruct
{
    public class ColorBrewerScheme
    {
        public string Family { get; set; } = string.Empty;        // "Accent", "Set1", "Paired"...
        public string Type { get; set; } = string.Empty;          // "qual", "div", "seq"
        public int MaxColors { get; set; }        // максимальное количество цветов в семействе

        // Генерирует имена цветов для заданного K
        // Например: Accent, K=5 → ["Accent_5_qual_1", ..., "Accent_5_qual_5"]
        public List<string> GetColorNames(int k)
        {
            return Enumerable.Range(1, k)
                .Select(i => $"{Family}_{k}_{Type}_{i}")
                .ToList();
        }
    }
}
