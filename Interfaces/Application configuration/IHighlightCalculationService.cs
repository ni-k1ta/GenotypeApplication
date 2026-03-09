using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file.Highlights;
using System.Data;

namespace GenotypeApplication.Interfaces.Application_configuration
{
    public interface IHighlightCalculationService
    {
        HighlightMapModel Calculate(DataFileFormatModel parameters, DataTable? data, CancellationToken ct = default);
    }
}
