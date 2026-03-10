using GenotypeApplication.Models.Structure.Data_file;
using System.Globalization;

namespace GenotypeApplication.Services.Application_configuration.Data_file_scanners
{
    public class AdditionalRowsDetector : FormatDetectorBase
    {
        private const int _order = 20;
        public override int Order => _order;

        private enum HeaderRowType
        {
            MarkerNames,
            RecessiveAlleles,
            MapDistances,
            Unknown
        }

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            if (data.IsEmpty || data.RowsCount < 3) return;

            int foundHeadersCount = 0;

            for (int i = 0; i < 3 && i < data.RowsCount; i++)
            {
                var rowValues = GetNonEmptyRowValues(data, i);

                if (rowValues.Length == data.ColumnCount) break;
                else foundHeadersCount++;
            }

            if (foundHeadersCount > 0)
            {
                DetectLengthDifferentRows(dataDetectionModel, foundHeadersCount); //в данных есть мета-столбцы => нужно просто классифицировать дополнительные строки по признакам
            }
            else
            {
                if (format.PHASEINFO) return; //был найден PhaseInfo => в данных есть мета-столбцы => нет дополнительных строк, которые должны быть короче основных строк с данными, и NumLoci уже был определён в PhaseInfoDetector

                DetectLengthSameRows(dataDetectionModel);
            }
        }

        private void DetectLengthDifferentRows(DataDetectionModel dataDetectionModel, int foundHeadersCount)
        {
            var format = dataDetectionModel.Format;

            switch (foundHeadersCount)
            {
                case 1:
                    ClassifySingleHeader(dataDetectionModel);
                    break;
                case 2:
                    ClassifyTwoHeaders(dataDetectionModel);
                    break;
                case 3:
                    format.MarkerNames = true;
                    format.RecessiveAlleles = true;
                    format.MapDistances = true;
                    break;
                default:
                    break;
            }
        }
        private void DetectLengthSameRows(DataDetectionModel dataDetectionModel)
        {
            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            int mapDistanceRow = -1;

            for (int i = 2; i >= 0; i--)
            {
                var rowValues = GetNonEmptyRowValues(data, i);

                if (IsMapDistances(rowValues))
                {
                    mapDistanceRow = i;
                    break;
                }
            }

            switch (mapDistanceRow)
            {
                case 2:
                    format.MarkerNames = true;
                    format.RecessiveAlleles = true;
                    format.MapDistances = true;
                    return;
                case 1:
                    format.MapDistances = true;

                    var rowValues = GetNonEmptyRowValues(data, 0);

                    if (IsMarkerNames(rowValues)) format.MarkerNames = true;
                    else format.RecessiveAlleles = true;

                    return;
                case 0:
                    format.MapDistances = true;
                    return;
                default:
                    break;
            }

            int recessiveAllelesRow = -1;

            for (int i = 1; i >= 0; i--)
            {
                var rowValues = GetNonEmptyRowValues(data, i);
                if (IsRecessiveAlleles(dataDetectionModel, rowValues))
                {
                    recessiveAllelesRow = i;
                    break;
                }
            }

            switch (recessiveAllelesRow)
            {
                case 1:
                    format.RecessiveAlleles = true;
                    format.MarkerNames = true;
                    return;
                case 0:
                    format.RecessiveAlleles = true;
                    return;
                default:
                    break;
            }

            var firstRow = GetNonEmptyRowValues(data, 0);
            if (IsMarkerNames(firstRow)) format.MarkerNames = true;
        }

