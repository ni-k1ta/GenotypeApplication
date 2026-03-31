using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
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

        private readonly IDirectoryService _directoryService = new DirectoryService();

        private readonly object _lock = new();
        private bool _isRunning;

        public StructureHarvesterInteractionService(IDirectoryService directoryService, IFileService fileService)
        {
            _directoryService = directoryService;
        }

        public void PrepareStructureHarvesterDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullStructureHarvesterFolderPath = Path.Combine(fullCurrentSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME);
            Directory.CreateDirectory(fullStructureHarvesterFolderPath);
        }

        public async Task StartExecution(string fullSetFolderPath, bool evannoParam, bool clumppOutputParam/*, CancellationToken ct*/) // <-- расскоментить
        {
            var fullStructureFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_FOLDER_NAME);
            var fullStructureResultsFolderPath = Path.Combine(fullStructureFolderPath, STRUCTURE_RESULTS_FOLDER_NAME);

            bool hasStructureResults = _directoryService.IsDirectoryExist(fullStructureResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullStructureResultsFolderPath);

            if (!hasStructureResults)
                throw new FileNotFoundException("Не найдены результаты работы Structure"); //изменить

            var fullStructureHarvesterFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullStructureHarvesterFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullStructureHarvesterFolderPath} does not exist.");

            var fullResultsFolderPath = Path.Combine(fullStructureHarvesterFolderPath, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME);
            Directory.CreateDirectory(fullResultsFolderPath);

            var fullStructureHarvesterExecutableFilePath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, STRUCTURE_HARVESTER_EXECUTABLE_FILE_NAME);
            if (!File.Exists(fullStructureHarvesterExecutableFilePath)) throw new FileNotFoundException();

            IsRunning = true;

            Process? process = null;

            try
            {
                //ct.ThrowIfCancellationRequested(); <-- расскоментить

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
                    if (e.Data is not null)
                    {
                        Debug.WriteLine($"{e.Data}");
                        //OutputReceived?.Invoke(k, iteration, e.Data);
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                    {
                        //_logger.LogWarning(
                        //"STDERR [K={K}, i={Iteration}]: {Line}", k, iteration, e.Data);
                        Debug.WriteLine($"{e.Data}");
                        //OutputReceived?.Invoke(k, iteration, $"[STDERR] {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(/*ct*/); // <-- расскоментить
            }
            catch (OperationCanceledException) when (process is { HasExited: false })
            {
                //_logger.LogWarning(
                //"Killing process: K={K}, Iteration={Iteration}", k, iteration);

                try { process.Kill(entireProcessTree: true); }
                catch (Exception ex)
                {
                    // _logger.LogError(ex,
                    //"Failed to kill process: K={K}, Iteration={Iteration}", k, iteration);
                }

                throw;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                process?.Dispose();
                IsRunning = false;
            }
        }

        public bool IsRunning
        {
            get { lock (_lock) return _isRunning; }
            private set { lock (_lock) _isRunning = value; }
        }
    }
}
