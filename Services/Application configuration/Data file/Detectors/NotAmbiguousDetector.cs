using GenotypeApplication.Models.Structure.Data_file;
using System.Globalization;

namespace GenotypeApplication.Services.Application_configuration.Data_file_scanners
{
    public class NotAmbiguousDetector : FormatDetectorBase
    {
        private const int _order = 210;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            if (!format.RecessiveAlleles)
            {
                format.NOTAMBIGUOUS = false;
                return;
            }

            var startRow = FormatDetectorsHelper.GetFirstDataRow(format);
            var startColumn = data.ColumnCount - ((data.ColumnCount - FormatDetectorsHelper.GetCurrentDataColumn(format)) / 2);

            var allValues = new List<int>();

            for (int col = startColumn; col < data.ColumnCount; col++)
            {
                var columnData = data.GetColumn(col, startRow);
                foreach (var value in columnData)
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num) && num != format.Missing)
                    {
                        allValues.Add(num);
                    }
                }
            }

            if (allValues.Count == 0)
            {
                format.NOTAMBIGUOUS = false;
                return;
            }

            var knownValues = new HashSet<int> { -999, -888, -99, -1, 99, 888, 999 };
            var candidates = new Dictionary<int, int>();

            foreach (var val in allValues)
            {
                bool isRepeatingDigits = IsRepeatingDigits(val);
                bool isKnownCandidate = knownValues.Contains(val);

                if (isRepeatingDigits || isKnownCandidate)
                {
                    if (candidates.ContainsKey(val))
                        candidates[val]++;
                    else
                        candidates[val] = 1;
                }
            }

            if (candidates.Count == 0)
            {
                format.NOTAMBIGUOUS = false;
                return;
            }

            var matchedFromList = candidates.Where(kv => knownValues.Contains(kv.Key) && kv.Value > 1).ToList();

            if (matchedFromList.Count > 0)
            {
                format.NOTAMBIGUOUS = true;    
                format.NotAmbiguousValue = PickBest(matchedFromList);
                return;
            }

            var matchedAny = candidates.Where(kv => knownValues.Contains(kv.Key)).ToList();

            if (matchedAny.Count > 0)
            {
                format.NOTAMBIGUOUS = true;    
                format.NotAmbiguousValue = PickBest(matchedAny);
                return;
            }

            format.NOTAMBIGUOUS = true;
            format.NotAmbiguousValue = PickBest(candidates.ToList());
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
