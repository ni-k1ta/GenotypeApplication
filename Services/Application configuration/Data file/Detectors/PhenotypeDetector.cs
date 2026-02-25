using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Data_file_scanners
{
    internal class PhenotypeDetector : FormatDetectorBase
    {
        private const int _order = 140;
        public override int Order => _order;

        /// <summary>
        /// Минимальная доля успешно распарсенных значений.
        /// </summary>
        private const double _minParseSuccessRatio = 0.9;

        /// <summary>
        /// Максимальное соотношение уникальных к общему числу.
        /// Если слишком много уникальных — скорее генотипические данные.
        /// </summary>
        private const double _maxUniqueRatio = 0.5;

        /// <summary>
        /// Максимально допустимое значение Phenotype.
        /// Фенотипические категории — обычно небольшие числа.
        /// </summary>
        private const int _maxPhenotypeValue = 100;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel); // ИСПРАВЛЕН БАГ

            var format = dataDetectionModel.Format;

            //// Phenotype идёт после Label + PopData + PopFlag + LocData
            //int columnIndex = (format.Label ? 1 : 0)
            //                + (format.PopData ? 1 : 0)
            //                + (format.PopFlag ? 1 : 0)
            //                + (format.LocData ? 1 : 0);

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess)
            {
                format.Phenotype = false;
                return;
            }

            format.Phenotype = IsPhenotype(Values);
        }

        private bool IsPhenotype(string[] column)
        {
            if (column.Length == 0) return false;

            // Один проход: парсинг, исключение missing, сбор статистик
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

            // Проверка 1: Достаточно ли значений распарсились
            double parseSuccessRatio = (double)allParsed.Count / column.Length;
            if (parseSuccessRatio < _minParseSuccessRatio) return false;

            int scanned = allParsed.Count;
            int uniqueCount = allParsed.Distinct().Count();
            int minValue = allParsed.Min();
            int maxValue = allParsed.Max();
            double uniqueRatio = (double)uniqueCount / scanned;

            // Проверка 2: Проверка диапазона значений
            // Фенотипические категории — неотрицательные (кроме missing)
            // и не слишком большие (не микросателлитные аллели)
            if (minValue < 0) return false;
            if (maxValue > _maxPhenotypeValue) return false;

            // Проверка 3: Уникальность — не слишком высокая и не слишком низкая
            if (uniqueRatio > _maxUniqueRatio) return false;
            if (uniqueCount <= 1) return false; // все значения одинаковы — бессмысленно

            // Проверка 4: Two-row формат — попарная валидация
            // Phenotype одинаков для обеих строк одного индивида
            //if (!dataDetectionModel.Format.OneRowPerInd)
            //{
            //    if (!ValidatePairsForTwoRowFormat(allParsed)) return false;
            //}

            return true;
        }

        /// <summary>
        /// Проверяет попарное совпадение значений для two-row формата.
        /// В two-row формате каждый индивид занимает 2 строки,
        /// значение Phenotype повторяется в обеих строках.
        /// </summary>
        //private bool ValidatePairsForTwoRowFormat(List<int> values)
        //{
        //    if (values.Count < 2) return false;
        //    if (values.Count % 2 != 0) return false;

        //    int totalPairs = values.Count / 2;
        //    int matchedPairs = 0;

        //    for (int i = 0; i < values.Count; i += 2)
        //    {
        //        if (values[i] == values[i + 1])
        //            matchedPairs++;
        //    }

        //    double pairMatchRatio = (double)matchedPairs / totalPairs;
        //    return pairMatchRatio >= 0.9;
        //}
    }
}
