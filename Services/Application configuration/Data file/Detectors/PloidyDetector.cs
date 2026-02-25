using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Application_configuration.Data_file.Detectors.Completed
{
    public class PloidyDetector : FormatDetectorBase
    {
        private const int _order = 30;
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

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess) throw new Exception(); //TODO изменить Exception

            var (HasPattern, RepeatCount) = FormatDetectorsHelper.HasRepeatPattern(Values);

            if (HasPattern && RepeatCount > 1)
            {
                format.Ploidy = RepeatCount;
                return;
            }
        }
    }
}
