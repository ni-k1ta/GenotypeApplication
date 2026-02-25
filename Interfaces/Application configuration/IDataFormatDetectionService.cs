using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;

namespace GenotypeApplication.Interfaces.Application_configuration
{
    public interface IDataFormatDetectionService
    {
        DataFileFormatModel StartParameterDetection(DataTableModel data);
    }
}
