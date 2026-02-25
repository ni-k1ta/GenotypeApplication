using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Application_configuration.Data_file.Detectors
{
    internal class GenotypeDataDetector : FormatDetectorBase
    {
        private const int _order = 220;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            
        }
    }
}
