using System.Windows.Media;

namespace GenotypeApplication.Models.Structure.Data_file.Highlights
{
    public class HighlightRegionModel
    {
        public int StartRow { get; }
        public int EndRow { get; }
        public int StartCol { get; }
        public int EndCol { get; }
        public string ParameterName { get; }
        public Color Color { get; }

        public HighlightRegionModel(
            int startRow, int endRow,
            int startCol, int endCol,
            string parameterName, Color color)
        {
            StartRow = startRow;
            EndRow = endRow;
            StartCol = startCol;
            EndCol = endCol;
            ParameterName = parameterName;
            Color = color;
        }
    }
}
