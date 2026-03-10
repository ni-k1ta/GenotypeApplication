using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;
using System.Globalization;

namespace GenotypeApplication.Services.Data_file_scanners
{
    public class MissingValueDetector : FormatDetectorBase
    {
        private const int _order = 150;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            var startRow = FormatDetectorsHelper.GetFirstDataRow(format);
            var startColumn = data.ColumnCount - ((data.ColumnCount - FormatDetectorsHelper.GetCurrentDataColumn(format)) / 2);

            var allValues = new List<int>();

            for (int col = startColumn; col < data.ColumnCount; col++)
            {
                var columnData = data.GetColumn(col, startRow);
                foreach (var value in columnData)
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                        allValues.Add(num);
                }
            }

            if (allValues.Count == 0)
            {
                format.Missing = -9;
                return;
            }

            var knownValues = new HashSet<int> { -999, -99, -9, -1, 9, 99, 999 };
            var candidates = new Dictionary<int, int>();

            foreach (var val in allValues)
            {
                bool isNegative = val < 0;
                bool isRepeatingDigits = IsRepeatingDigits(val);
                bool isKnownCandidate = knownValues.Contains(val);

                if (isNegative || isRepeatingDigits || isKnownCandidate)
                {
                    if (candidates.ContainsKey(val))
                        candidates[val]++;
                    else
                        candidates[val] = 1;
                }
            }

            if (candidates.Count == 0)
            {
                format.Missing = -9;
                return;
            }

            var matchedFromList = candidates.Where(kv => knownValues.Contains(kv.Key) && kv.Value > 1).ToList();

            if (matchedFromList.Count > 0)
            {
                format.Missing = PickBest(matchedFromList);
                return;
            }

            var matchedAny = candidates.Where(kv => knownValues.Contains(kv.Key)).ToList();

            if (matchedAny.Count > 0)
            {
                format.Missing = PickBest(matchedAny);
                return;
            }

            format.Missing = PickBest(candidates.ToList());
        }

        int PickBest(List<KeyValuePair<int, int>> items)
        {
            bool sameSign = items.All(x => x.Key > 0) || items.All(x => x.Key <= 0);

            if (sameSign)
            {
                int maxCount = items.Max(kv => kv.Value);
                var topItems = items.Where(kv => kv.Value == maxCount).Select(kv => kv.Key).ToList();

                if (topItems.Count == 1) return topItems[0];

                var negatives = topItems.Where(v => v < 0).ToList();
                var positives = topItems.Where(v => v >= 0).ToList();

                if (negatives.Count > 0) return negatives.Max();

                return positives.Min();
            }
            else
            {
                var negativesValues = items.Where(kv => kv.Key <= 0).ToList();
                int maxCount = negativesValues.Max(kv => kv.Value);
                var topItems = items.Where(kv => kv.Value == maxCount).Select(kv => kv.Key).ToList();

                if (topItems.Count == 1) return topItems[0];

                var negatives = topItems.Where(v => v < 0).ToList();
                return negatives.Max();
            }
        }

        bool IsRepeatingDigits(int value)
        {
            var s = Math.Abs(value).ToString();
            return s.Length > 1 && s.All(c => c == s[0]);
        }
    }
}
