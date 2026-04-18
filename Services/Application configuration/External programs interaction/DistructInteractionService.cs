using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models;
using GenotypeApplication.Models.Distruct;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using OxyPlot;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using static GenotypeApplication.Constants.DistructConstants;

namespace GenotypeApplication.Services.Application_configuration.External_program_interaction
{
    public class DistructInteractionService
    {
        private readonly string DISTRUCT_EXECUTABLE_FILE_NAME = DistructConstants.DISTRUCT_EXECUTABLE_FILE_NAME;
        private readonly string DISTRUCT_FOLDER_NAME = DistructConstants.DISTRUCT_FOLDER_NAME;
        private readonly string DISTRUCT_RESULTS_FOLDER_NAME = DistructConstants.DISTRUCT_RESULTS_FOLDER_NAME;
        private readonly string DISTRUCT_OPTIONAL_FOLDER_NAME = DistructConstants.DISTRUCT_OPTIONAL_FOLDER_NAME;
        private readonly string DISTRUCT_CLUST_PERM_FOLDER = DistructConstants.DISTRUCT_CLUST_PERM_FOLDER;

        private readonly string EXTERNAL_PROGRAMS_FOLDER_PATH = PathConstants.EXTERNAL_PROGRAMS_DEFAULT_FOLDER_PATH;

        private readonly string CLUMPP_FOLDER_NAME = CLUMPPConstants.CLUMPP_FOLDER_NAME;
        private readonly string CLUMPP_RESULTS_FOLDER_NAME = CLUMPPConstants.CLUMPP_RESULTS_FOLDER_NAME;

        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        private CancellationTokenSource? _cts;
        private readonly object _lock = new();
        private bool _isRunning;

        public event Action<double>? ProgressChanged;
        private readonly Dictionary<int, double> _unitProgress = new();
        private int _totalUnits = 1;

        ProgramLogger _logger;

        public DistructInteractionService(IDirectoryService directoryService, IFileService fileService, ProgramLogger logger)
        {
            _directoryService = directoryService;
            _fileService = fileService;
            _logger = logger;
        }

        public void PrepareDistructDirectory(string fullCurrentSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullCLUMPPFolderPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME);
            Directory.CreateDirectory(fullCLUMPPFolderPath);
        }

        public async Task PrepareConfiguration(string fullCurrentSetFolderPath, string clumppConfigurationName, DistructConfigurationModel configurationModel)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(clumppConfigurationName);
            ArgumentNullException.ThrowIfNull(configurationModel);

