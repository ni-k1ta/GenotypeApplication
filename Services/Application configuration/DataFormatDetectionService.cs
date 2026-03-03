using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.Application_configuration;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file.Detectors;
using GenotypeApplication.Services.Application_configuration.Data_file.Detectors.Completed;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;
using GenotypeApplication.Services.Data_file_scanners;
using System.IO;

namespace GenotypeApplication.Services.Application_configuration
{
    public class DataFormatDetectionService : IDataFormatDetectionService
    {
        private readonly List<IFormatDetector> _detectors;

        public DataFormatDetectionService()
        {
            _detectors = new List<IFormatDetector>
            {
                new PhaseInfoDetector(), //10
                new AdditionalRowsDetector(), //20
                new OneRowPerIndDetector(), //30
                new PloidyDetector(), //40
                new LabelDetector(),  //100
                new PopDataDetector(), //110
                new PopFlagDetector(), //120
                new LocDataDetector(), //130
                new PhenotypeDetector(), //140
                new MissingValueDetector(), //150
                new ExtraColsDetector(), //160
                new NotAmbiguousDetector(), //210
                new GenotypeDataDetector(), //220
            };

            _detectors = _detectors.OrderBy(d => d.Order).ToList();
        }

        public DataFileFormatModel StartParameterDetection(DataTableModel data)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (data.IsEmpty) throw new InvalidDataException("Data table from data file can't be empty!");

            try
            {
                var dataDetectionModel = new DataDetectionModel(data);

                foreach (var detector in _detectors)
                {
                    detector.Detect(dataDetectionModel);
                }

                if (!IsDetectionResultValid(dataDetectionModel.Data, dataDetectionModel.Format)) throw new FormatException(); //количество параметров формата данных, определённых детекторами, <= минимального требуемого количества даже для минимальных возможных данных в файле (учитывая default значения)

                return dataDetectionModel.Format;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool IsDetectionResultValid(DataTableModel data, DataFileFormatModel format)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(format);

            // Ожидаемые строки
            int headerRows = (format.MarkerNames ? 1 : 0)
                           + (format.RecessiveAlleles ? 1 : 0)
                           + (format.MapDistances ? 1 : 0);

            int dataRows;
            if (format.OneRowPerInd == true)
                dataRows = format.NumInds * (format.PHASEINFO ? 2 : 1);
            else
                dataRows = format.NumInds * format.Ploidy + (format.PHASEINFO ? format.NumInds : 0);

            int expectedRows = headerRows + dataRows;

            // Ожидаемые столбцы
            int metadataCols = (format.Label ? 1 : 0)
                             + (format.PopData ? 1 : 0)
                             + (format.PopFlag ? 1 : 0)
                             + (format.LocData ? 1 : 0)
                             + (format.Phenotype ? 1 : 0);

            int genotypeCols = format.OneRowPerInd == true
                ? format.Ploidy * format.NumLoci
                : format.NumLoci;

            int expectedCols = metadataCols + format.ExtraCols + genotypeCols;

            // Сравнение
            return expectedRows == data.RowsCount && expectedCols == data.ColumnCount;
        }
    }
}
