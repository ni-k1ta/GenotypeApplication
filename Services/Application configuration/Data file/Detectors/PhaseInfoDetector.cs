using GenotypeApplication.Models.Structure.Data_file;
using System.Globalization;

namespace GenotypeApplication.Services.Application_configuration.Data_file_scanners
{
    internal class PhaseInfoDetector : FormatDetectorBase
    {
        private const int _order = 10;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            if (data.IsEmpty) return;

            var lastRowValues = data.GetRow(data.RowsCount - 1).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (lastRowValues.Length < data.ColumnCount)
            {
                format.PHASEINFO = true;
                //УБРАТЬ ОТСЮДА И ДОБАВИТЬ В ОПРЕДЕЛЕНИЕ
                //format.Ploidy = 2; //phaseinfo только для диплоидных данных (оф. документация)
                //format.NumLoci = lastRowValues.Length; //количество значений в дополнительных строках всегда = количество локусов
                return;
            }

            foreach (var value in lastRowValues)
            {
                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                {
                    format.PHASEINFO = false;
                    return;
                }

                if (number < 0f || number > 1f)
                {
                    format.PHASEINFO = false;
                    return;
                }
            }

            format.PHASEINFO = true;
            //УБРАТЬ ОТСЮДА И ДОБАВИТЬ В ОПРЕДЕЛЕНИЕ
            //format.Ploidy = 2; //phaseinfo только для диплоидных данных (оф. документация) УБРАТЬ ОТСЮДА И ДОБАВИТЬ В ОПРЕДЕЛЕНИЕ
            //format.NumLoci = lastRowValues.Length; //количество значений в дополнительных строках всегда = количество локусов УБРАТЬ ОТСЮДА И ДОБАВИТЬ В ОПРЕДЕЛЕНИЕ
        }
    }
}
