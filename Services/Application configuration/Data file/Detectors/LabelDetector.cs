using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;
using System.Globalization;

namespace GenotypeApplication.Services.Data_file_scanners
{
    public class LabelDetector : FormatDetectorBase
    {
        private const int _order = 100;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel); //исключения типа ArgumentNullException являются скорее отладочными, а не для пользователя, т.к. при конечной реализованной логике приложения не могут настать, поэтому их не нужно обабатывать и уведомлять о них пользователя, они только для тестов, либо для тех кто будет изменять код приложения => todo везде изменить аругменты на nameof(...) и не добавлять их отлавливание в catch().

            var format = dataDetectionModel.Format;

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess)
            {
                format.Label = false;
                return;
            }

            format.Label = IsLabel(Values, format);
        }

        private bool IsLabel(string[] column, DataFileFormatModel format)
        {
            int columnLength = column.Length;

            if (columnLength == 0) return false;

            int nonNumericCount = 0;
            var intValues = new List<int>();

            foreach (var columnValue in column)
            {
                if (!double.TryParse(columnValue, CultureInfo.InvariantCulture, out var d)) nonNumericCount++;
                else if (d % 1 == 0) intValues.Add((int)d);
            }

            // Критерий 1: Высокая доля нечисловых значений → это Label
            double nonNumericRatio = (double)nonNumericCount / columnLength;
            if (nonNumericRatio >= 0.9) return true;

            if (format.OneRowPerInd == false && format.Ploidy >= 2)
            {
                // Критерий 2: Последовательные ID(1, 2, 3, ...)
                var uniqueSorted = intValues.Distinct().OrderBy(x => x).ToList();
                if (uniqueSorted.Count == columnLength / format.Ploidy)
                    if (IsContinuousSequence(uniqueSorted)) return true;
            }

            // Критерий 4: Все значения уникальны
            if (intValues.Distinct().Count() == (columnLength / ((format.Ploidy != 0 && format.OneRowPerInd == false) ? format.Ploidy : 1))) return true;

            return false;
        }
        private bool IsContinuousSequence(List<int> sortedValues)
        {
            for (int i = 1; i < sortedValues.Count; i++)
                if (sortedValues[i] - sortedValues[i - 1] != 1) return false;

            return true;
        }
    }
}
