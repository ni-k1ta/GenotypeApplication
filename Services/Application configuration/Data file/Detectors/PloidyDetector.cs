using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Application_configuration.Data_file.Detectors.Completed
{
    public class PloidyDetector : FormatDetectorBase
    {
        private const int _order = 40;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            //var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            if (format.Ploidy != 0) return;

            if (format.PHASEINFO)
            {
                format.Ploidy = 2; //phaseinfo только для диплоидных данных (оф. документация)
                return;
            }

            if (format.OneRowPerInd == false)
            {
                var (IsSuccess, Values) = PrepareData(dataDetectionModel);

                if (!IsSuccess) throw new Exception(); //TODO изменить Exception

                var (_, RepeatCount) = FormatDetectorsHelper.HasRepeatPattern(Values);

                format.Ploidy = RepeatCount; //если OneRowPerInd == false, значит паттерн повторений был найден => достаточно просто взять количество повторений
                return;
            }
        }
    }
}
