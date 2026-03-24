using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Structure;
using System.Diagnostics;
using System.IO;

namespace GenotypeApplication.Services.Application_configuration.External_program_interaction
{
    public class StructureInteractionService
    {
        private readonly string STRUCTURE_FOLDER_NAME = StructureConstants.STRUCTURE_FOLDER_NAME;
        private readonly string STRUCTURE_EXECUTABLE_FILE_NAME = StructureConstants.STRUCTURE_EXECUTABLE_FILE_NAME;
        private readonly string STRUCTURE_MAINPARAMETERS_FILE_NAME = StructureConstants.STRUCTURE_MAINPARAMETERS_FILE_NAME;
        private readonly string STRUCTURE_EXTRAPARAMETERS_FILE_NAME = StructureConstants.STRUCTURE_EXTRAPARAMETERS_FILE_NAME;
        private readonly string STRUCTURE_RESULTS_FOLDER_NAME = StructureConstants.STRUCTURE_RESULTS_FOLDER_NAME;
        private readonly string EXTERNAL_PROGRAMS_FOLDER_PATH = PathConstants.EXTERNAL_PROGRAMS_DEFAULT_FOLDER_PATH;
        private readonly string STRUCTURE_OUTPUT_FILE_DEFAULT_NAME = StructureConstants.STRUCTURE_OUTPUT_FILE_DEFAULT_NAME;

        private readonly IFileService _fileService = new FileService();
        private readonly IDirectoryService _directoryService = new DirectoryService();

        //private readonly ILogger<StructureExecutionService> _logger;

        private CancellationTokenSource? _cts;
        private readonly object _lock = new();
        private bool _isRunning;

        //событие, вызываемое при получении строки вывода от процесса.
        public event Action<int, int, string>? OutputReceived;

        //событие, вызываемое при завершении одного юнита работы.
        public event Action<int, int, int, int, int>? UnitCompleted;

        public StructureInteractionService()
        {
            //_logger = logger;
        }

        public void PrepareStructureDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullStructureFolderPath = Path.Combine(fullCurrentSetFolderPath, STRUCTURE_FOLDER_NAME);
            Directory.CreateDirectory(fullStructureFolderPath);
        }

        public void PrepareInputDataFile(string fullCurrentSetFolderPath, string dataFileFullPath, string? savedDataFileFullPath = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(dataFileFullPath);

            var fullStructureFolderPath = Path.Combine(fullCurrentSetFolderPath, STRUCTURE_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullStructureFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullStructureFolderPath} does not exist.");

            if (!string.IsNullOrWhiteSpace(savedDataFileFullPath)) _fileService.DeleteFile(savedDataFileFullPath);

            _fileService.CopyFile(dataFileFullPath, Path.Combine(fullStructureFolderPath, Path.GetFileName(dataFileFullPath)));
        }

        public async Task PrepareParametersFile(string fullCurrentSetFolderPath, DataFileFormatModel dataFileFormatModel, StructureMainParametersModel structureMainParametersModel, StructureExtraParametersModel structureExtraParametersModel)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNull(dataFileFormatModel);
            ArgumentNullException.ThrowIfNull(structureMainParametersModel);
            ArgumentNullException.ThrowIfNull(structureExtraParametersModel);

            var structureMainParametersLines = DefineParameterModelConverter.GetFormatedLines(dataFileFormatModel)
            .Concat(DefineParameterModelConverter.GetFormatedLines(structureMainParametersModel))
            .ToList();

            var structureExtraParametersLines = (DefineParameterModelConverter.GetFormatedLines(structureExtraParametersModel)).ToList();

            var fullStructureFolderPath = Path.Combine(fullCurrentSetFolderPath, STRUCTURE_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullStructureFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullStructureFolderPath} does not exist.");

            var fullStructureParametersFilePath = Path.Combine(fullStructureFolderPath, STRUCTURE_MAINPARAMETERS_FILE_NAME);
            await _fileService.WriteAllLinesAsync(fullStructureParametersFilePath, structureMainParametersLines);

