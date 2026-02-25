using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Structure.Data_file;
using System.Globalization;

namespace GenotypeApplication.Services.Application_configuration.Data_file_scanners
{
    public abstract class FormatDetectorBase : IFormatDetector
    {
        public abstract int Order { get; }
        public abstract void Detect(DataDetectionModel dataDetectionModel);

        public FormatDetectorsHelper FormatDetectorsHelper { get; } = new FormatDetectorsHelper();

        protected (bool IsSuccess, string[] Values) PrepareData(DataDetectionModel dataDetectionModel)
        {
            ArgumentNullException.ThrowIfNull(dataDetectionModel);

            var data = dataDetectionModel.Data;
            var format = dataDetectionModel.Format;

            if (data.IsEmpty)
            {
                return (false, new List<string>().ToArray());
            }

            int rowIndex = FormatDetectorsHelper.GetFirstDataRow(format);
            int columnIndex = FormatDetectorsHelper.GetCurrentDataColumn(format);

            if (columnIndex >= data.ColumnCount)
            {
                return (false, new List<string>().ToArray());
            }

            var column = data.GetColumn(columnIndex, rowIndex);
            var columnValues = column.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (columnValues.Length == 0) return (false, new List<string>().ToArray());

            return (true, columnValues);
        }

        

        /// <summary>
        /// Проверяет, является ли строковое значение целым числом (включая отрицательные).
        /// </summary>
        protected bool IsInteger(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }
        /// <summary>
        /// Проверяет, является ли строковое значение вещественным числом.
        /// </summary>
        protected bool IsFloat(string value)
        {
            return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out _);
        }
        /// <summary>
        /// Содержит ли строка хотя бы одно нечисловое значение.
        /// </summary>
        protected bool RowHasNonNumeric(string[] row)
        {
            return row.Any(v => !IsFloat(v));
        }
        /// <summary>
        /// Содержит ли строка хотя бы одно дробное (не целое) число.
        /// </summary>
        public bool RowHasFloats(string[] row)
        {
            foreach (var v in row)
            {
                if (IsFloat(v) && !IsInteger(v))
                    return true;
            }
            return false;
        }
    }
}