            var fullDistructFolderPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullDistructFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullDistructFolderPath} does not exist.");

            var fullConfigurationCLUMPPFolderPath = Path.Combine(fullDistructFolderPath, clumppConfigurationName);

            Directory.CreateDirectory(fullConfigurationCLUMPPFolderPath);
            var fullConfigurationFolderPath = Path.Combine(fullConfigurationCLUMPPFolderPath, configurationModel.ParametersName);
            Directory.CreateDirectory(fullConfigurationFolderPath);

            if (!string.IsNullOrWhiteSpace(configurationModel.INFILE_LABEL_ATOP) ||
                !string.IsNullOrWhiteSpace(configurationModel.INFILE_LABEL_BELOW))
            {
                var fullOptionalFolderPath = Path.Combine(fullConfigurationFolderPath, DISTRUCT_OPTIONAL_FOLDER_NAME);

                Directory.CreateDirectory(fullOptionalFolderPath);

                if (!string.IsNullOrWhiteSpace(configurationModel.INFILE_LABEL_ATOP))
                {
                    var fileName = Path.GetFileName(configurationModel.INFILE_LABEL_ATOP);
                    var targetPath = Path.Combine(fullOptionalFolderPath, fileName);
                    _fileService.CopyFile(configurationModel.INFILE_LABEL_ATOP, targetPath);
                    configurationModel.INFILE_LABEL_ATOP = Path.Combine(DISTRUCT_OPTIONAL_FOLDER_NAME, fileName);
                }

                if (!string.IsNullOrWhiteSpace(configurationModel.INFILE_LABEL_BELOW))
                {
                    var fileName = Path.GetFileName(configurationModel.INFILE_LABEL_BELOW);
                    var targetPath = Path.Combine(fullOptionalFolderPath, fileName);
                    _fileService.CopyFile(configurationModel.INFILE_LABEL_BELOW, targetPath);
                    configurationModel.INFILE_LABEL_BELOW = Path.Combine(DISTRUCT_OPTIONAL_FOLDER_NAME, fileName);
                }
            }

            var fullConfigurationFilePath = Path.Combine(fullConfigurationFolderPath, configurationModel.ParametersName);

            var configurationParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationModel, "#define").ToList();
            await _fileService.WriteAllLinesAsync(fullConfigurationFilePath, configurationParametersLines);
        }

        public async Task<List<DistructConfigurationModel>> LoadConfigurationsListAsync(string fullSetFolderPath, string CLUMPPConfigurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (!_directoryService.IsDirectoryExist(fullSetFolderPath)) throw new DirectoryNotFoundException($"Set folder \"{fullSetFolderPath}\" was not found.");

            var fullDistructFolderPath = Path.Combine(fullSetFolderPath, DISTRUCT_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullDistructFolderPath)) throw new DirectoryNotFoundException($"Distruct folder was not found in the set folder.");

            string fullFolderWithConfigurationsPath = Path.Combine(fullDistructFolderPath, CLUMPPConfigurationName);
            if (!_directoryService.IsDirectoryExist(fullFolderWithConfigurationsPath)) throw new DirectoryNotFoundException();

            try
            {
                List<DistructConfigurationModel> configurations = new();

                foreach (var configurationFolder in Directory.EnumerateDirectories(fullFolderWithConfigurationsPath))
                {
                    var configurationName = Path.GetFileName(configurationFolder);

                    var (configuration, _, _) = await LoadConfigurationAsync(fullSetFolderPath, CLUMPPConfigurationName, configurationName);

                    configurations.Add(configuration);
                }

                return configurations;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<(DistructConfigurationModel configuration, int kFrom, int kTo)> LoadConfigurationAsync(string fullSetFolderPath, string CLUMPPConfigurationName, string configurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            if (!_directoryService.IsDirectoryExist(fullSetFolderPath)) throw new DirectoryNotFoundException($"Set folder \"{fullSetFolderPath}\" was not found.");

            var fullDistructFolderPath = Path.Combine(fullSetFolderPath, DISTRUCT_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullDistructFolderPath)) throw new DirectoryNotFoundException($"Distruct folder was not found in the set folder.");

            string fullFolderWithConfigurationsPath = Path.Combine(fullDistructFolderPath, CLUMPPConfigurationName);

            string fullConfigurationFolderPath = Path.Combine(fullFolderWithConfigurationsPath, configurationName);

            try
            {
                string fullConfigurationFilePath = Path.Combine(fullConfigurationFolderPath, configurationName);

                DistructConfigurationModel configurationModel = new();

                var parameterLines = await _fileService.ReadAllLinesAsync(fullConfigurationFilePath);
                DefineParameterModelConverter.PopulateModelFromLines(configurationModel, parameterLines, "#define");

                int kMin = int.MaxValue, kMax = 0;
                bool found = false;
                // Ищем K-файлы в Results
                var resultsPath = Path.Combine(fullConfigurationFolderPath, DISTRUCT_RESULTS_FOLDER_NAME);
                if (_directoryService.IsDirectoryExist(resultsPath) && !_directoryService.IsDirectoryEmpty(resultsPath))
                {
                    var regex = new Regex(@"^K(\d+)\.ps$", RegexOptions.Compiled);

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

                configurationModel.ParametersName = configurationName;

                return (configurationModel, kMin, kMax);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private record DistructJob(int K, string ParametersFileName, string InputFilePopqPath, string InputFileIndvqPath, string OutFilePath);
        private List<DistructJob> BuildJobs(int kFrom, int kTo, string configurationName, string clumppConfigurationName)
        {
            var jobs = new List<DistructJob>();

            for (int k = kFrom; k <= kTo; k++)
            {
                jobs.Add(new DistructJob(
                    k,
                    configurationName,
                    Path.Combine("..", "..", "..", CLUMPP_FOLDER_NAME, clumppConfigurationName, CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.popq"),
                    Path.Combine("..", "..", "..", CLUMPP_FOLDER_NAME, clumppConfigurationName, CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.indq"),
                    Path.Combine(DISTRUCT_RESULTS_FOLDER_NAME, $"K{k}.ps")
                    ));
            }

            return jobs;
        }

        public async Task StartExecution(string configurationName, int kFrom, int kTo, string fullSetFolderPath, string clumppConfigurationName, ObservableCollection<ClusterColorItem> clusterColors, bool isGrayscale, int coresCount)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(clumppConfigurationName);

            if (kFrom > kTo)
                throw new ArgumentException($"kFrom ({kFrom}) must be <= kTo ({kTo}).");
            if (coresCount <= 0)
                throw new ArgumentException("coresCount must be > 0.");

            var fullCLUMPPResultsFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME, clumppConfigurationName, CLUMPP_RESULTS_FOLDER_NAME);
            bool hasCLUMPPResults = _directoryService.IsDirectoryExist(fullCLUMPPResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullCLUMPPResultsFolderPath);
            if (!hasCLUMPPResults)
                throw new FileNotFoundException("Не найдены результаты работы CLUMPP");

            var fullConfigurationCLUMPPFolderPath = Path.Combine(fullSetFolderPath, DISTRUCT_FOLDER_NAME, clumppConfigurationName);
            if (!_directoryService.IsDirectoryExist(fullConfigurationCLUMPPFolderPath))
                throw new DirectoryNotFoundException(
                    $"The directory {fullConfigurationCLUMPPFolderPath} does not exist.");

            var fullConfigurationFolderPath = Path.Combine(fullConfigurationCLUMPPFolderPath, configurationName);
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
                      clumppConfigurationName);

            int completedUnits = 0;

            _totalUnits = jobs.Count;

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
                fullDistructExecutableFilePath,
                fullConfigurationFolderPath,
                clusterColors,
                isGrayscale,
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
        private async Task RunSingleProcessAsync(DistructJob job, string executablePath, string workingDirectory, ObservableCollection<ClusterColorItem> clusterColors, bool isGrayscale, SemaphoreSlim semaphore, CancellationToken ct, Func<(int completed, int total)> reportProgress)
        {
            await semaphore.WaitAsync(ct);

            Process? process = null;

            try
            {
                ct.ThrowIfCancellationRequested();

                string clustPermFilePath = string.Empty;
                if (clusterColors != null && clusterColors.Count > 0)
                {
                    var clustersColorsFolderPath = Path.Combine(workingDirectory, DISTRUCT_CLUST_PERM_FOLDER);
                    Directory.CreateDirectory(clustersColorsFolderPath);

                    clustPermFilePath = CreateClustPermFile(clusterColors, job.K, isGrayscale, clustersColorsFolderPath);
                }

                var arguments =
                    $"-d {job.ParametersFileName}" +
                    $" -K {job.K}" +
                    $" -p {job.InputFilePopqPath}" +
                    $" -i {job.InputFileIndvqPath}" +
                    $" -o {job.OutFilePath}" +

                    (string.IsNullOrWhiteSpace(clustPermFilePath) ?
                        string.Empty : 
                        $" -c \"{Path.Combine(DISTRUCT_CLUST_PERM_FOLDER, $"clust_k{job.K}.perm")}\"");

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
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    if (e.Data.StartsWith("Warning", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Warning($"[K={job.K}] {e.Data}");
                    }
                    else
                    {
                        _logger.Info($"[K={job.K}] {e.Data}");
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    _logger.Error($"[K={job.K}] {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Debug.WriteLine($"Started [K={job.K}] PID={process.Id}");

                await process.WaitForExitAsync(ct);

                Debug.WriteLine($"Finished [K={job.K}] ExitCode={process.ExitCode}");

                var (completed, total) = reportProgress();

                lock (_unitProgress)
                {
                    _unitProgress[job.K] = 100.0;
                    ReportTotalProgress();
                }

                //_logger.LogInformation(
                //"Process finished: K={K}, Iteration={Iteration}, " +
                //"ExitCode={ExitCode}, Progress={Completed}/{Total}",
                //k, iteration, process.ExitCode, completed, total);
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

        private string CreateClustPermFile(ObservableCollection<ClusterColorItem> clusterColors, int k, bool isGrayscale, string outputFolderPath)
        {
            if (clusterColors == null || clusterColors.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var item in clusterColors.Where(i => i.ClusterIndex <= k))
            {
                if (isGrayscale)
                    sb.AppendLine($"{item.ClusterIndex} {item.GrayscaleValue:F3}");
                else
                    sb.AppendLine($"{item.ClusterIndex} {item.ColorName}");
            }

            var clustPermFilePath = Path.Combine(outputFolderPath, $"clust_k{k}.perm");
            File.WriteAllText(clustPermFilePath, sb.ToString());
            return clustPermFilePath;
        }

        public async Task<BitmapImage> GeneratePreviewForKAsync(string fullSetFolderPath, string clumppConfigurationName, string configurationName, int k, ObservableCollection<ClusterColorItem> clusterColors, bool isGrayscale)
        {
            var fullCLUMPPResultsFolderPath = Path.Combine(fullSetFolderPath, CLUMPP_FOLDER_NAME, clumppConfigurationName, CLUMPP_RESULTS_FOLDER_NAME);
            bool hasCLUMPPResults = _directoryService.IsDirectoryExist(fullCLUMPPResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullCLUMPPResultsFolderPath);
            if (!hasCLUMPPResults)
                throw new FileNotFoundException("Не найдены результаты работы CLUMPP"); //изменить

            var fullConfigurationCLUMPPFolderPath = Path.Combine(fullSetFolderPath, DISTRUCT_FOLDER_NAME, clumppConfigurationName);
            var fullConfigurationFolderPath = Path.Combine(fullConfigurationCLUMPPFolderPath, configurationName);
            var fullResultsFolderPath = Path.Combine(fullConfigurationFolderPath, "tmp");
            Directory.CreateDirectory(fullResultsFolderPath);

            var fullDistructExecutableFilePath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, DISTRUCT_EXECUTABLE_FILE_NAME);

            var job = new DistructJob(
                    k,
                    configurationName,
                    Path.Combine("..", "..", "..", CLUMPP_FOLDER_NAME, clumppConfigurationName, CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.popq"),
                    Path.Combine("..", "..", "..", CLUMPP_FOLDER_NAME, clumppConfigurationName, CLUMPP_RESULTS_FOLDER_NAME, $"K{k}.indq"),
                    Path.Combine("tmp", $"K{k}.ps")
                    );

            Process? process = null;

            try
            {
                string clustPermFilePath = CreateClustPermFile(clusterColors, job.K, isGrayscale, fullResultsFolderPath);

                var arguments =
                    $"-d {job.ParametersFileName}" +
                    $" -K {job.K}" +
                    $" -p {job.InputFilePopqPath}" +
                    $" -i {job.InputFileIndvqPath}" +
                    $" -o {job.OutFilePath}" +

                    (string.IsNullOrWhiteSpace(clustPermFilePath) ?
                        string.Empty :
                        $" -c {Path.Combine("tmp", $"clust_k{job.K}.perm")}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = fullDistructExecutableFilePath,
                    Arguments = arguments,
                    WorkingDirectory = fullConfigurationFolderPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    Debug.WriteLine($"[K={job.K}] {e.Data}");

                    if (e.Data.StartsWith("Warning", StringComparison.OrdinalIgnoreCase))
                    {
                        //_logger.Warning($"[K={job.K}] {e.Data}");
                    }
                    else
                    {
                       // _logger.Info($"[K={job.K}] {e.Data}");
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    Debug.WriteLine($"[K={job.K}] ERROR: {e.Data}");

                    //_logger.Error($"[K={job.K}] {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Debug.WriteLine($"Started [K={job.K}] PID={process.Id}");

                await process.WaitForExitAsync();

                Debug.WriteLine($"Finished [K={job.K}] ExitCode={process.ExitCode}");



                string ghostscriptPath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, "gswin64c.exe");
                if (!File.Exists(ghostscriptPath)) { throw new FileNotFoundException("Ghostscript executable not found.", ghostscriptPath); }

                string inputFilePath = Path.Combine(fullResultsFolderPath, $"K{job.K}.ps");
                string outputFileBaseName = Path.GetFileNameWithoutExtension(inputFilePath);

                var bbox = await GetBoundingBoxAsync(ghostscriptPath, inputFilePath);

                var settings = FormatMap.Get(DistructConstants.OutputFormat.Png);
                var fileName = $"{outputFileBaseName}{settings.Extension}";
                var outputFilePath = Path.Combine(fullResultsFolderPath, fileName);

                var args = new List<string>
                {
                    "-dNOPAUSE",
                    "-dBATCH",
                    "-dSAFER",
                    $"-sDEVICE={settings.Device}",
                    $"-sOutputFile=\"{outputFilePath}\"",
                    $"-dDEVICEWIDTHPOINTS={bbox.Width}",
                    $"-dDEVICEHEIGHTPOINTS={bbox.Height}",
                    "-dFIXEDMEDIA"
                };

                args.Add($"-r{300}");
                args.Add($"\"{inputFilePath}\"");
                var ghostlibArgs = string.Join(" ", args);

                try
                {
                    await RunGhostscriptAsync(ghostscriptPath, ghostlibArgs);
                }
                catch (Exception)
                {

                    throw;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(outputFilePath);
                bitmap.EndInit();
                bitmap.Freeze();  // важно для MVVM!
                return bitmap;
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
                _directoryService.DeleteDirectory(fullResultsFolderPath);
            }
        }

        public async Task RenameConfiguration(string fullCurrentSetFolderPath, string clumppConfigurationName, string oldConfigurationName, string newConfigurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(clumppConfigurationName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(oldConfigurationName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(newConfigurationName);

            var fullOldConfigurationPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME, clumppConfigurationName, oldConfigurationName);
            var fullNewConfigurationPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME, clumppConfigurationName, newConfigurationName);

            if (string.Equals(oldConfigurationName, newConfigurationName, StringComparison.OrdinalIgnoreCase)) { return; }

            if (!Directory.Exists(fullOldConfigurationPath))
            {
                throw new DirectoryNotFoundException($"Исходная конфигурация не найдена: {fullOldConfigurationPath}");
            }

            if (Directory.Exists(fullNewConfigurationPath))
            {
                throw new IOException($"Конфигурация с именем {newConfigurationName} уже существует.");
            }

            Directory.Move(fullOldConfigurationPath, fullNewConfigurationPath);

            var fullOldConfigurationFilePath = Path.Combine(fullNewConfigurationPath, oldConfigurationName);
            var newConfigurationFilePath = Path.Combine(fullNewConfigurationPath, newConfigurationName);

            if (File.Exists(fullOldConfigurationFilePath))
            {
                var configurationModel = new DistructConfigurationModel();

                var parametersLines = await _fileService.ReadAllLinesAsync(fullOldConfigurationFilePath);
                DefineParameterModelConverter.PopulateModelFromLines(configurationModel, parametersLines, "#define");

                configurationModel.ParametersName = newConfigurationName;

                var newParametersLines = DefineParameterModelConverter.GetFormatedLines(configurationModel, "#define").ToList();
                await _fileService.WriteAllLinesAsync(newConfigurationFilePath, newParametersLines);

                _fileService.DeleteFile(fullOldConfigurationFilePath);
            }
        }

        public bool IsConfigurationExist(string fullCurrentSetFolderPath, string clumppConfigurationName, string configurationName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            var fullDistructFolderPath = Path.Combine(fullCurrentSetFolderPath, DISTRUCT_FOLDER_NAME);
            var fullCLUMPPConfigurationFolderPath = Path.Combine(fullDistructFolderPath, clumppConfigurationName);
            var fullConfigurationFolderPath = Path.Combine(fullCLUMPPConfigurationFolderPath, configurationName);
            return _directoryService.IsDirectoryExist(fullConfigurationFolderPath);
        }

        public void StopExecution()
        {
            if (_cts is { IsCancellationRequested: false })
                _cts.Cancel();
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

        public async Task ExportResultsAsync(string fullSetFolderPath, string clumppConfigurationName, string configurationName, DistructConstants.OutputFormat formats, int k, int orientation)
        {
            string ghostscriptPath = Path.Combine(EXTERNAL_PROGRAMS_FOLDER_PATH, "gswin64c.exe");
            if (!File.Exists(ghostscriptPath)) { throw new FileNotFoundException("Ghostscript executable not found.", ghostscriptPath); }

            string fullResultsFolderPath = Path.Combine(fullSetFolderPath, DISTRUCT_FOLDER_NAME, clumppConfigurationName, configurationName, DISTRUCT_RESULTS_FOLDER_NAME);
            string inputFilePath = Path.Combine(fullResultsFolderPath, $"K{k}.ps");
            if (!File.Exists(inputFilePath)) { throw new FileNotFoundException("Input file not found.", inputFilePath); }

            string outputFolderPath = Path.Combine(fullResultsFolderPath, "Exported");
            Directory.CreateDirectory(outputFolderPath);

            var bbox = await GetBoundingBoxAsync(ghostscriptPath, inputFilePath);

            string outputFileBaseName = Path.GetFileNameWithoutExtension(inputFilePath);

            foreach (DistructConstants.OutputFormat format in Enum.GetValues(typeof(DistructConstants.OutputFormat)))
            {
                // пропускаем None и форматы, которые не запрошены
                if (format == OutputFormat.None)
                    continue;

                if (!formats.HasFlag(format))
                    continue;

                var settings = FormatMap.Get(format);
                var fileName = $"{outputFileBaseName}{settings.Extension}";
                var outputFilePath = Path.Combine(outputFolderPath, fileName);

                var args = new List<string>
                {
                    "-dNOPAUSE",
                    "-dBATCH",
                    "-dSAFER",
                    $"-sDEVICE={settings.Device}",
                    $"-sOutputFile=\"{outputFilePath}\"",
                    $"-dDEVICEWIDTHPOINTS={bbox.Width}",
                    $"-dDEVICEHEIGHTPOINTS={bbox.Height}",
                    "-dFIXEDMEDIA"
                };

                if (settings.Device == "pdfwrite") { args.Add("-dAutoRotatePages=/None"); }

                // добавляем разрешение только для растровых форматов
                if (settings.NeedsResolution) { args.Add($"-r{300}"); }
                // входной файл идёт последним
                args.Add($"\"{inputFilePath}\"");
                var arguments = string.Join(" ", args);

                try
                {
                    await RunGhostscriptAsync(ghostscriptPath, arguments);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
        private async Task<(int Width, int Height)> GetBoundingBoxAsync(string ghostscriptPath, string inputFilePath)
        {
            var errorOutput = new StringBuilder();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ghostscriptPath,
                    Arguments = $"-dNOPAUSE -dBATCH -dSAFER -sDEVICE=bbox \"{inputFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (s, e) => tcs.SetResult(process.ExitCode);
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    errorOutput.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();
            await tcs.Task;
            process.Dispose();

            // Парсим HiResBoundingBox для точности
            var output = errorOutput.ToString();
            var match = Regex.Match(output, @"%%HiResBoundingBox:\s+(-?[\d.]+)\s+(-?[\d.]+)\s+(-?[\d.]+)\s+(-?[\d.]+)");

            if (!match.Success)
                throw new Exception($"Could not determine BoundingBox: {output}");

            //double x1 = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            //double y1 = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            double x2 = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            double y2 = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);

            int padding = 2;

            int width = (int)Math.Ceiling(x2) + padding;
            int height = (int)Math.Ceiling(y2) + padding;

            return (width, height);
        }
        private async Task RunGhostscriptAsync(string exeFilePath, string arguments)
        {
            var errorOutput = new StringBuilder();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exeFilePath,
                    Arguments = arguments,
                    UseShellExecute = false,    // не открывать окно
                    CreateNoWindow = true,      // без консольного окна
                    RedirectStandardError = true // ловим ошибки
                },
                EnableRaisingEvents = true  // важно! без этого Exited не сработает
            };

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (s, e) =>
            {
                tcs.SetResult(process.ExitCode);
            };

            // собираем ошибки асинхронно, не блокируя поток
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    errorOutput.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();  // запускаем асинхронное чтение stderr

            var exitCode = await tcs.Task;  // ждём завершения, не блокируя UI

            process.Dispose();

            if (exitCode != 0)
            {
                throw new Exception(
                    $"Ghostscript завершился с ошибкой (код {exitCode}): {errorOutput}");
            }
        }
    }
}
