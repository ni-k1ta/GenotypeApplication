using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;

namespace GenotypeApplication.Interfaces.Application_configuration
{
    public interface IDataFormatDetectionService
    {
        DataFileFormatModel StartFormatDetection(DataTableModel data);
        bool IsFormatMatchesWithData(DataTableModel? data, DataFileFormatModel format);
    }
}
