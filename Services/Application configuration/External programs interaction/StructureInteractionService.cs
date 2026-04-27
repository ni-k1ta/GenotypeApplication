using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

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

        private readonly IFileService _fileService;
        private readonly IDirectoryService _directoryService;

        private CancellationTokenSource? _cts;
        private readonly object _lock = new();
        private bool _isRunning;

        public event Action<double>? ProgressChanged;
        private readonly Dictionary<(int k, int iteration), double> _unitProgress = new();
        int _totalUnits = 1;

        private ProgramLogger _logger;
        public StructureInteractionService(IDirectoryService directoryService, IFileService fileService, ProgramLogger logger)
        {
            _directoryService = directoryService;
            _fileService = fileService;
            _logger = logger;
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

            if (!string.IsNullOrWhiteSpace(savedDataFileFullPath)) _fileService.DeleteFile(Path.Combine(fullStructureFolderPath, Path.GetFileName(savedDataFileFullPath)));

            _fileService.CopyFile(dataFileFullPath, Path.Combine(fullStructureFolderPath, Path.GetFileName(dataFileFullPath)));
        }

        public async Task PrepareParameterFiles(string fullCurrentSetFolderPath, DataFileFormatModel dataFileFormatModel, StructureMainParametersModel structureMainParametersModel, StructureExtraParametersModel structureExtraParametersModel)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullCurrentSetFolderPath);
            ArgumentNullException.ThrowIfNull(dataFileFormatModel);
            ArgumentNullException.ThrowIfNull(structureMainParametersModel);
            ArgumentNullException.ThrowIfNull(structureExtraParametersModel);

            var structureMainParametersLines = DefineParameterModelConverter.GetFormatedLines(dataFileFormatModel, "#define")
            .Concat(DefineParameterModelConverter.GetFormatedLines(structureMainParametersModel, "#define"))
            .ToList();

            var structureExtraParametersLines = (DefineParameterModelConverter.GetFormatedLines(structureExtraParametersModel, "#define")).ToList();

            var fullStructureFolderPath = Path.Combine(fullCurrentSetFolderPath, STRUCTURE_FOLDER_NAME);

            if (!_directoryService.IsDirectoryExist(fullStructureFolderPath))
                throw new DirectoryNotFoundException($"The directory {fullStructureFolderPath} does not exist.");

            var fullStructureParametersFilePath = Path.Combine(fullStructureFolderPath, STRUCTURE_MAINPARAMETERS_FILE_NAME);
            await _fileService.WriteAllLinesAsync(fullStructureParametersFilePath, structureMainParametersLines);

            var fullStructureExtraParametersFilePath = Path.Combine(fullStructureFolderPath, STRUCTURE_EXTRAPARAMETERS_FILE_NAME);
            await _fileService.WriteAllLinesAsync(fullStructureExtraParametersFilePath, structureExtraParametersLines);
        }

        public async Task<(DataFileFormatModel dataFileFormatModel, StructureMainParametersModel mainParametersModel, StructureExtraParametersModel extraParametersModel, string fullInputFilePath, int kStart, int kEnd, int iterations)> LoadConfiguration(string fullSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            var fullStructureFolderPath = Path.Combine(fullSetFolderPath, STRUCTURE_FOLDER_NAME);
            if (!_directoryService.IsDirectoryExist(fullStructureFolderPath))
                throw new DirectoryNotFoundException($"Structure folder \"{fullStructureFolderPath}\" was not found.");

            var fullStructureMainParametersFilePath = Path.Combine(fullStructureFolderPath, STRUCTURE_MAINPARAMETERS_FILE_NAME);
            var fullStructureExtraParametersFilePath = Path.Combine(fullStructureFolderPath, STRUCTURE_EXTRAPARAMETERS_FILE_NAME);
            if (!File.Exists(fullStructureMainParametersFilePath) || !File.Exists(fullStructureExtraParametersFilePath))
                throw new FileNotFoundException($"Structure parameter files was not found by path \"{fullStructureFolderPath}\".");

            try
            {
                var mainParametersLines = await _fileService.ReadAllLinesAsync(fullStructureMainParametersFilePath);
                var extraParametersLines = await _fileService.ReadAllLinesAsync(fullStructureExtraParametersFilePath);

                DataFileFormatModel dataFileFormatModel = new();
                StructureMainParametersModel structureMainParametersModel = new();
                StructureExtraParametersModel structureExtraParametersModel = new();

                DefineParameterModelConverter.PopulateModelFromLines(dataFileFormatModel, mainParametersLines, "#define");
                DefineParameterModelConverter.PopulateModelFromLines(structureMainParametersModel, mainParametersLines, "#define");
                DefineParameterModelConverter.PopulateModelFromLines(structureExtraParametersModel, extraParametersLines, "#define");

                var inputFileName = Directory.GetFiles(fullStructureFolderPath).Select(Path.GetFileName).FirstOrDefault(name => string.Equals(name, structureMainParametersModel.INFILE, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(inputFileName))
                    throw new FileNotFoundException($"Input data file was not found in structure folder \"{fullStructureFolderPath}\".");

                var fullInputFilePath = Path.Combine(fullStructureFolderPath, inputFileName);

                int iterations = 0, kStart = int.MaxValue, kEnd = 0;
                bool found = false;

                var fullStructureResultsFolderPath = Path.Combine(fullStructureFolderPath, STRUCTURE_RESULTS_FOLDER_NAME);

                if (_directoryService.IsDirectoryExist(fullStructureResultsFolderPath) && !_directoryService.IsDirectoryEmpty(fullStructureResultsFolderPath))
                {
                    var regex = new Regex(@"^outfile_K(\d+)-i(\d+)_f$", RegexOptions.Compiled);

                    foreach (var filePath in Directory.EnumerateFiles(fullStructureResultsFolderPath))
                    {
                        var match = regex.Match(Path.GetFileName(filePath));
                        if (!match.Success)
                            continue;

                        found = true;
                        int k = int.Parse(match.Groups[1].Value);
                        int i = int.Parse(match.Groups[2].Value);

                        if (i > iterations) iterations = i;
                        if (k < kStart) kStart = k;
                        if (k > kEnd) kEnd = k;
                    }

                    if (!found)
                    {
                        iterations = 0;
                        kStart = 0;
                        kEnd = 0;
                    }
                }

                return (dataFileFormatModel, structureMainParametersModel, structureExtraParametersModel, fullInputFilePath, kStart, kEnd, iterations);
            }
            catch (Exception) { throw; }
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
            if (!File.Exists(fullStructureExecutableFilePath)) throw new FileNotFoundException("Structure executable file not found.");

            var fullStructureOutFilePath = Path.Combine(fullResultsFolderPath, STRUCTURE_OUTPUT_FILE_DEFAULT_NAME);

            var units = new List<(int k, int iteration)>();
            for (int k = kFrom; k <= kTo; k++)
                for (int i = 1; i <= iterations; i++)
                    units.Add((k, i));

            _totalUnits = units.Count;
            int completedUnits = 0;

            lock (_unitProgress)
            {
                _unitProgress.Clear();
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(App.GlobalCts.Token);
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
                   return (completed, _totalUnits);
               }));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception) { throw; }
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

                var arguments = $"-K {k} -o \"{outFilePath}K{k}-i{iteration}\"";

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

                    _logger.Info($"[K={k}, i={iteration}] {e.Data}");

                    double unitValue = e.Data switch
                    {
                        var s when s.Contains("Finished initialization") => 33.0,
                        var s when s.Contains("Final results printed to file") => 90.0,
                        _ => -1
                    };

                    if (unitValue >= 0)
                    {
                        lock (_unitProgress)
                        {
                            _unitProgress[(k, iteration)] = unitValue;
                            ReportTotalProgress();
                        }
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    _logger.Error($"[K={k}, i={iteration}] {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(ct);

                if (process.ExitCode != 0)
                {
                    _cts?.Cancel();
                    _logger.Error($"Error on [K={k}, i={iteration}] step. ExitCode={process.ExitCode}. Execution has been stopped.");
                }

                var (completed, total) = reportProgress();

                lock (_unitProgress)
                {
                    _unitProgress[(k, iteration)] = 100.0;
                    ReportTotalProgress();
                }
            }
            catch (OperationCanceledException) when (process is { HasExited: false })
            {
                try { process.Kill(entireProcessTree: true); }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to kill Structure process: K={k}, Iteration={iteration} {ex.Message}");
                }

                throw;
            }
            finally
            {
                process?.Dispose();
                semaphore.Release();
            }
        }

        private void ReportTotalProgress()
        {
            if (_unitProgress.Count == 0 || _totalUnits == 0) return;
            double total = _unitProgress.Values.Sum() / _totalUnits;
            ProgressChanged?.Invoke(total);
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
