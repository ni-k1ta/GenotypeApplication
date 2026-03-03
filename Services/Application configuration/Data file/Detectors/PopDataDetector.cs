using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;
using System.Globalization;

namespace GenotypeApplication.Services.Data_file_scanners
{
    public class PopDataDetector : FormatDetectorBase
    {
        private const int _order = 110;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess)
            {
                format.PopData = false;
                return;
            }

            format.PopData = IsPopData(Values, dataDetectionModel);
        }

        private bool IsPopData(string[] column, DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            if (column.Length == 0) return false;

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            var uniqueInts = new SortedSet<int>();
            var groupCounts = new Dictionary<int, int>();

            // Все значения должны быть целыми положительными числами
            foreach (var columnValue in column)
            {
                if (!int.TryParse(columnValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                    return false;

                if (parsed < 0)
                    return false;

                uniqueInts.Add(parsed);
                groupCounts.TryGetValue(parsed, out int count);
                groupCounts[parsed] = count + 1;
            }

            // Количество уникальных значений должно быть значительно меньше числа строк
            // Если уникальных значений столько же, сколько строк — это идентификатор, не группировка
            // Порог: уникальных значений не более 50% от общего количества строк
            // (в реальных данных популяций обычно 2-20 при сотнях индивидов)
            double uniqueRatio = (double)uniqueInts.Count / column.Length;
            if (uniqueRatio > 0.5)
                return false;

            int popFlagColumnIndex = (format.Label ? 1 : 0) + 1;
            int popFlagStartRowIndex = (format.MarkerNames ? 1 : 0) + (format.RecessiveAlleles ? 1 : 0) + (format.MapDistances ? 1 : 0);

            if (popFlagColumnIndex > data.ColumnCount || popFlagStartRowIndex > data.RowsCount) return false;

            var popFlagColumn = data.GetColumn(popFlagColumnIndex, popFlagStartRowIndex);
            var popFlagColumnValues = popFlagColumn.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            bool hasZero = false;
            bool hasOne = false;
            bool hasAnother = false;

            foreach (var v in popFlagColumnValues)
            {
                if (v == "0")
                    hasZero = true;
                else if (v == "1")
                    hasOne = true;
                else
                {
                    hasAnother = true;
                    break;
                }
            }

            // Оба значения должны присутствовать
            if ((hasZero && hasOne) && !hasAnother)
                return true;

            if (uniqueInts.Count == 0)
                return false;

            int min = uniqueInts.Min;
            int max = uniqueInts.Max;
            int expectedRange = max - min + 1;

            double continuityRatio = (double)uniqueInts.Count / expectedRange;
            if (continuityRatio < 0.9) return false;

            var singletonCount = groupCounts.Count(kvp => kvp.Value == 1);
            double singletonRatio = (double)singletonCount / uniqueInts.Count;
            if (singletonRatio > 0.3) return false;

            double maxValueRatio = (double)max / (column.Length / ((format.Ploidy != 0 && format.OneRowPerInd == false) ? format.Ploidy : 1));
            if (maxValueRatio >= 0.25) return false;

            if (groupCounts.Count < 2)
                return false;

            int minSize = groupCounts.Values.Min();
            int maxSize = groupCounts.Values.Max();

            if (maxSize == 0)
                return false;

            double ratio = (double)minSize / maxSize;
            if (ratio < 0.05)
                return false;

            return true;
        }
    }
}
