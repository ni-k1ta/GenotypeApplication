using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Application_configuration.Data_file.Detectors.Completed
{
    public class OneRowPerIndDetector : FormatDetectorBase
    {
        private const int _order = 40;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var format = dataDetectionModel.Format;

            var (IsSuccess, Values) = PrepareData(dataDetectionModel);

            if (!IsSuccess) throw new Exception(); //TODO изменить Exception

            var (HasPattern, RepeatCount) = FormatDetectorsHelper.HasRepeatPattern(Values);

            if (HasPattern && RepeatCount > 1)
            {
                format.OneRowPerInd = false;
            }
        }
    }
}
