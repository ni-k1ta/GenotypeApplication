using GenotypeApplication.Models.Structure;

namespace GenotypeApplication.Services.Application_configuration.Data_file_scanners
{
    public class FormatDetectorsHelper
    {
        public int GetFirstDataRow(DataFileFormatModel format)
        {
            int row = 0;
            if (format.MarkerNames) row++;
            if (format.RecessiveAlleles) row++;
            if (format.MapDistances) row++;
            return row;
        }
        public int GetCurrentDataColumn(DataFileFormatModel format)
        {
            int column = 0;
            if (format.Label) column++;
            if (format.PopData) column++;
            if (format.PopFlag) column++;
            if (format.LocData) column++;
            if (format.Phenotype) column++;
            column += format.ExtraCols;
            return column;
        }

        public (bool HasPattern, int RepeatCount) HasRepeatPattern(IReadOnlyList<string> values)
        {
            if (values.Count < 2)
                return (false, 0);

            var groupSizes = new List<int>();
            int currentSize = 1;

            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] == values[i - 1])
                    currentSize++;
                else
                {
                    groupSizes.Add(currentSize);
                    currentSize = 1;
                }
            }

            groupSizes.Add(currentSize);

            // убираем последнюю группу если она неполная
            if (groupSizes.Count > 1 && groupSizes[^1] < groupSizes[0])
                groupSizes.RemoveAt(groupSizes.Count - 1);

            var distinctSizes = groupSizes.Distinct().ToList();

            if (distinctSizes.Count == 1 && distinctSizes[0] > 1)
                return (true, distinctSizes[0]);

            return (false, 0);
        }
    }
}
