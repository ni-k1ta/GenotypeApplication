using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Data_file_scanners
{
    public class LocDataDetector : FormatDetectorBase
    {
        private const int _order = 130;
        public override int Order => _order;

        private const double _minParseSuccessRatio = 0.9;

        private const double _maxUniqueRatio = 0.45;

        private const double _minSamplesPerLocation = 2.0;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess)
            {
                format.LocData = false;
                return;
            }

            format.LocData = IsLocData(Values, format);
        }

        private bool IsLocData(string[] column, DataFileFormatModel format)
        {
            if (column.Length == 0) return false;

            var validIntegers = new List<int>(column.Length);
            var frequency = new Dictionary<int, int>();
            int parseFailures = 0;

            foreach (var value in column)
            {
                if (int.TryParse(value, out int intValue))
                {
                    validIntegers.Add(intValue);

                    if (!frequency.ContainsKey(intValue))
                        frequency[intValue] = 0;

                    frequency[intValue]++;
                }
                else
                {
                    parseFailures++;
                }
            }

            int scanned = validIntegers.Count;

            double parseSuccessRatio = scanned / (double)column.Length;
            if (parseSuccessRatio < _minParseSuccessRatio) return false;

            if (scanned == 0 || frequency.Count == 0) return false;

            int minValue = validIntegers.Min();
            int maxValue = validIntegers.Max();
            int uniqueCount = frequency.Count;

            if (minValue < 0) return false;

            double uniqueRatio = (double)uniqueCount / scanned;
            if (uniqueRatio > _maxUniqueRatio) return false;

            double samplesPerLocation = (double)scanned / uniqueCount;
            if (samplesPerLocation < _minSamplesPerLocation) return false;

            var sortedUnique = frequency.Keys.OrderBy(k => k).ToList();
            int expectedRange = sortedUnique.Last() - sortedUnique.First() + 1;
            double continuityRatio = (double)uniqueCount / expectedRange;
            if (continuityRatio < 0.7) return false;

            int singletonCount = frequency.Count(kvp => kvp.Value == 1);
            double singletonRatio = (double)singletonCount / uniqueCount;
            if (singletonRatio > 0.2) return false;


            return true;
        }
    }
}
