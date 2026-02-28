using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Services.Application_configuration.Data_file_scanners;

namespace GenotypeApplication.Services.Application_configuration.Data_file.Detectors
{
    public class GenotypeDataDetector : FormatDetectorBase
    {
        private const int _order = 220;
        public override int Order => _order;

        public override void Detect(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            var startRow = FormatDetectorsHelper.GetFirstDataRow(format);
            var dataRowsCount = data.RowsCount - startRow;

            var startCol = FormatDetectorsHelper.GetCurrentDataColumn(format);
            var dataColsCount = data.ColumnCount - startCol;

            //Again Ploidy
            if (format.Ploidy == 0)
            {
                if (startRow != 0)
                {
                    var firstRowValues = data.GetRow(0).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    format.Ploidy = dataColsCount / firstRowValues.Length;
                }
                else
                {
                    format.Ploidy = 2;
                }
            }

            //NumLoci
            if (startRow != 0)
            {
                var firstRowValues = data.GetRow(0).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                format.NumLoci = firstRowValues.Length;
            }
            else if (format.PHASEINFO)
            {
                var lastRowValues = data.GetRow(data.RowsCount - 1).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                format.NumLoci = lastRowValues.Length;
            }
            //else if (format.OneRowPerInd == false || format.Ploidy == 1)
            //{
            //    format.NumLoci = dataColsCount;
            //}
            else if (format.OneRowPerInd == true && format.Ploidy > 1)
            {
                format.NumLoci = dataColsCount / format.Ploidy;
            }

            //NumInds
            if (format.Ploidy != 0 && format.OneRowPerInd == false)
            {
                format.NumInds = dataRowsCount / (format.Ploidy + (format.PHASEINFO ? 1 : 0));
            }
            else if (format.OneRowPerInd == true)
            {
                format.NumInds = dataRowsCount / (format.PHASEINFO ? 2 : 1);
            }
        }
    }
}
