using GenotypeApplication.Interfaces;
using System.Data;
using System.IO;

namespace GenotypeApplication.Services
{
    public class DataTableService : IDataTableService
    {
        public DataTable Load(string filePath)
        {
            try
            {
                IFileService fileService = new FileService();

                var nonEmptyLines = fileService.ReadFile(filePath);

                if (!nonEmptyLines.Any()) throw new InvalidDataException("Data file can't be empty!"); //todo возможно переделать на уведомление, что файл пустой

                var parsed = new List<(int offset, string[] tokens)>();
                int maxCols = 0;

                foreach (var line in nonEmptyLines)
                {
                    int offset = line.TakeWhile(char.IsWhiteSpace).Count();
                    var tokens = line.TrimStart().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                    parsed.Add((offset, tokens));
                    maxCols = Math.Max(maxCols, offset + tokens.Length);
                }

                var table = new DataTable();
                for (int i = 0; i < maxCols; i++)
                    table.Columns.Add($"Column{i + 1}", typeof(string));

                foreach (var (offset, tokens) in parsed)
                {
                    var row = table.NewRow();
                    for (int j = 0; j < tokens.Length; j++)
                        row[offset + j] = tokens[j];

                    table.Rows.Add(row);
                }

                return table;
            }
            catch (FileNotFoundException fnfe)
            {
                throw new FileNotFoundException("Data file was not found.", fnfe.FileName);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
