using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Services.Set;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GenotypeApplication.Services.Application_configuration.External_program_interaction
{
    public class CLUMPPInteractionService
    {
        private readonly string CLUMPP_EXECUTABLE_FILE_NAME = CLUMPPConstants.CLUMPP_EXECUTABLE_FILE_NAME;
        private readonly string CLUMPP_FOLDER_NAME = CLUMPPConstants.CLUMPP_FOLDER_NAME;
        private readonly string CLUMPP_RESULTS_FOLDER_NAME = CLUMPPConstants.CLUMPP_RESULTS_FOLDER_NAME;
        private readonly string EXTERNAL_PROGRAMS_FOLDER_PATH = PathConstants.EXTERNAL_PROGRAMS_DEFAULT_FOLDER_PATH;
        private readonly string CLUMPP_POP_PARAMFILE_POSTFIX = CLUMPPConstants.CLUMPP_POP_PARAMFILE_POSTFIX;
        private readonly string CLUMPP_INDV_PARAMFILE_POSTFIX = CLUMPPConstants.CLUMPP_INDV_PARAMFILE_POSTFIX;

        private readonly string STRUCTURE_HARVESTER_FOLDER_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_FOLDER_NAME;
        private readonly string STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME = StructureHarvesterConstants.STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME;

        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        private CancellationTokenSource? _cts;
        private readonly object _lock = new();
        private bool _isRunning;

        //событие, вызываемое при получении строки вывода от процесса.
        public event Action<int, string>? OutputReceived;

        //событие, вызываемое при завершении одного юнита работы.
        public event Action<int, int, int, int>? UnitCompleted;

        public CLUMPPInteractionService(IDirectoryService directoryService, IFileService fileService)
        {
            _directoryService = directoryService;
            _fileService = fileService;
            //_logger = logger;
        }

        public void PrepareCLUMPPDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME);
            Directory.CreateDirectory(fullCLUMPPFolderPath);
        }

        public async Task PrepareConfiguration(string fullCurrentSetFolderPath, CLUMPPConfigurationModel configurationParametersModel, bool isPop, int popCount, bool isIndv, int indvCount)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNull(configurationParametersModel);

            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullCLUMPPFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullCLUMPPFolderPath} does not exist.");

            var fullConfigurationFolderPath = Path.Combine(fullCLUMPPFolderPath, configurationParametersModel.ParametersName);

            Directory.CreateDirectory(fullConfigurationFolderPath);

            var fullConfigurationFilePath = Path.Combine(fullConfigurationFolderPath, configurationParametersModel.ParametersName);

            List<string> configurationParametersLines;

            if (isIndv)
            {
                configurationParametersModel.DATATYPE = false;
                configurationParametersModel.C = indvCount;
                configurationParametersModel.W = false;
                configurationParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationParametersModel).ToList();
                await _fileService.WriteAllLinesAsync(fullConfigurationFilePath + CLUMPP_INDV_PARAMFILE_POSTFIX, configurationParametersLines);
            }
            if (isPop)
            {
                configurationParametersModel.DATATYPE = true;
                configurationParametersModel.C = popCount;
                configurationParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationParametersModel).ToList();
                await _fileService.WriteAllLinesAsync(fullConfigurationFilePath + CLUMPP_POP_PARAMFILE_POSTFIX, configurationParametersLines);
            }
        }

        private record CLUMPPJob(int K, string ParametersFileName, string DataType, string InputFilePath, string OutFilePath, string OutMiscfilePath);

        private List<CLUMPPJob> BuildJobs(int kFrom, int kTo, bool isPop, bool isIndv, string configurationName)
        {
            var jobs = new List<CLUMPPJob>();

            for (int k = kFrom; k <= kTo; k++)
            {
                if (isPop)
                {
                    jobs.Add(new CLUMPPJob(
                        K: k,
                        ParametersFileName: (configurationName + CLUMPP_POP_PARAMFILE_POSTFIX),
                        DataType: "-p",
                        InputFilePath: Path.Combine("..", "..", STRUCTURE_HARVESTER_FOLDER_NAME, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME, $"K{k}.popfile"),
                        OutFilePath: Path.Combine(CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.popq"),
                        OutMiscfilePath: Path.Combine(CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.popq.miscfile")));
                }

                if (isIndv)
                {
                    jobs.Add(new CLUMPPJob(
                        K: k,
                        ParametersFileName: (configurationName + CLUMPP_INDV_PARAMFILE_POSTFIX),
                        DataType: "-i",
                        InputFilePath: Path.Combine("..", "..", STRUCTURE_HARVESTER_FOLDER_NAME, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME, $"K{k}.indfile"),
                        OutFilePath: Path.Combine(CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.indq"),
                        OutMiscfilePath: Path.Combine(CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.indq.miscfile")));
                }
            }

            return jobs;
        }

        public async Task StartExecution(string configurationName, bool isPop, bool isIndv, int kFrom, int kTo, string fullSetFolderPath, int coresCount)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (kFrom > kTo)
                throw new ArgumentException($"kFrom ({kFrom}) must be <= kTo ({kTo}).");
            if (coresCount <= 0)
                throw new ArgumentException("coresCount must be > 0.");
            if (!isPop && !isIndv)
                throw new ArgumentException("At least one of isPop or isIndv must be true.");

            var fullStructureHarvesterResultsFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_HARVESTER_FOLDER_NAME, STRUCTURE_HARVESTER_RESULTS_FOLDER_NAME);
            bool hasStructureHarvesterResults = _directoryService.IsDirectoryExist(fullStructureHarvesterResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullStructureHarvesterResultsFolderPath);
            if (!hasStructureHarvesterResults)
                throw new FileNotFoundException("Не найдены результаты работы Structure Harvester"); //изменить

            var fullConfigurationFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME, configurationName);
            if (!_directoryService.IsDirectoryExist(fullConfigurationFolderPath))
                throw new DirectoryNotFoundException(
                    $"The directory {fullConfigurationFolderPath} does not exist.");

            var fullResultsFolderPath = Path.Combine(fullConfigurationFolderPath, CLUMPP_RESULTS_FOLDER_NAME);
            Directory.CreateDirectory(fullResultsFolderPath);

            var fullCLUMPPExecutableFilePath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, CLUMPP_EXECUTABLE_FILE_NAME);
            if (!File.Exists(fullCLUMPPExecutableFilePath)) throw new FileNotFoundException();

            var jobs = BuildJobs(
                       kFrom, kTo, isPop, isIndv,
                       configurationName);

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
                fullCLUMPPExecutableFilePath,
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

        private async Task RunSingleProcessAsync(CLUMPPJob job, string executablePath, string workingDirectory, SemaphoreSlim semaphore, CancellationToken ct, Func<(int completed, int total)> reportProgress)
        {
            await semaphore.WaitAsync(ct);

            Process? process = null;

            try
            {
                ct.ThrowIfCancellationRequested();

                string highestHLine = string.Empty;

                var arguments =
                    $"{job.ParametersFileName}" +
                    $" {job.DataType}" +
                    $" \"{job.InputFilePath}\"" +
                    $" -o {job.OutFilePath}" +
                    $" -j {job.OutMiscfilePath}" +
                    $" -k {job.K}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;

                    Debug.WriteLine($"[K={job.K}] {e.Data}");
                    OutputReceived?.Invoke(job.K, e.Data);

                    if (e.Data.StartsWith("The highest value of H", StringComparison.OrdinalIgnoreCase)
                        || e.Data.StartsWith("The highest value of G", StringComparison.OrdinalIgnoreCase))
                    {
                        highestHLine = e.Data;
                    }

                    if (e.Data.StartsWith("The program finished in", StringComparison.OrdinalIgnoreCase))
                    {
                        process.StandardInput.WriteLine();
                    }
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

                if (!string.IsNullOrWhiteSpace(highestHLine))
                {
                    try
                    {
                        var hvFileName = $"K{job.K}_hv.txt";
                        var hvFilePath = Path.Combine(workingDirectory, hvFileName);

                        await File.WriteAllTextAsync(
                            hvFilePath,
                            highestHLine + Environment.NewLine,
                            Encoding.UTF8
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[K={job.K}] Can't write H/G value: {ex.Message}");
                        //_logger.Error($"[K={job.K}] {(job.type ? ("Can't write H value in new file: ") : ("Can't write G value in new file: "))} {ex.Message}");
                    }
                }

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
        public void StopExecution()
        {
            if (_cts is { IsCancellationRequested: false })
                _cts.Cancel();
        }

        public bool IsConfigurationExist(string fullCurrentSetFolderPath, string configurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME);
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
