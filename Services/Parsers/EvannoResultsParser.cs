using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Structure_Harvester;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Services.Parsers
{
    public class EvannoResultsParser
    {
        private readonly string STRUCTURE_HARVESTER_FOLDER_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_FOLDER_NAME;
        private readonly string STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME;

        private readonly IDirectoryService _directoryService = new DirectoryService();
        private readonly IFileService _fileService = new FileService();

        public List<EvannoParametersModel> Parse(string fullSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            var fullStructureHarvesterResultsFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullStructureHarvesterResultsFolderPath))
                throw new DirectoryNotFoundException($"Structure Harvester results folder not found: {fullStructureHarvesterResultsFolderPath}");

            try
            {
                var fullEvannoFilePath = Path.Combine(fullStructureHarvesterResultsFolderPath, "evanno.txt");

                if (!File.Exists(fullEvannoFilePath))
                    throw new FileNotFoundException("Evanno file not found.", fullSetFolderPath);

                var rows = new List<EvannoParametersModel>();

                var lines = _fileService.ReadFile(fullEvannoFilePath)
                    .Where(line => !line.TrimStart().StartsWith("#"))
                    .ToList();

                foreach (var line in lines)
                {
                    var parts = line.Split('\t');

                    if (parts.Length < 7)
                        continue;

                    if (parts[0].Trim().Equals("K", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!int.TryParse(parts[0].Trim(), out int k))
                        continue;

                    rows.Add(new EvannoParametersModel
                    {
                        K = k,
                        MeanLnPK = ParseDouble(parts[2]),
                        StdevLnPK = ParseDouble(parts[3]),
                        LnPrimeK = ParseNullableDouble(parts[4]),
                        LnDoublePrimeK = ParseNullableDouble(parts[5]),
                        DeltaK = ParseNullableDouble(parts[6])
                    });
                }

                return rows;
            }
            catch (Exception) { throw; }
        }

        private static double ParseDouble(string value)
        {
            return double.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        private static double? ParseNullableDouble(string value)
        {
            var trimmed = value.Trim();

            if (string.IsNullOrEmpty(trimmed) ||
                trimmed.Equals("NA", StringComparison.OrdinalIgnoreCase) ||
                trimmed == "—" || trimmed == "-")
            {
                return null;
            }

            if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return result;

            return null;
        }
    }
}
