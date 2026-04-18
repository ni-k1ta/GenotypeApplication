using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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

        public event Action<double>? ProgressChanged;
        private readonly Dictionary<(int k, string dataType), double> _unitProgress = new();
        int _totalUnits = 1;

        private ProgramLogger _logger;

        public CLUMPPInteractionService(IDirectoryService directoryService, IFileService fileService, ProgramLogger logger)
        {
            _directoryService = directoryService;
            _fileService = fileService;
            _logger = logger;
        }

        public void PrepareCLUMPPDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME);
            Directory.CreateDirectory(fullCLUMPPFolderPath);
        }

        public async Task PrepareConfiguration(string fullCurrentSetFolderPath, CLUMPPConfigurationModel configurationParameters, bool isPop, int popCount, bool isIndv, int indvCount)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNull(configurationParameters);

            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullCLUMPPFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullCLUMPPFolderPath} does not exist.");

            var fullConfigurationFolderPath = Path.Combine(fullCLUMPPFolderPath, configurationParameters.ParametersName);

            Directory.CreateDirectory(fullConfigurationFolderPath);

            var fullConfigurationFilePath = Path.Combine(fullConfigurationFolderPath, configurationParameters.ParametersName);

            CLUMPPConfigurationModel tempConfiguration = new CLUMPPConfigurationModel(configurationParameters);

            List<string> configurationParametersLines;

            if (isIndv)
            {
                tempConfiguration.DATATYPE = false;
                tempConfiguration.C = indvCount;
                tempConfiguration.W = false;
                configurationParametersLines = DefineParameterModelConverter.GetFormatedLines(tempConfiguration).ToList();
                await _fileService.WriteAllLinesAsync(fullConfigurationFilePath + CLUMPP_INDV_PARAMFILE_POSTFIX, configurationParametersLines);
            }
            if (isPop)
            {
                tempConfiguration.DATATYPE = true;
                tempConfiguration.C = popCount;
                configurationParametersLines = DefineParameterModelConverter.GetFormatedLines(tempConfiguration).ToList();
                await _fileService.WriteAllLinesAsync(fullConfigurationFilePath + CLUMPP_POP_PARAMFILE_POSTFIX, configurationParametersLines);
            }
        }

        public async Task<List<CLUMPPConfigurationModel>> LoadConfigurationsListAsync(string fullSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (!_directoryService.IsDirectoryExist(fullSetFolderPath)) throw new DirectoryNotFoundException($"Set folder \"{fullSetFolderPath}\" was not found.");

            string fullCLUMPPFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullCLUMPPFolderPath)) throw new DirectoryNotFoundException($"CLUMPP folder was not found in set folder: {fullCLUMPPFolderPath}");

            try
            {
                List<CLUMPPConfigurationModel> configurations = new();

                foreach (var configurationFolder in Directory.EnumerateDirectories(fullCLUMPPFolderPath))
                {
                    var configurationName = Path.GetFileName(configurationFolder);

                    var (configurationModel, _, _, _, _) = await LoadConfiguration(fullSetFolderPath, configurationName);

                    configurations.Add(configurationModel);
                }

                return configurations;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<(CLUMPPConfigurationModel configurationModel, bool isPop, bool isIndv, int kStart, int kEnd)> LoadConfiguration(string fullSetFolderPath, string configurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (!_directoryService.IsDirectoryExist(fullSetFolderPath)) throw new DirectoryNotFoundException($"Set folder \"{fullSetFolderPath}\" was not found.");

            string fullCLUMPPFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullCLUMPPFolderPath)) throw new DirectoryNotFoundException($"CLUMPP folder was not found in set folder: {fullCLUMPPFolderPath}");

            string fullConfigurationFolderPath = Path.Combine(fullCLUMPPFolderPath, configurationName);

            try
            {
                var popParametersFilePath = Path.Combine(fullConfigurationFolderPath, $"{configurationName}{CLUMPP_POP_PARAMFILE_POSTFIX}");
                var indvParametersFilePath = Path.Combine(fullConfigurationFolderPath, $"{configurationName}{CLUMPP_INDV_PARAMFILE_POSTFIX}");

                // Проверяем наличие файлов _pop и _indv
                bool isPop = File.Exists(popParametersFilePath);
                bool isIndv = File.Exists(indvParametersFilePath);

                CLUMPPConfigurationModel configurationModel = new();

                if (isPop)
                {
                    var parametersLines = await _fileService.ReadAllLinesAsync(popParametersFilePath);
                    DefineParameterModelConverter.PopulateModelFromLines(configurationModel, parametersLines);
                }
                if (!isPop && isIndv)
                {
                    var parametersLines = await _fileService.ReadAllLinesAsync(indvParametersFilePath);
                    DefineParameterModelConverter.PopulateModelFromLines(configurationModel, parametersLines);
                }

                int kMin = int.MaxValue, kMax = 0;
                bool found = false;
                // Ищем K-файлы в Results
                var resultsPath = Path.Combine(fullConfigurationFolderPath, CLUMPP_RESULTS_FOLDER_NAME);
                if (_directoryService.IsDirectoryExist(resultsPath) && !_directoryService.IsDirectoryEmpty(resultsPath))
                {
                    var regex = new Regex(@"^K(\d+)\.(popq|indvq)$", RegexOptions.Compiled);

                    foreach (var filePath in Directory.EnumerateFiles(resultsPath))
                    {
                        var match = regex.Match(Path.GetFileName(filePath));
                        if (!match.Success)
                            continue;

                        found = true;
                        int k = int.Parse(match.Groups[1].Value);

                        if (k < kMin) kMin = k;
                        if (k > kMax) kMax = k;
                    }

                    if (!found)
                    {
                        kMax = 0;
                        kMin = 0;
                    }
                }

                configurationModel.HasPopResults = Directory.EnumerateFiles(resultsPath, "*.popq").Any();
                configurationModel.IsProcessed = found;
                configurationModel.ParametersName = configurationName;

                return (configurationModel, isPop, isIndv, kMin, kMax);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool HasPopResults(string fullSetFolderPath, string configurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(configurationName);

            if (!_directoryService.IsDirectoryExist(fullSetFolderPath)) throw new DirectoryNotFoundException($"Set folder \"{fullSetFolderPath}\" was not found.");

            string fullCLUMPPFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullCLUMPPFolderPath)) throw new DirectoryNotFoundException($"CLUMPP folder was not found in set folder: {fullCLUMPPFolderPath}");

            string fullConfigurationFolderPath = Path.Combine(fullCLUMPPFolderPath, configurationName);
            if (!_directoryService.IsDirectoryExist(fullConfigurationFolderPath)) throw new DirectoryNotFoundException($"Set folder \"{fullConfigurationFolderPath}\" was not found.");

            var resultsPath = Path.Combine(fullConfigurationFolderPath, CLUMPP_RESULTS_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(resultsPath)) throw new DirectoryNotFoundException($"Set folder \"{resultsPath}\" was not found.");

            return Directory.EnumerateFiles(resultsPath, "*.popq").Any();
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

            _totalUnits = jobs.Count;
            int completedUnits = 0;

            lock (_unitProgress)
            {
                _unitProgress.Clear();
            }

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
                    return (completed, _totalUnits);
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

                int totalConfigurations = 0;
                int lastRepeat = 0;
                bool inRunningPhase = false;

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
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    _logger.Info($"[K={job.K}] {e.Data}");

                    if (e.Data.StartsWith("The highest value of H", StringComparison.OrdinalIgnoreCase)
                        || e.Data.StartsWith("The highest value of G", StringComparison.OrdinalIgnoreCase))
                    {
                        highestHLine = e.Data;
                    }

                    var line = e.Data.Trim();

                    if (line.StartsWith("In total,") && line.Contains("configurations"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3 && int.TryParse(parts[2], out int parsed))
                        {
                            totalConfigurations = parsed;
                        }

                        lock (_unitProgress)
                        {
                            _unitProgress[(job.K, job.DataType)] = 2.0;
                            ReportTotalProgress();
                        }
                    }
                    else if (line == "Running...")
                    {
                        inRunningPhase = true;
                    }
                    else if (inRunningPhase && line.StartsWith("Results"))
                    {
                        inRunningPhase = false;

                        lock (_unitProgress)
                        {
                            _unitProgress[(job.K, job.DataType)] = 95.0;
                            ReportTotalProgress();
                        }
                    }
                    else if (inRunningPhase && totalConfigurations > 0)
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && int.TryParse(parts[1], out int repeat) && repeat > lastRepeat)
                        {
                            lastRepeat = repeat;
                            double unitValue = 2.0 + (repeat / (double)totalConfigurations) * 88.0;

                            lock (_unitProgress)
                            {
                                _unitProgress[(job.K, job.DataType)] = unitValue;
                                ReportTotalProgress();
                            }
                        }
                    }

                    //if (e.Data.Contains("Press Return", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    process.StandardInput.WriteLine();
                    //}
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    _logger.Error($"[K={job.K}] {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.StandardInput.Close();

                Debug.WriteLine($"Started [K={job.K}] PID={process.Id}");

                await process.WaitForExitAsync(ct);

                Debug.WriteLine($"Finished [K={job.K}] ExitCode={process.ExitCode}");

                if (!string.IsNullOrWhiteSpace(highestHLine))
                {
                    try
                    {
                        var hvFileName = $"K{job.K}_hv.txt";
                        var hvFilePath = Path.Combine(workingDirectory, CLUMPP_RESULTS_FOLDER_NAME, hvFileName);

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

                lock (_unitProgress)
                {
                    _unitProgress[(job.K, job.DataType)] = 100.0;
                    ReportTotalProgress();
                }
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

        public async Task RenameConfiguration(string fullCurrentSetFolderPath, string oldConfigurationName, string newConfigurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(oldConfigurationName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(newConfigurationName);

            var fullOldConfigurationPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME, oldConfigurationName);
            var fullNewConfigurationPath = Path.Combine(fullCurrentSetFolderPath, CLUMPP_FOLDER_NAME, newConfigurationName);

            Directory.Move(fullOldConfigurationPath, fullNewConfigurationPath);

            var fullOldPopParametersFilePath = Path.Combine(fullNewConfigurationPath, $"{oldConfigurationName}{CLUMPP_POP_PARAMFILE_POSTFIX}");
            var fullNewPopParametersFilePath = Path.Combine(fullNewConfigurationPath, $"{newConfigurationName}{CLUMPP_POP_PARAMFILE_POSTFIX}");

            var fullOldIndvParametersFilePath = Path.Combine(fullNewConfigurationPath, $"{oldConfigurationName}{CLUMPP_INDV_PARAMFILE_POSTFIX}");
            var fullNewIndvParametersFilePath = Path.Combine(fullNewConfigurationPath, $"{newConfigurationName}{CLUMPP_INDV_PARAMFILE_POSTFIX}");

            var configurationModel = new CLUMPPConfigurationModel();

            if (File.Exists(fullOldPopParametersFilePath))
            {
                var parametersLines = await _fileService.ReadAllLinesAsync(fullOldPopParametersFilePath);
                DefineParameterModelConverter.PopulateModelFromLines(configurationModel, parametersLines);

                configurationModel.ParametersName = newConfigurationName;

                var newParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationModel).ToList();
                await _fileService.WriteAllLinesAsync(fullNewPopParametersFilePath, newParametersLines);

                _fileService.DeleteFile(fullOldPopParametersFilePath);
            }

            if (File.Exists(fullOldIndvParametersFilePath))
            {
                var parametersLines = await _fileService.ReadAllLinesAsync(fullOldIndvParametersFilePath);
                DefineParameterModelConverter.PopulateModelFromLines(configurationModel, parametersLines);

                configurationModel.ParametersName = newConfigurationName;

                var newParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationModel).ToList();
                await _fileService.WriteAllLinesAsync(fullNewIndvParametersFilePath, newParametersLines);

                _fileService.DeleteFile(fullOldIndvParametersFilePath);
            }
        }

        private void ReportTotalProgress()
        {
            if (_unitProgress.Count == 0 || _totalUnits == 0) return;
            double total = _unitProgress.Values.Sum() / _totalUnits;
            ProgressChanged?.Invoke(total);
        }

        public bool IsRunning
        {
            get { lock (_lock) return _isRunning; }
            private set { lock (_lock) _isRunning = value; }
        }
    }
}
