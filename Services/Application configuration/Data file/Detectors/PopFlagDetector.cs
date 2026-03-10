using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Data_file_scanners
{
    public class PopFlagDetector : FormatDetectorBase
    {
        private const int _order = 120;
        public override int Order => _order;

        private const double _minOnesRatio = 0.05;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;

            // PopFlag не может существовать без PopData
            if (!format.PopData)
            {
                format.PopFlag = false;
                return;
            }

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess)
            {
                format.PopFlag = false;
                return;
            }

            format.PopFlag = IsPopFlag(Values, dataDetectionModel);
        }

        private bool IsPopFlag(string[] column, DataDetectionModel dataDetectionModel)
        {
            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            int columnLength = column.Length;
            if (columnLength == 0) return false;

            var validIntegers = new List<int>(columnLength);

            foreach (var columnValue in column)
            {
                if (int.TryParse(columnValue, out int intValue))
                {
                    if (intValue != 0 && intValue != 1) return false;
                    validIntegers.Add(intValue);
                }
                else return false;
            }

            if (validIntegers.Count == 0) return false;

            int scanned = validIntegers.Count;
            int onesCount = validIntegers.Count(v => v == 1);
            int zerosCount = scanned - onesCount;
            double onesRatio = (double)onesCount / scanned;

            if (onesRatio < _minOnesRatio) return false;

            int popDataColumnIndex = format.Label ? 1 : 0;
            int popDataStartRowIndex = (format.MarkerNames ? 1 : 0) + (format.RecessiveAlleles ? 1 : 0) + (format.MapDistances ? 1 : 0);

            if (popDataStartRowIndex > data.RowsCount) return false;

            var popDataColumn = data.GetColumn(popDataColumnIndex, popDataStartRowIndex);
            var popDataColumnValues = popDataColumn.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            var popDataValues = new List<int>(popDataColumnValues.Length);
            foreach (var v in popDataColumnValues)
            {
                if (int.TryParse(v, out var n)) popDataValues.Add(n);
            }

            if (format.OneRowPerInd == false && format.Ploidy >= 2)
            {
                if (!HasSamePatternAsPopData(validIntegers, popDataValues, format)) return false;
            }

            return true;
        }

        private bool HasSamePatternAsPopData(List<int> values, List<int> compareValues, DataFileFormatModel format)
        {
            if (values.Count < 2 || compareValues.Count < 2) return false;

            int currentSize = 1;

            for (int i = 1; i < values.Count; i++)
            {
                if ((currentSize < format.Ploidy))
                {
                    if ((compareValues[i] == compareValues[i - 1]) && (values[i] == values[i - 1])) currentSize++;
                    else return false;
                }
                else currentSize = 1;
            }

            return true;
        }
    }
}
