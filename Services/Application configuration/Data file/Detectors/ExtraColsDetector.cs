using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;
using System.Globalization;
using System.Windows.Forms;

namespace GenotypeApplication.Services.Data_file_scanners
{
    public class ExtraColsDetector : FormatDetectorBase
    {
        private const int _order = 160;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;
            var data = dataDetectionModel.Data;

            if (data.IsEmpty)
            {
                format.ExtraCols = 0;
                return;
            }

            int firstDataRow = FormatDetectorsHelper.GetFirstDataRow(format);
            int firstCandidateCol = FormatDetectorsHelper.GetCurrentDataColumn(format);
            int totalColumns = data.ColumnCount;

            if (firstCandidateCol >= totalColumns)
            {
                format.ExtraCols = 0;
                return;
            }

            if (format.OneRowPerInd == false && (format.MarkerNames || format.RecessiveAlleles || format.MapDistances))
            {
                int mainDataColumnCount = data.GetRow(0).Where(s => !string.IsNullOrWhiteSpace(s)).Count();
                int extraColsCount = totalColumns - mainDataColumnCount - firstCandidateCol;

                format.ExtraCols = Math.Max(0, extraColsCount);
                return;
            }

            int detectedExtraCols = IsExtraCols(firstDataRow, firstCandidateCol, totalColumns, dataDetectionModel);
            format.ExtraCols = Math.Max(0, detectedExtraCols);
        }

        private int IsExtraCols(int firstDataRow, int firstCandidateCol, int totalColumns, DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;
            var data = dataDetectionModel.Data;

            if (data.IsEmpty) return 0;

            int extraColsCount = 0;

            for (int col = firstCandidateCol; col < totalColumns; col++)
            {
                var column = data.GetColumn(col, firstDataRow);
                var columnValues = column.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                if (columnValues.Length == 0) break;

                if (ContainsNonNumericValues(columnValues) || ContainsFloatValues(columnValues))
                {
                    extraColsCount++;
                    continue;
                }

                if (format.OneRowPerInd == false && format.Ploidy >= 2)
                {
                    if (IsColumnDuplicatedPerIndividual(data, col, firstDataRow, format.Ploidy))
                    {
                        extraColsCount++;
                        continue;
                    }
                    break;
                }

                if (!ContainsMissingValue(columnValues, format.Missing))
                {
                    bool subsequentHaveMissing = AnySubsequentColumnHasMissing(data, col + 1, totalColumns, firstDataRow, format.Missing);

                    if (subsequentHaveMissing)
                    {
                        extraColsCount++;
                        continue;
                    }
                }

                break;
            }

            return extraColsCount;
        }

        private bool AnySubsequentColumnHasMissing(DataTableModel data, int startCol, int endCol, int firstDataRow, int missingValue)
        {
            int columnsToCheck = Math.Min(10, endCol - startCol);
            if (columnsToCheck <= 0) return false;

            int step = Math.Max(1, (endCol - startCol) / columnsToCheck);

            for (int col = startCol; col < endCol; col += step)
            {
                var values = data.GetColumn(col, firstDataRow);
                if (ContainsMissingValue(values, missingValue)) return true;
            }

            return false;
        }

        private bool ContainsMissingValue(string[] values, int missingValue)
        {
            return values.Any(v => v.Trim() == missingValue.ToString());
        }
        private bool ContainsNonNumericValues(string[] values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value)) continue;

                var normalized = value.Replace(',', '.');

                if (!double.TryParse(normalized, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    return true;
            }

            return false;
        }
        private bool ContainsFloatValues(string[] values)
        {
            var floatValues = new List<string>();
            var intValues = new List<int>();

            foreach (var s in values)
            {
                var normalized = s.Replace(',', '.');
                if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                {
                    if (v % 1 != 0) floatValues.Add(normalized);
                    else if (normalized.Contains('.')) return true;
                }

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    intValues.Add(i);
            }

            if (floatValues.Count/values.Length > 0.5) return true;

            if (intValues.Count/values.Length > 0.65 && floatValues.Count/values.Length < 0.35) return false;

            return true;
        }

        private bool IsColumnDuplicatedPerIndividual(DataTableModel data, int columnIndex, int firstDataRow, int ploidy)
        {
            int maxRows = data.RowsCount - firstDataRow;

            int individualsToCheck = maxRows / ploidy;

            if (individualsToCheck == 0) return false;

            int duplicatedCount = 0;
            int totalChecked = 0;

            for (int ind = 0; ind < individualsToCheck; ind++)
            {
                int baseRow = firstDataRow + ind * ploidy;

                var values = new string[ploidy];
                bool hasData = true;

                for (int p = 0; p < ploidy; p++)
                {
                    int rowIdx = baseRow + p;
                    if (rowIdx >= data.RowsCount)
                    {
                        hasData = false;
                        break;
                    }

                    var rowData = data.GetCellValue(columnIndex, rowIdx);
                    if (rowData.Length == 0)
                    {
                        hasData = false;
                        break;
                    }

                    values[p] = rowData;
                }

                if (!hasData) continue;

                totalChecked++;

                bool allSame = values.All(v => v == values[0]);
                if (allSame) duplicatedCount++;
            }

            if (totalChecked == 0) return false;

            double ratio = (double)duplicatedCount / totalChecked;
            return ratio >= 0.9;
        }
    }
}
