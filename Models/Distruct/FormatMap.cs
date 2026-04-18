using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GenotypeApplication.Constants.DistructConstants;

namespace GenotypeApplication.Models.Distruct
{
    public static class FormatMap
    {
        private static readonly Dictionary<OutputFormat, FormatSettings> Map = new()
        {
            [OutputFormat.Pdf] = new FormatSettings
            {
                Device = "pdfwrite",
                Extension = ".pdf",
                NeedsResolution = false
            },
            [OutputFormat.Png] = new FormatSettings
            {
                Device = "png16m",      // полноцветный PNG (24 бит)
                Extension = ".png",
                NeedsResolution = true
            },
            [OutputFormat.Jpeg] = new FormatSettings
            {
                Device = "jpeg",
                Extension = ".jpg",
                NeedsResolution = true
            },
            [OutputFormat.Bmp] = new FormatSettings
            {
                Device = "bmp16m",      // полноцветный BMP (24 бит)
                Extension = ".bmp",
                NeedsResolution = true
            }
        };

        public static FormatSettings Get(OutputFormat format)
        {
            return Map[format];
        }
    }
}
