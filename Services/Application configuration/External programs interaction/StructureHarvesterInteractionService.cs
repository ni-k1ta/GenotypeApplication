using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Services.Application_configuration.Logger;
using System.Diagnostics;
using System.IO;

namespace GenotypeApplication.Services.Application_configuration.External_program_interaction
{
    public class StructureHarvesterInteractionService
    {
        private readonly string STRUCTURE_HARVESTER_EXECUTABLE_FILE_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_EXECUTABLE_FILE_NAME;
        private readonly string STRUCTURE_HARVESTER_FOLDER_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_FOLDER_NAME;
        private readonly string STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME;
        private readonly string EXTERNAL_PROGRAMS_FOLDER_PATH = PathConstants.EXTERNAL_PROGRAMS_DEFAULT_FOLDER_PATH;

        private readonly string STRUCTURE_FOLDER_NAME = StructureConstants.STRUCTURE_FOLDER_NAME;
        private readonly string STRUCTURE_RESULTS_FOLDER_NAME = StructureConstants.STRUCTURE_RESULTS_FOLDER_NAME;

        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        private readonly object _lock = new();
        private bool _isRunning;

        private ProgramLogger _logger;

        public StructureHarvesterInteractionService(IDirectoryService directoryService, IFileService fileService, ProgramLogger logger)
        {
            _directoryService = directoryService;
            _fileService = fileService;
            _logger = logger;
        }

        public void PrepareStructureHarvesterDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullStructureHarvesterFolderPath = Path.Combine(fullCurrentSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME);
            Directory.CreateDirectory(fullStructureHarvesterFolderPath);
        }

        public async Task<(bool evanno, bool clumpp)> LoadConfiguration(string fullSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            var fullStructureHarvesterResultsFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullStructureHarvesterResultsFolderPath))
                throw new DirectoryNotFoundException($"Structure Harvester result folder for set {Path.GetFileName(fullSetFolderPath)} was not found.");

            bool evanno = File.Exists(Path.Combine(fullStructureHarvesterResultsFolderPath, "evanno.txt"));

            bool clumpp = Directory.EnumerateFiles(fullStructureHarvesterResultsFolderPath).Any(f =>
                {
                    var name = Path.GetFileName(f);
                    return name.EndsWith(".indfile", StringComparison.OrdinalIgnoreCase) ||
                           name.EndsWith(".popfile", StringComparison.OrdinalIgnoreCase);
                });

            return (evanno, clumpp);
        }

        public async Task StartExecution(string fullSetFolderPath, bool evannoParam, bool clumppOutputParam)
        {
            var fullStructureFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_FOLDER_NAME);
            var fullStructureResultsFolderPath = Path.Combine(fullStructureFolderPath, STRUCTURE_RESULTS_FOLDER_NAME);

            bool hasStructureResults = _directoryService.IsDirectoryExist(fullStructureResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullStructureResultsFolderPath);

            if (!hasStructureResults)
                throw new FileNotFoundException("Structure results were not found.");

            var fullStructureHarvesterFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullStructureHarvesterFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullStructureHarvesterFolderPath} does not exist.");

            var fullResultsFolderPath = Path.Combine(fullStructureHarvesterFolderPath, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME);
            Directory.CreateDirectory(fullResultsFolderPath);

            var fullStructureHarvesterExecutableFilePath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, STRUCTURE_HARVESTER_EXECUTABLE_FILE_NAME);
            if (!File.Exists(fullStructureHarvesterExecutableFilePath)) throw new FileNotFoundException();

            var ct = CancellationTokenSource.CreateLinkedTokenSource(App.GlobalCts.Token);
            var token = ct.Token;

            IsRunning = true;

            Process? process = null;

            try
            {
                token.ThrowIfCancellationRequested(); 

                var arguments = $"--dir=\"{fullStructureResultsFolderPath}\"" +
                            $" --out=\"{fullResultsFolderPath}\"" +
                            (evannoParam ? " --evanno" : string.Empty) +
                            (clumppOutputParam ? " --clumpp" : string.Empty);

                var startInfo = new ProcessStartInfo
                {
                    FileName = fullStructureHarvesterExecutableFilePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;

                    _logger.Info($"{e.Data}");
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;
                    _logger.Error($"{e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(token);
            }
            catch (OperationCanceledException) when (process is { HasExited: false })
            {
                try { process.Kill(entireProcessTree: true); }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to kill process: {ex.Message}");
                }

                throw;
            }
            catch (Exception) { throw; }
            finally
            {
                process?.Dispose();
                IsRunning = false;
            }
        }

        public int GetPopulationsCountFromResults(string fullSetFolderPath)
        {
            var directoryPath = Path.Combine(fullSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME);

            string? filePath = Directory.GetFiles(directoryPath, "*.popfile").FirstOrDefault()
                     ?? Directory.GetFiles(directoryPath, "*.indfile").FirstOrDefault();

            if (filePath == null) return 0;

            bool isPopFile = filePath.EndsWith(".popfile", StringComparison.OrdinalIgnoreCase);

            var seenIds = new List<int>();

            foreach (string line in _fileService.ReadFile(filePath))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                int val;

                if (isPopFile)
                {
                    int colonIndex = trimmed.IndexOf(':');
                    if (colonIndex <= 0 || !int.TryParse(trimmed.AsSpan(0, colonIndex).Trim(), out val))
                        continue;
                }
                else
                {
                    string[] parts = trimmed.Split(':', 2);
                    if (parts.Length < 2) continue;

                    string[] tokens = parts[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length < 4 || !int.TryParse(tokens[3], out val))
                        continue;
                }

                if (seenIds.Contains(val))
                    break;

                seenIds.Add(val);
            }

            return seenIds.Count;
        }

        public bool IsRunning
        {
            get { lock (_lock) return _isRunning; }
            private set { lock (_lock) _isRunning = value; }
        }
    }
}
