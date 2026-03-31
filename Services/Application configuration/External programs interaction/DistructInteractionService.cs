using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models;
using GenotypeApplication.Services.Set;
using System.Diagnostics;
using System.IO;

namespace GenotypeApplication.Services.Application_configuration.External_program_interaction
{
    public class DistructInteractionService
    {
        private readonly string DISTRUCT_EXECUTABLE_FILE_NAME = DistructConstants.DISTRUCT_EXECUTABLE_FILE_NAME;
        private readonly string DISTRUCT_FOLDER_NAME = DistructConstants.DISTRUCT_FOLDER_NAME;
        private readonly string DISTRUCT_RESULTS_FOLDER_NAME = DistructConstants.DISTRUCT_RESULTS_FOLDER_NAME;

        private readonly string EXTERNAL_PROGRAMS_FOLDER_PATH = PathConstants.EXTERNAL_PROGRAMS_DEFAULT_FOLDER_PATH;

        private readonly string CLUMPP_FOLDER_NAME = CLUMPPConstants.CLUMPP_FOLDER_NAME;
        private readonly string CLUMPP_RESULTS_FOLDER_NAME = CLUMPPConstants.CLUMPP_RESULTS_FOLDER_NAME;

        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        private CancellationTokenSource? _cts;
        private readonly object _lock = new();
        private bool _isRunning;

        //событие, вызываемое при получении строки вывода от процесса.
        public event Action<int, string>? OutputReceived;

        //событие, вызываемое при завершении одного юнита работы.
        public event Action<int, int, int, int>? UnitCompleted;

        public DistructInteractionService(IDirectoryService directoryService, IFileService fileService)
        {
            _directoryService = directoryService;
            _fileService = fileService;
            //_logger = logger;
        }

