using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Distruct
{
    public static class ColorBrewerData
    {
        public static List<ColorBrewerScheme> AllSchemes { get; } = new()
    {
        // Qualitative
        new() { Family = "Accent",  Type = "qual", MaxColors = 8 },
        new() { Family = "Dark2",   Type = "qual", MaxColors = 8 },
        new() { Family = "Paired",  Type = "qual", MaxColors = 12 },
        new() { Family = "Pastel1", Type = "qual", MaxColors = 9 },
        new() { Family = "Pastel2", Type = "qual", MaxColors = 8 },
        new() { Family = "Set1",    Type = "qual", MaxColors = 9 },
        new() { Family = "Set2",    Type = "qual", MaxColors = 8 },
        new() { Family = "Set3",    Type = "qual", MaxColors = 12 },

        // Diverging
        new() { Family = "BrBG",     Type = "div", MaxColors = 11 },
        new() { Family = "PiYG",     Type = "div", MaxColors = 11 },
        new() { Family = "PRGn",     Type = "div", MaxColors = 11 },
        new() { Family = "PuOr",     Type = "div", MaxColors = 11 },
        new() { Family = "RdBu",     Type = "div", MaxColors = 11 },
        new() { Family = "RdGy",     Type = "div", MaxColors = 11 },
        new() { Family = "RdYlBu",   Type = "div", MaxColors = 11 },
        new() { Family = "RdYlGn",   Type = "div", MaxColors = 11 },
        new() { Family = "Spectral", Type = "div", MaxColors = 11 },

        // Sequential
        new() { Family = "Blues",   Type = "seq", MaxColors = 9 },
        new() { Family = "BuGn",    Type = "seq", MaxColors = 9 },
        new() { Family = "BuPu",    Type = "seq", MaxColors = 9 },
        new() { Family = "GnBu",    Type = "seq", MaxColors = 9 },
        new() { Family = "Greens",  Type = "seq", MaxColors = 9 },
        new() { Family = "Greys",   Type = "seq", MaxColors = 9 },
        new() { Family = "Oranges", Type = "seq", MaxColors = 9 },
        new() { Family = "OrRd",    Type = "seq", MaxColors = 9 },
        new() { Family = "PuBu",    Type = "seq", MaxColors = 9 },
        new() { Family = "PuBuGn",  Type = "seq", MaxColors = 9 },
        new() { Family = "PuRd",    Type = "seq", MaxColors = 9 },
        new() { Family = "Purples", Type = "seq", MaxColors = 9 },
        new() { Family = "RdPu",    Type = "seq", MaxColors = 9 },
        new() { Family = "Reds",    Type = "seq", MaxColors = 9 },
        new() { Family = "YlGn",    Type = "seq", MaxColors = 9 },
        new() { Family = "YlGnBu",  Type = "seq", MaxColors = 9 },
        new() { Family = "YlOrBr",  Type = "seq", MaxColors = 9 },
        new() { Family = "YlOrRd",  Type = "seq", MaxColors = 9 }
    };

        public static List<ColorBrewerScheme> GetAvailableSchemes(int kMax)
        {
            return AllSchemes.Where(s => s.MaxColors >= kMax).ToList();
        }
    }
}
