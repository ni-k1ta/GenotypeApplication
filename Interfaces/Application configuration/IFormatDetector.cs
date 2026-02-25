using GenotypeApplication.Models.Structure.Data_file;

namespace GenotypeApplication.Interfaces
{
    public interface IFormatDetector
    {
        int Order { get; }
        void Detect(DataDetectionModel dataDetectionModel);
    }
}
