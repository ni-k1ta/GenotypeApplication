using System.Windows.Media;

namespace GenotypeApplication.Models.Structure.Data_file.Highlights
{
    public class HighlightMapModel
    {
        public IReadOnlyList<HighlightRegionModel> ColumnRegions { get; }

        public IReadOnlyList<HighlightRegionModel> RowRegions { get; }

        public HighlightRegionModel? DataBlock { get; }

        public IReadOnlyList<HighlightRegionModel> PhaseInfoRegions { get; }


        public int? MissingValue { get; }
        public int? NotAmbiguousValue { get; }
        public Color MissingCellBorderColor { get; }
        public Color NotAmbiguousCellBorderColor { get; }

        public HighlightMapModel(
            IReadOnlyList<HighlightRegionModel> columnRegions,
            IReadOnlyList<HighlightRegionModel> rowRegions,
            HighlightRegionModel? dataBlock,
            IReadOnlyList<HighlightRegionModel> phaseInfoRegions,
            int? missingValue,
            int? notAmbiguousValue,
            Color missingCellBorderColor,
            Color notAmbiguousCellBorderColor)
        {
            ColumnRegions = columnRegions;
            RowRegions = rowRegions;
            DataBlock = dataBlock;
            PhaseInfoRegions = phaseInfoRegions;
            MissingValue = missingValue;
            NotAmbiguousValue = notAmbiguousValue;
            MissingCellBorderColor = missingCellBorderColor;
            NotAmbiguousCellBorderColor = notAmbiguousCellBorderColor;
        }
    }
}