        public void PrepareDistructDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME);
            Directory.CreateDirectory(fullCLUMPPFolderPath);
        }

        public async Task PrepareConfiguration(string fullCurrentSetFolderPath, DistructConfigurationModel configurationModel)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNull(configurationModel);

            var fullDistructFolderPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullDistructFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullDistructFolderPath} does not exist.");

            var fullConfigurationFolderPath = Path.Combine(fullDistructFolderPath, configurationModel.ParametersName);

            Directory.CreateDirectory(fullConfigurationFolderPath);

            var fullConfigurationFilePath = Path.Combine(fullConfigurationFolderPath, configurationModel.ParametersName);

            var configurationParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationModel, "#define").ToList();
            await _fileService.WriteAllLinesAsync(fullConfigurationFilePath, configurationParametersLines);
        }

        private record DistructJob(int K, string ParametersFileName, string InputFilePopqPath, string InputFileIndvqPath, string OutFilePath);
        private List<DistructJob> BuildJobs(int kFrom, int kTo, string configurationName, string CLUMPPResultsPath, string resultsPath)
        {
            var jobs = new List<DistructJob>();

            for (int k = kFrom; k <= kTo; k++)
            {
                jobs.Add(new DistructJob(
                    k,
                    configurationName,
                    Path.Combine(CLUMPPResultsPath, $"K{k}.popq"),
                    Path.Combine(CLUMPPResultsPath, $"K{k}.indq"),
                    Path.Combine(resultsPath, $"K{k}.ps")
                    ));
            }

            return jobs;
        }
        public async Task StartExecution(string configurationName, int kFrom, int kTo, string fullSetFolderPath, int coresCount)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (kFrom > kTo)
                throw new ArgumentException($"kFrom ({kFrom}) must be <= kTo ({kTo}).");
            if (coresCount <= 0)
                throw new ArgumentException("coresCount must be > 0.");

            var fullCLUMPPResultsFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME, CLUMPP_RESULTS_FOLDER_NAME);
            bool hasStructureHarvesterResults = _directoryService.IsDirectoryExist(fullCLUMPPResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullCLUMPPResultsFolderPath);
            if (!hasStructureHarvesterResults)
                throw new FileNotFoundException("Не найдены результаты работы CLUMPP"); //изменить

            var fullConfigurationFolderPath = Path.Combine(fullSetFolderPath, DISTRUCT_FOLDER_NAME, configurationName);
            if (!_directoryService.IsDirectoryExist(fullConfigurationFolderPath))
                throw new DirectoryNotFoundException(
                    $"The directory {fullConfigurationFolderPath} does not exist.");

            var fullResultsFolderPath = Path.Combine(fullConfigurationFolderPath, DISTRUCT_RESULTS_FOLDER_NAME);
            Directory.CreateDirectory(fullResultsFolderPath);

            var fullDistructExecutableFilePath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, DISTRUCT_EXECUTABLE_FILE_NAME);
            if (!File.Exists(fullDistructExecutableFilePath)) throw new FileNotFoundException();

            var jobs = BuildJobs(
                      kFrom, kTo,
                      configurationName,
                      fullCLUMPPResultsFolderPath,
                      fullResultsFolderPath);

            int totalUnits = jobs.Count;
            int completedUnits = 0;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            IsRunning = true;

            using var semaphore = new SemaphoreSlim(coresCount, coresCount);

            try
            {
                var tasks = jobs.Select(job => RunSingleProcessAsync(
                job,
                fullDistructExecutableFilePath,
                fullConfigurationFolderPath,
                semaphore,
                token,
                () =>
                {
                    int completed = Interlocked.Increment(ref completedUnits);
                    return (completed, totalUnits);
                }));

                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                IsRunning = false;
                _cts.Dispose();
                _cts = null;
            }
        }
        private async Task RunSingleProcessAsync(DistructJob job, string executablePath, string workingDirectory, SemaphoreSlim semaphore, CancellationToken ct, Func<(int completed, int total)> reportProgress)
        {
            await semaphore.WaitAsync(ct);

            Process? process = null;

            try
            {
                ct.ThrowIfCancellationRequested();

                var arguments =
                    $"-d {job.ParametersFileName}" +
                    $" -K {job.K}" +
                    $" -p {job.InputFilePopqPath}" +
                    $" -i {job.InputFileIndvqPath}" +
                    $" -o {job.OutFilePath}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;

                    Debug.WriteLine($"[K={job.K}] {e.Data}");
                    OutputReceived?.Invoke(job.K, e.Data);
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                    {
                        Debug.WriteLine($"[K={job.K}] [STDERR] {e.Data}");
                        OutputReceived?.Invoke(job.K, $"[STDERR] {e.Data}");

                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Debug.WriteLine($"Started [K={job.K}] PID={process.Id}");

                await process.WaitForExitAsync(ct);

                Debug.WriteLine($"Finished [K={job.K}] ExitCode={process.ExitCode}");

                var (completed, total) = reportProgress();

                //_logger.LogInformation(
                //"Process finished: K={K}, Iteration={Iteration}, " +
                //"ExitCode={ExitCode}, Progress={Completed}/{Total}",
                //k, iteration, process.ExitCode, completed, total);

                UnitCompleted?.Invoke(job.K, process.ExitCode, completed, total);
            }
            catch (OperationCanceledException) when (process is { HasExited: false })
            {
                //_logger.LogWarning(
                //"Killing process: K={K}, Iteration={Iteration}", k, iteration);

                try { process.Kill(entireProcessTree: true); }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[K={job.K}] Failed to kill: {ex.Message}");

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
                semaphore.Release();
            }
        }

        public bool IsConfigurationExist(string fullCurrentSetFolderPath, string configurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME);
            var fullConfigurationFolderPath = Path.Combine(fullCLUMPPFolderPath, configurationName);
            return _directoryService.IsDirectoryExist(fullConfigurationFolderPath);
        }

        public bool IsRunning
        {
            get { lock (_lock) return _isRunning; }
            private set { lock (_lock) _isRunning = value; }
        }
    }
}
