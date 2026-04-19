using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file.Highlights;
using System.Data;
using System.Windows.Media;

namespace GenotypeApplication.Services.Application_configuration.Data_file
{
    public class HighlightCalculationService
    {
        private static readonly Color LabelColor = Colors.LimeGreen;
        private static readonly Color PopDataColor = Colors.DodgerBlue;
        private static readonly Color PopFlagColor = Colors.MediumPurple;
        private static readonly Color LocDataColor = Colors.Orange;
        private static readonly Color PhenotypeColor = Colors.Crimson;
        private static readonly Color ExtraColsColor = Colors.Teal;

        private static readonly Color MarkerNamesColor = Colors.Red;
        private static readonly Color RecessiveAllelesColor = Colors.DarkOrange;
        private static readonly Color MapDistancesColor = Colors.Purple;

        private static readonly Color DataBlockColor = Colors.Gray;

        private static readonly Color PhaseInfoColor = Colors.DeepPink;
        private static readonly Color MissingCellColor = Colors.Yellow;
        private static readonly Color NotAmbiguousCellColor = Colors.Cyan;

        public HighlightMapModel Calculate(DataFileFormatModel parameters, DataTable? data, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            int metadataCols = CalculateMetadataColCount(parameters);
            int headerRows = CalculateHeaderRowCount(parameters);

            int dataRows = CalculateDataRows(parameters);
            int genotypeCols = CalculateGenotypeCols(parameters);

            //ct.ThrowIfCancellationRequested();

            var columns = CalculateColumns(parameters, headerRows, dataRows);
            var rows = CalculateRows(parameters, data);

            HighlightRegionModel? dataBlock = null;
            if (dataRows > 0 && genotypeCols > 0)
            {
                dataBlock = new HighlightRegionModel(   
                    startRow: headerRows,
                    endRow: headerRows + dataRows - 1,
                    startCol: metadataCols,
                    endCol: metadataCols + genotypeCols - 1,
                    "DataBlock", DataBlockColor);
            }

            var phaseRows = CalculatePhaseInfoRows(parameters, data, headerRows);

            int? missingVal = parameters.Missing;
            int? notAmbVal = parameters.NOTAMBIGUOUS ? parameters.NotAmbiguousValue : null;

            return new HighlightMapModel(
                 columnRegions: columns,
                 rowRegions: rows,
                 dataBlock: dataBlock,
                 phaseInfoRegions: phaseRows,
                 missingValue: missingVal,
                 notAmbiguousValue: notAmbVal,
                 missingCellBorderColor: MissingCellColor,
                 notAmbiguousCellBorderColor: NotAmbiguousCellColor);
        }

        private static int FindRowStartCol(DataTable? data, int rowIndex)
        {
            if (data == null) return 0;
            if (rowIndex < 0 || rowIndex >= data.Rows.Count) return 0;

            var row = data.Rows[rowIndex];

            for (int col = 0; col < data.Columns.Count; col++)
            {
                var value = row[col]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(value))
                    return col;
            }

            return 0;
        }
        private static int FindRowEndCol(DataTable? data, int rowIndex)
        {
            if (data == null) return 0;
            if (rowIndex < 0 || rowIndex >= data.Rows.Count) return 0;

            var row = data.Rows[rowIndex];

            for (int col = data.Columns.Count - 1; col >= 0; col--)
            {
                var value = row[col]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(value))
                    return col;
            }

