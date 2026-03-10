using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Data_file_scanners
{
    internal class PhenotypeDetector : FormatDetectorBase
    {
        private const int _order = 140;
        public override int Order => _order;

        private const double _minParseSuccessRatio = 0.9;

        private const double _maxUniqueRatio = 0.5;

        private const int _maxPhenotypeValue = 100;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess)
            {
                format.Phenotype = false;
                return;
            }

            format.Phenotype = IsPhenotype(Values/*, format*/);
        }

        private bool IsPhenotype(string[] column/*, DataFileFormatModel format*/)
        {
            if (column.Length == 0) return false;

            var allParsed = new List<int>(column.Length);
            int parseFailures = 0;

            foreach (var v in column)
            {
                if (int.TryParse(v, out int val))
                {
                    allParsed.Add(val);
                }
                else
                {
                    parseFailures++;
                }
            }

            double parseSuccessRatio = (double)allParsed.Count / column.Length;
            if (parseSuccessRatio < _minParseSuccessRatio) return false;

            int scanned = allParsed.Count;
            int uniqueCount = allParsed.Distinct().Count();
            int minValue = allParsed.Min();
            int maxValue = allParsed.Max();
            double uniqueRatio = (double)uniqueCount / scanned;

            if (minValue < 0) return false;
            if (maxValue > _maxPhenotypeValue) return false;

            if (uniqueRatio > _maxUniqueRatio) return false;
            if (uniqueRatio < 0.2) return false;
            if (uniqueCount <= 1) return false;

            return true;
        }
    }
}