        private bool IsRecessiveAlleles(DataDetectionModel dataDetectionModel, string[] row)
        {
            if (row.Length == 0) return false;

            if (!row.All(IsInteger)) return false;

            var uniqueValues = new HashSet<string>();

            foreach (var rowValue in row)
            {
                uniqueValues.Add(rowValue);
            }

            if ((double)uniqueValues.Count/row.Length >= 0.44) return false;

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            int mid = row.Length / 2;
            var halfRow = row.Skip(mid).ToArray();

            var rowValues = new HashSet<string>(halfRow);

            int halfHeight = data.RowsCount / 2;
            int startRow = data.RowsCount - halfHeight;

            var startColumn = data.ColumnCount - mid;

            var tableValues = new HashSet<string>();
            for (int col = startColumn; col < data.ColumnCount; col++)
            {
                var columnData = data.GetColumn(col, startRow);
                foreach (var val in columnData)
                {
                    var normalized = val.Replace(',', '.');
                    if (!double.TryParse(normalized, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
                    {
                        tableValues.Clear();
                        break;
                    }

                    tableValues.Add(val);
                }
            }

            int matches = rowValues.Count(v => tableValues.Contains(v));

            if ((double)matches/rowValues.Count < 0.7) return false;

            return true;
        }
        private void ClassifySingleHeader(DataDetectionModel dataDetectionModel)
        {
            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            var rowValues = GetNonEmptyRowValues(data, 0);

            if (rowValues.Length == 0) return;

            var rowType = IdentifyRowType(rowValues);

            switch (rowType)
            {
                case HeaderRowType.MarkerNames:
                    format.MarkerNames = true;
                    break;
                case HeaderRowType.RecessiveAlleles:
                    format.RecessiveAlleles = true;
                    break;
                case HeaderRowType.MapDistances:
                    format.MapDistances = true;
                    break;
                default:
                    break;
            }
        }
        private HeaderRowType IdentifyRowType(string[] row)
        {
            if (row.Length == 0) return HeaderRowType.Unknown;

            if (IsMapDistances(row)) return HeaderRowType.MapDistances;

            if (IsMarkerNames(row)) return HeaderRowType.MarkerNames;

            return HeaderRowType.RecessiveAlleles;
        }
        private void ClassifyTwoHeaders(DataDetectionModel dataDetectionModel)
        {
            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            var firstRowValues = GetNonEmptyRowValues(data, 0);

            if (firstRowValues.Length == 0) return;

            var secondRowValues = GetNonEmptyRowValues(data, 1);

            if (secondRowValues.Length == 0) return;

            if (!IsMapDistances(secondRowValues))
            {
                format.MarkerNames = true;
                format.RecessiveAlleles = true;
                return;
            }

            format.MapDistances = true;

            if (IsMarkerNames(firstRowValues)) format.MarkerNames = true;
            else format.RecessiveAlleles = true;
        }

        private bool IsMapDistances(string[] row)
        {
            if (row.Length == 0) return false;

            if (!row.All(IsFloat)) return false;

            var firstValue = double.Parse(row[0], CultureInfo.InvariantCulture);
            if (Math.Abs(firstValue - (-1.0)) > 1e-6)
                return false;

            bool hasFloat = false;
            foreach (var v in row)
            {
                var fv = double.Parse(v, CultureInfo.InvariantCulture);
                if (fv < -1.0 - 1e-6)
                    return false;
                if (Math.Abs(fv - Math.Round(fv)) > 1e-6)
                    hasFloat = true;
            }

            return hasFloat || row.All(v =>
            {
                var fv = double.Parse(v, CultureInfo.InvariantCulture);
                return Math.Abs(fv - (-1.0)) < 1e-6 || Math.Abs(fv) < 1e-6;
            });
        }
        private bool IsMarkerNames(string[] row)
        {
            if (row.Length == 0) return false;

            bool hasNumbers = row.Any(s => IsFloat(s));
            bool hasNonNumbers = RowHasNonNumeric(row);
            if (hasNumbers && hasNonNumbers) return false;

            if (!hasNumbers) return true;

            double uniqueRatio = row.Length > 0 ? (double)row.Distinct().Count() / row.Length : 0.0;
            if (uniqueRatio == 1) return true;

            return false;
        }

        private string[] GetNonEmptyRowValues(DataTableModel data, int rowIndex)
        {
            if (rowIndex >= data.RowsCount) return Array.Empty<string>();

            return data.GetRow(rowIndex).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }
    }
}