            return 0;
        }

        private List<HighlightRegionModel> CalculateColumns(DataFileFormatModel p, int headerRows, int totalDataRows)
        {
            var result = new List<HighlightRegionModel>();
            int cursor = 0;

            int lastRow = headerRows + totalDataRows - 1;

            if (p.Label)
            {
                result.Add(new HighlightRegionModel(headerRows, lastRow, cursor, cursor, "Label", LabelColor));
                cursor++;
            }
            if (p.PopData)
            {
                result.Add(new HighlightRegionModel(headerRows, lastRow, cursor, cursor, "PopData", PopDataColor));
                cursor++;
            }
            if (p.PopFlag)
            {
                result.Add(new HighlightRegionModel(headerRows, lastRow, cursor, cursor, "PopFlag", PopFlagColor));
                cursor++;
            }
            if (p.LocData)
            {
                result.Add(new HighlightRegionModel(headerRows, lastRow, cursor, cursor, "LocData", LocDataColor));
                cursor++;
            }
            if (p.Phenotype)
            {
                result.Add(new HighlightRegionModel(headerRows, lastRow, cursor, cursor, "Phenotype", PhenotypeColor));
                cursor++;
            }
            if (p.ExtraCols > 0)
            {
                int endIndex = cursor + p.ExtraCols - 1;
                result.Add(new HighlightRegionModel(headerRows, lastRow, cursor, endIndex, "ExtraCols", ExtraColsColor));
            }

            return result;
        }
        private List<HighlightRegionModel> CalculateRows(DataFileFormatModel p, DataTable? d)
        {
            var result = new List<HighlightRegionModel>();
            int cursor = 0;

            if (p.MarkerNames)
            {
                int startCol = FindRowStartCol(d, cursor);
                int endCol = FindRowEndCol(d, cursor);

                result.Add(new HighlightRegionModel(cursor, cursor, startCol, endCol, "MarkerNames", MarkerNamesColor));
                cursor++;
            }
            if (p.RecessiveAlleles)
            {
                int startCol = FindRowStartCol(d, cursor);
                int endCol = FindRowEndCol(d, cursor);

                result.Add(new HighlightRegionModel(cursor, cursor, startCol, endCol, "RecessiveAlleles", RecessiveAllelesColor));
                cursor++;
            }
            if (p.MapDistances)
            {
                int startCol = FindRowStartCol(d, cursor);
                int endCol = FindRowEndCol(d, cursor);

                result.Add(new HighlightRegionModel(cursor, cursor, startCol, endCol, "MapDistances", MapDistancesColor));
                cursor++;
            }

            return result;
        }
        private int CalculateMetadataColCount(DataFileFormatModel p)
        {
            return (p.Label ? 1 : 0)
                 + (p.PopData ? 1 : 0)
                 + (p.PopFlag ? 1 : 0)
                 + (p.LocData ? 1 : 0)
                 + (p.Phenotype ? 1 : 0)
                 + (p.ExtraCols > 0 ? p.ExtraCols : 0);
        }
        private int CalculateHeaderRowCount(DataFileFormatModel p)
        {
            return (p.MarkerNames ? 1 : 0)
                 + (p.RecessiveAlleles ? 1 : 0)
                 + (p.MapDistances ? 1 : 0);
        }
        private int CalculateDataRows(DataFileFormatModel p)
        {
            if (p.OneRowPerInd)
                return p.NumInds * (p.PHASEINFO ? 2 : 1);
            else
                return p.NumInds * p.Ploidy + (p.PHASEINFO ? p.NumInds : 0);
        }

        private int CalculateGenotypeCols(DataFileFormatModel p)
        {
            return p.OneRowPerInd
                ? p.Ploidy * p.NumLoci
                : p.NumLoci;
        }

        private List<HighlightRegionModel> CalculatePhaseInfoRows(DataFileFormatModel p, DataTable? d, int headerRows)
        {
            var result = new List<HighlightRegionModel>();
            if (!p.PHASEINFO) return result;

            //int lastCol = metadataCols - 1;

            if (p.OneRowPerInd)
            {
                for (int i = 0; i < p.NumInds; i++)
                {
                    int rowIndex = headerRows + i * 2 + 1;

                    int startCol = FindRowStartCol(d, rowIndex);
                    int endCol = FindRowEndCol(d, rowIndex);

                    result.Add(new HighlightRegionModel(rowIndex, rowIndex, startCol, endCol, "PhaseInfo", PhaseInfoColor));
                }
            }
            else
            {
                int blockSize = p.Ploidy + 1;
                for (int i = 0; i < p.NumInds; i++)
                {
                    int rowIndex = headerRows + i * blockSize + p.Ploidy;

                    int startCol = FindRowStartCol(d, rowIndex);
                    int endCol = FindRowEndCol(d, rowIndex);

                    result.Add(new HighlightRegionModel(rowIndex, rowIndex, startCol, endCol, "PhaseInfo", PhaseInfoColor));
                }
            }

            return result;
        }
    }
}
