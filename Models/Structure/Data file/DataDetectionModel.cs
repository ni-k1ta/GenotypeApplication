namespace GenotypeApplication.Models.Structure.Data_file
{
    public class DataDetectionModel
    {
        public DataTableModel Data { get; }
        public DataFileFormatModel Format { get; } = new();

        public DataDetectionModel(DataTableModel data)
        {
            Data = data ?? throw new ArgumentNullException("Data table from data file can't be null!");
        }
    }
}