            var fullStructureExtraParametersFilePath = Path.Combine(fullStructureFolderPath, STRUCTURE_EXTRAPARAMETERS_FILE_NAME);
            await _fileService.WriteAllLinesAsync(fullStructureExtraParametersFilePath, structureExtraParametersLines);
        }

        public async Task StartExecution(int kFrom, int kTo, int iterations, string fullSetFolderPath, int coresCount)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (kFrom > kTo)
                throw new ArgumentException($"kFrom ({kFrom}) must be <= kTo ({kTo}).");
            if (iterations <= 0)
                throw new ArgumentException("iterations must be > 0.");
            if (coresCount <= 0)
                throw new ArgumentException("coresCount must be > 0.");

            var fullStructureFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullStructureFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullStructureFolderPath} does not exist.");

            var fullResultsFolderPath = Path.Combine(fullStructureFolderPath, STRUCTURE_RESULTS_FOLDER_NAME);
            Directory.CreateDirectory(fullResultsFolderPath);

            var fullStructureExecutableFilePath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, STRUCTURE_EXECUTABLE_FILE_NAME);
            if (!File.Exists(fullStructureExecutableFilePath)) throw new FileNotFoundException();

            var fullStructureOutFilePath = Path.Combine(fullResultsFolderPath, STRUCTURE_OUTPUT_FILE_DEFAULT_NAME);

            var units = new List<(int k, int iteration)>();
            for (int k = kFrom; k <= kTo; k++)
                for (int i = 1; i <= iterations; i++)
                    units.Add((k, i));

            int totalUnits = units.Count;
            int completedUnits = 0;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            IsRunning = true;

            using var semaphore = new SemaphoreSlim(coresCount, coresCount);

            try
            {
                var tasks = units.Select(unit => RunSingleProcessAsync(
               unit.k,
               unit.iteration,
               fullStructureExecutableFilePath,
               fullStructureFolderPath,
               fullStructureOutFilePath,
               semaphore,
               token,
               () =>
               {
                   int completed = Interlocked.Increment(ref completedUnits);
                   return (completed, totalUnits); //нужно только для проброса выше (например во ViewModel для отображения прогресса - этим мы займёмся позже) и не используется нигде для непосредственно логики 
               }));

                await Task.WhenAll(tasks);

                //_logger.LogInformation("All {Total} units completed successfully.", totalUnits);
            }
            catch (Exception)
            {
                //_logger.LogWarning(
                //"Execution was cancelled. Completed {Completed}/{Total} units.",
                //completedUnits, totalUnits);
                throw;
            }
            finally
            {
                IsRunning = false;
                _cts.Dispose();
                _cts = null;
            }
        }
        private async Task RunSingleProcessAsync(
        int k,
        int iteration,
        string executablePath,
        string workingDirectory,
        string outFilePath,
        SemaphoreSlim semaphore,
        CancellationToken ct,
        Func<(int completed, int total)> reportProgress)
        {
            await semaphore.WaitAsync(ct);

            Process? process = null;

            try
            {
                ct.ThrowIfCancellationRequested();

                var arguments = $"-K {k} -o {outFilePath}K{k}-i{iteration}";

                //_logger.LogInformation(
                //"Starting process: K={K}, Iteration={Iteration}, Args=\"{Args}\"",
                //k, iteration, arguments);

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                    {
                        Debug.WriteLine($"[K={k}, i={iteration}] {e.Data}");
                        OutputReceived?.Invoke(k, iteration, e.Data);
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                    {
                        //_logger.LogWarning(
                        //"STDERR [K={K}, i={Iteration}]: {Line}", k, iteration, e.Data);
                        Debug.WriteLine($"[K={k}, i={iteration}] [STDERR] {e.Data}");
                        OutputReceived?.Invoke(k, iteration, $"[STDERR] {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Debug.WriteLine($"Started [K={k}, i={iteration}] PID={process.Id}");

                await process.WaitForExitAsync(ct);

                Debug.WriteLine($"Finished [K={k}, i={iteration}] ExitCode={process.ExitCode}");

                var (completed, total) = reportProgress();

                //_logger.LogInformation(
                //"Process finished: K={K}, Iteration={Iteration}, " +
                //"ExitCode={ExitCode}, Progress={Completed}/{Total}",
                //k, iteration, process.ExitCode, completed, total);

                UnitCompleted?.Invoke(k, iteration, process.ExitCode, completed, total);
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
            finally
            {
                process?.Dispose();
                semaphore.Release();
            }
        }
        public void StopExecution()
        {
            if (_cts is { IsCancellationRequested: false })
                _cts.Cancel();
        }

        public bool IsRunning
        {
            get { lock (_lock) return _isRunning; }
            private set { lock (_lock) _isRunning = value; }
        }
    }
}
