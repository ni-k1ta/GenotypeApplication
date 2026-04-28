using GenotypeApplication.Application_windows;
using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Models.Project;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using GenotypeApplication.View_models.External_programs_tabs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class StructureTabControlVM : ExternalProgramTabVMBase
    {
        #region Data file parameters
        private string _dataFileName;
        private string _dataFileFullPath;
        private string _savedDataFileFullPath;
        private DataFileFormatModel _dataFileFormatModel;

        private readonly ParametersChangesTracker<DataFileFormatModel> _dataFileFormatChangesTracker = new();

        private bool _isNewDataFile;
        #endregion

        #region Main parameters

        private int _burnInParam;
        private int _numRepsParam;
        private bool _markovPhaseParam;
        private bool _phasedParam;

        #endregion

        #region Extra parameters

        private bool _noAdmixParam;
        private bool _linkageParam;
        private bool _usePopInfoParam;
        private bool _locPriorParam;
        private bool _onefstParam;

        private bool _inferAlphaParam;
        private double _inferAlphaValueParam;
        private bool _popAlphasParam;
        private double _popAlphaValueParam;

        private bool _inferLambdaParam;
        private double _inferLambdaValueParam;
        private bool _popSpecificLambdaParam;
        private double _popSpecificLambdaValueParam;

        private double _fPriorMeanParam;
        private double _fPriorSDParam;
        private bool _unifPriorAlphaParam;
        private double _alphaMaxParam;
        private double _alphaPropSDParam;
        private double _alphaPriorBParam;
        private double _alphaPriorAParam;
        private double _log10rMinParam;
        private double _log10rPropSDParam;
        private double _log10rMaxParam;
        private double _log10rStartParam;
        private int _gensBackParam;
        private double _migrPriorParam;
        private bool _pFromPopFlagOnlyParam;
        private bool _locIsPopParam;
        private double _locPriorInitParam;
        private double _maxLocPriorParam;
        private bool _printNetParam;
        private bool _printLambdaParam;
        private bool _printQsumParam;
        private bool _siteBySiteParam;
        private int _updateFreqParam;
        private bool _printLikesParam;
        private bool _intermedSaveParam;
        private bool _echoDataParam;
        private bool _printQhatParam;
        private bool _ancestDistParam;
        private double _ancestPintParam;
        private int _numBoxesParam;
        private bool _startAtPopInfoParam;
        private int _metroFreqParam;
        private bool _freqsCorrParam;
        private bool _computeProbParam;
        private int _admBurnInParam;
        private bool _randomizeParam;
        private int _seedParam;
        private bool _reportThitRateParam;

        #endregion

        #region Set parameters
        private string _setName;

        private int _kFrom;
        private int _kTo;
        private int _iterations;

        private bool _isCreatingNewSet;
        private bool _wasSaved;

        private SetModel? _selectedComboBoxSet;
        #endregion

        #region Services
        //private readonly SetConfigurationService _setConfigurationService;
        private readonly StructureInteractionService _structureInteractionService;//

        private readonly IWindowService _windowService;//

        private readonly ParametersChangesTracker<StructureMainParametersModel> _mainParametersChangesTracker = new();//-
        private readonly ParametersChangesTracker<StructureExtraParametersModel> _extraParametersChangesTracker = new();//-
        #endregion

        #region Progress parameters
        private double _structureProgress;
        public double StructureProgress
        {
            get => _structureProgress;
            set { SetField(ref _structureProgress, value); }
        }

        private string _structureProgressText = "Not started";
        public string StructureProgressText
        {
            get => _structureProgressText;
            set { SetField(ref _structureProgressText, value); }
        }

        private bool _structureStopped;
        public bool StructureStopped
        {
            get => _structureStopped;
            set { SetField(ref _structureStopped, value); }
        }

        private bool _structureCompleted;

        private bool _isSaving;
        #endregion

        public StructureTabControlVM(WorkflowStateModel workflowStateModel, string fullProjectFolderPath, int coresCount, SetConfigurationService setConfigurationService, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService, IValidator<string> pathValidator, IValidator<string> parameterNameValidator, IWindowService windowService, LoggerService loggerService, IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> kRangeValidator) : base(workflowStateModel, SetProcessingStage.Structure, coresCount, fullProjectFolderPath, setConfigurationService, directoryService, fileService, messageService, dialogService, loggerService, pathValidator, parameterNameValidator, kRangeValidator)
        {
            _dataFileName = string.Empty;

            _dataFileFullPath = string.Empty;
            _savedDataFileFullPath = string.Empty;
            _dataFileFormatModel = new();
            _isNewDataFile = true;

            _setName = string.Empty;

            _kFrom = 1;
            _kTo = 3;
            _iterations = 3;

            workflowStateModel.SetModelsList.CollectionChanged += (_, _) => RebuildComboBoxItems();
            RebuildComboBoxItems();
            SelectedComboBoxSet = SetModelsComboBoxList.LastOrDefault(); // здесь через сеттер устаеовятся _isCreatingNewSet и _wasSaved

            LoadDataFileCommand = new AsyncRelayCommand(execute => LoadDataFile());
            SaveChangesAsyncCommand = new AsyncRelayCommand(execute => SaveChangesAsync(), canExecute => CanSaveChanges());
            StartStructureAsyncCommand = new AsyncRelayCommand(execute => StartStructureAsync(), canExecute => CanStartStructure());
            StopStructureCommand = new RelayCommand(execute => StopStructure(), canExecute => CanStopStructure());

            _structureInteractionService = new StructureInteractionService(directoryService, fileService, _logger);
            _structureInteractionService.ProgressChanged += value =>
            {
                if (StructureStopped || _structureCompleted || value >= 100.0) return;

                UIDispatcherHelper.RunOnUI(() =>
                {
                    if (StructureStopped || _structureCompleted) return;
                    StructureProgress = value;
                    StructureProgressText = $"[{_setName}] In progress... {value:F0}%";
                });
            };

            //workflowStateModel.NewSetCreated += ResetProgress;

            _windowService = windowService;
        }

        protected override void ResetProgress()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                StructureProgress = 0;
                StructureProgressText = $"Not started";
            });
            StructureStopped = false;
            _structureCompleted = false;
        }

        #region Data file properties
        public DataFileFormatModel DataFileFormatModel
        {
            get => _dataFileFormatModel;
            set
            {
                if (SetField(ref _dataFileFormatModel, value))
                {
                    OnPropertyChanged(nameof(LocPriorEnable));
                    OnPropertyChanged(nameof(LocIsPopParamEnable));
                }
            }
        }
        public string DataFileFullPath
        {
            get => _dataFileFullPath;
            set
            {
                if (SetField(ref _dataFileFullPath, value))
                    OnPropertyChanged(nameof(ConfigurationEnable));
            }
        }
        public string DataFileName
        {
            get => _dataFileName;
            set
            {
                if (SetField(ref _dataFileName, value))
                {
                    _parameterNameValidator.Validate(value);
                }
            }
        }
        #endregion

        #region Main parameters properties
        public int BurnInParam //
        {
            get => _burnInParam;
            set { SetField(ref _burnInParam, value); }
        }
        public int NumRepsParam //
        {
            get => _numRepsParam;
            set { SetField(ref _numRepsParam, value); }
        }
        public bool MarkovPhaseParam //
        {
            get => _markovPhaseParam;
            set { SetField(ref _markovPhaseParam, value); }
        }
        public bool PhasedParam //
        {
            get => _phasedParam;
            set
            {
                if (LinkageParam && DataFileFormatModel.Ploidy > 2 && !value)
                {
                    _messageService.ShowWarning("When the linkage model is used with polyploids, PHASED=true is required.");
                    return;
                }

                SetField(ref _phasedParam, value);
            }
        }
        #endregion

        #region Extra parameters properties
        public bool NoAdmixParam //
        {
            get => _noAdmixParam;
            set
            {
                if (SetField(ref _noAdmixParam, value))
                {
                    OnPropertyChanged(nameof(AdmixParamEnable));
                    if (value && LinkageParam)
                        LinkageParam = false;
                }
            }
        }
        public bool LinkageParam
        {
            get => _linkageParam;
            set
            {
                if (!SetField(ref _linkageParam, value)) return;

                if (value && DataFileFormatModel.Ploidy > 2) PhasedParam = true;
                if (!value && DataFileFormatModel.PHASEINFO)
                {
                    _messageService.ShowWarning("PHASEINFO will not be used when the linkage model is turned off.");
                }

                OnPropertyChanged(nameof(UsePopInfoEnable));
                OnPropertyChanged(nameof(LocPriorEnable));
                OnPropertyChanged(nameof(AdmixParamEnable));

                if (value && NoAdmixParam)
                    NoAdmixParam = false;
            }
        }
        public bool UsePopInfoParam
        {
            get => _usePopInfoParam;
            set
            {
                if (SetField(ref _usePopInfoParam, value))
                {
                    OnPropertyChanged(nameof(UsePopInfoEnable));
                }
            }
        }
        public bool LocPriorParam
        {
            get => _locPriorParam;
            set
            {
                if (SetField(ref _locPriorParam, value))
                {
                    if (value)
                    {
                        if (DataFileFormatModel.LocData && !DataFileFormatModel.PopData)
                        {
                            LocIsPopParam = false;
                        }
                        else if (!DataFileFormatModel.LocData && DataFileFormatModel.PopData)
                        {
                            LocIsPopParam = true;
                        }

                    }
                }
            }
        }

        public bool OnefstParam //
        {
            get => _onefstParam;
            set { SetField(ref _onefstParam, value); }
        }

        public bool InferAlphaParam //
        {
            get => _inferAlphaParam;
            set
            {
                if (SetField(ref _inferAlphaParam, value))
                    OnPropertyChanged(nameof(NotInferAlphaParam));
            }
        }
        public bool NotInferAlphaParam
        {
            get => !InferAlphaParam;
            set
            {
                if (value)
                    InferAlphaParam = false;
            }
        }
        public double InferAlphaValueParam //
        {
            get => _inferAlphaValueParam;
            set { SetField(ref _inferAlphaValueParam, value); }
        }
        public bool PopAlphasParam //
        {
            get => _popAlphasParam;
            set { SetField(ref _popAlphasParam, value); }
        }
        public double PopAlphaValueParam //
        {
            get => _popAlphaValueParam;
            set { SetField(ref _popAlphaValueParam, value); }
        }

        public bool InferLambdaParam //
        {
            get => _inferLambdaParam;
            set
            {
                if (SetField(ref _inferLambdaParam, value))
                    OnPropertyChanged(nameof(NotInferLambdaParam));
            }
        }
        public bool NotInferLambdaParam
        {
            get => !InferLambdaParam;
            set
            {
                if (value)
                    InferLambdaParam = false;
            }
        }
        public double InferLambdaValueParam //
        {
            get => _inferLambdaValueParam;
            set { SetField(ref _inferLambdaValueParam, value); }
        }
        public bool PopSpecificLambdaParam //
        {
            get => _popSpecificLambdaParam;
            set { SetField(ref _popSpecificLambdaParam, value); }
        }
        public double PopSpecificLambdaValueParam //
        {
            get => _popSpecificLambdaValueParam;
            set { SetField(ref _popSpecificLambdaValueParam, value); }
        }
        public double FPriorMeanParam //
        {
            get => _fPriorMeanParam;
            set { SetField(ref _fPriorMeanParam, value); }
        }
        public double FPriorSDParam //
        {
            get => _fPriorSDParam;
            set { SetField(ref _fPriorSDParam, value); }
        }
        public bool UnifPriorAlphaParam //
        {
            get => _unifPriorAlphaParam;
            set { SetField(ref _unifPriorAlphaParam, value); }
        }
        public double AlphaMaxParam //
        {
            get => _alphaMaxParam;
            set { SetField(ref _alphaMaxParam, value); }
        }
        public double AlphaPropSDParam //
        {
            get => _alphaPropSDParam;
            set { SetField(ref _alphaPropSDParam, value); }
        }
        public double AlphaPriorBParam //
        {
            get => _alphaPriorBParam;
            set { SetField(ref _alphaPriorBParam, value); }
        }
        public double AlphaPriorAParam //
        {
            get => _alphaPriorAParam;
            set { SetField(ref _alphaPriorAParam, value); }
        }
        public double Log10rMinParam //
        {
            get => _log10rMinParam;
            set { SetField(ref _log10rMinParam, value); }
        }
        public double Log10rPropSDParam //
        {
            get => _log10rPropSDParam;
            set { SetField(ref _log10rPropSDParam, value); }
        }
        public double Log10rMaxParam //
        {
            get => _log10rMaxParam;
            set { SetField(ref _log10rMaxParam, value); }
        }
        public double Log10rStartParam //
        {
            get => _log10rStartParam;
            set { SetField(ref _log10rStartParam, value); }
        }
        public int GensBackParam //
        {
            get => _gensBackParam;
            set { SetField(ref _gensBackParam, value); }
        }
        public double MigrPriorParam //
        {
            get => _migrPriorParam;
            set { SetField(ref _migrPriorParam, value); }
        }
        public bool PFromPopFlagOnlyParam
        {
            get => _pFromPopFlagOnlyParam;
            set { SetField(ref _pFromPopFlagOnlyParam, value); }
        }
        public bool LocIsPopParam //
        {
            get => _locIsPopParam;
            set { SetField(ref _locIsPopParam, value); }
        }
        public double LocPriorInitParam //
        {
            get => _locPriorInitParam;
            set { SetField(ref _locPriorInitParam, value); }
        }
        public double MaxLocPriorParam //
        {
            get => _maxLocPriorParam;
            set { SetField(ref _maxLocPriorParam, value); }
        }
        public bool PrintNetParam //
        {
            get => _printNetParam;
            set { SetField(ref _printNetParam, value); }
        }
        public bool PrintLambdaParam //
        {
            get => _printLambdaParam;
            set { SetField(ref _printLambdaParam, value); }
        }
        public bool PrintQsumParam //
        {
            get => _printQsumParam;
            set { SetField(ref _printQsumParam, value); }
        }
        public bool SiteBySiteParam //
        {
            get => _siteBySiteParam;
            set { SetField(ref _siteBySiteParam, value); }
        }
        public int UpdateFreqParam //
        {
            get => _updateFreqParam;
            set { SetField(ref _updateFreqParam, value); }
        }
        public bool PrintLikesParam //
        {
            get => _printLikesParam;
            set { SetField(ref _printLikesParam, value); }
        }
        public bool IntermedSaveParam //
        {
            get => _intermedSaveParam;
            set { SetField(ref _intermedSaveParam, value); }
        }
        public bool EchoDataParam //
        {
            get => _echoDataParam;
            set { SetField(ref _echoDataParam, value); }
        }
        public bool PrintQhatParam //
        {
            get => _printQhatParam;
            set { SetField(ref _printQhatParam, value); }
        }
        public bool AncestDistParam
        {
            get => _ancestDistParam;
            set { SetField(ref _ancestDistParam, value); }
        }
        public double AncestPintParam //
        {
            get => _ancestPintParam;
            set { SetField(ref _ancestPintParam, value); }
        }
        public int NumBoxesParam //
        {
            get => _numBoxesParam;
            set { SetField(ref _numBoxesParam, value); }
        }
        public bool StartAtPopInfoParam
        {
            get => _startAtPopInfoParam;
            set { SetField(ref _startAtPopInfoParam, value); }
        }
        public int MetroFreqParam //
        {
            get => _metroFreqParam;
            set { SetField(ref _metroFreqParam, value); }
        }
        public bool FreqsCorrParam //
        {
            get => _freqsCorrParam;
            set { SetField(ref _freqsCorrParam, value); }
        }
        public bool ComputeProbParam //
        {
            get => _computeProbParam;
            set { SetField(ref _computeProbParam, value); }
        }
        public int AdmBurnInParam
        {
            get => _admBurnInParam;
            set
            {
                if (value >= BurnInParam) return;
                SetField(ref _admBurnInParam, value);
            }
        }
        public bool RandomizeParam //
        {
            get => _randomizeParam;
            set { SetField(ref _randomizeParam, value); }
        }
        public int SeedParam //
        {
            get => _seedParam;
            set { SetField(ref _seedParam, value); }
        }
        public bool ReportThitRateParam //
        {
            get => _reportThitRateParam;
            set { SetField(ref _reportThitRateParam, value); }
        }
        #endregion

        #region Set parameters properties
        public string SetName
        {
            get => _setName;
            set
            {
                if (SetField(ref _setName, value))
                    ValidateProperty(value, _parameterNameValidator.Validate);
            }
        }
        public int KFrom
        {
            get => _kFrom;
            set
            {
                if (SetField(ref _kFrom, value))
                    ValidateProperty((value, KTo, 0, int.MaxValue), _kRangeValidator.Validate);
            }
        }
        public int KTo
        {
            get => _kTo;
            set
            {
                if (SetField(ref _kTo, value))
                    ValidateProperty((KFrom, value, 0, int.MaxValue), _kRangeValidator.Validate);
            }
        }
        public int Iterations
        {
            get => _iterations;
            set { SetField(ref _iterations, value); }
        }
        #endregion

        #region Commands properties
        public ICommand LoadDataFileCommand { get; }
        public ICommand SaveChangesAsyncCommand { get; }
        public AsyncRelayCommand StartStructureAsyncCommand { get; }
        public RelayCommand StopStructureCommand { get; }
        #endregion


        #region Enable properties
        public bool UsePopInfoEnable => !LinkageParam && UsePopInfoParam;
        public bool LocIsPopParamEnable => DataFileFormatModel.PopData && DataFileFormatModel.LocData;
        public bool LocPriorEnable => (DataFileFormatModel.PopData || DataFileFormatModel.LocData) && !LinkageParam;
        public bool AdmixParamEnable => !LinkageParam && !NoAdmixParam;

        public bool ConfigurationEnable => !string.IsNullOrWhiteSpace(DataFileFullPath);
        #endregion


        private static readonly SetModel _createNewSetPlaceholder = new() { Name = "Create new set" };
        public ObservableCollection<SetModel> SetModelsComboBoxList { get; } = new();

        public SetModel? SelectedComboBoxSet
        {
            get => _selectedComboBoxSet;
            set
            {
                if (IsRunning) { return; }
                if (SetField(ref _selectedComboBoxSet, value))
                {
                    if (value == _createNewSetPlaceholder)
                    {
                        if (!_isCreatingNewSet) ResetSetParameters();

                        _isCreatingNewSet = true;
                        SetName = string.Empty;
                        _wasSaved = false;

                        _currentSet = null;
                    }
                    else if (value != null)
                    {
                        CurrentSet = value;
                        SetName = value.Name;
                    }
                }
            }
        }

        private void RebuildComboBoxItems()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                SetModelsComboBoxList.Clear();

                foreach (SetModel item in FilteredSetModelsList)
                    SetModelsComboBoxList.Add(item);

                SetModelsComboBoxList.Add(_createNewSetPlaceholder);
            });
        }
        private void ResetSetParameters()
        {
            var newStructureMainParameters = new StructureMainParametersModel();
            var newStructureExtraParameters = new StructureExtraParametersModel();

            SetName = string.Empty;
            SetMainParameters(newStructureMainParameters, DataFileName);
            SetExtraParameters(newStructureExtraParameters);

            KFrom = 1;
            KTo = 3;
            Iterations = 3;
        }

        protected override async Task LoadSelectedSetParametersAsync(SetModel? set)
        {
            if (set == null || _isSaving) return;

            if (!set.IsStructureProcessed) ResetProgress();

            var setName = set.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var (dataFileFormatModel, strctureMainParametersModel, structureExtraParametersModel, fullInputFilePath, kStart, kEnd, iterations) = await _structureInteractionService.LoadConfiguration(fullSetFolderPath);

                SetField(ref _selectedComboBoxSet, set, nameof(SelectedComboBoxSet));

                DataFileFormatModel = dataFileFormatModel;
                DataFileFullPath = fullInputFilePath;
                _savedDataFileFullPath = DataFileFullPath;
                DataFileName = Path.GetFileName(fullInputFilePath);
                SetMainParameters(strctureMainParametersModel);
                SetExtraParameters(structureExtraParametersModel);

                SetName = setName;
                _dataFileFormatChangesTracker.TakeModelSnapshot(dataFileFormatModel);
                _mainParametersChangesTracker.TakeModelSnapshot(strctureMainParametersModel);
                _extraParametersChangesTracker.TakeModelSnapshot(structureExtraParametersModel);
                _isCreatingNewSet = false;
                _isNewDataFile = false;
                _wasSaved = true;

                if (kEnd == 0 || kStart == 0 || iterations == 0)
                {
                    KFrom = 1;
                    KTo = 3;
                    Iterations = 3;

                    UIDispatcherHelper.RunOnUI(() =>
                    {
                        StructureProgressText = $"[{_setName}] Not started";
                        StructureProgress = 0;
                    });
                    StructureStopped = false;
                    _structureCompleted = false;
                    return;
                }

                KFrom = kStart;
                KTo = kEnd;
                Iterations = iterations;

                UIDispatcherHelper.RunOnUI(() =>
                {
                    StructureProgress = 100;
                    StructureProgressText = $"[{_setName}] Completed";
                });
                StructureStopped = false;
                _structureCompleted = true;

                WorkflowState.SetPredefinedStructureParameters(iterations, kStart, kEnd, dataFileFormatModel.NumInds);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                _messageService.ShowError($"Failed to load Structure parameters. {dnfe.Message}");
                _wasSaved = false;
            }
            catch (FileNotFoundException fnfe)
            {
                _messageService.ShowError($"Failed to load Structure parameters. {fnfe.Message}");
                _wasSaved = false;
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Failed to load Structure parameters. {ex.Message}");
                _wasSaved = false;
            }
        }
        private async Task LoadDataFile()
        {
            DataFileFormatVM dataFileParametersViewModel = new(_dialogService, _messageService, _pathValidator, _windowService);

            try
            {
                if (!string.IsNullOrWhiteSpace(DataFileFullPath) && !string.IsNullOrWhiteSpace(DataFileName) && DataFileFormatModel != null)
                    await dataFileParametersViewModel.LoadDataFileFormat(DataFileFormatModel, DataFileFullPath);

                bool? loadResult = _windowService.ShowDialogWindow<LoadDataFileWindow, DataFileFormatVM>(dataFileParametersViewModel);

                if (loadResult == true)
                {
                    DataFileFullPath = dataFileParametersViewModel.DataFileFullPath;
                    DataFileFormatModel = dataFileParametersViewModel.DataFileFormatModel;

                    DataFileName = Path.GetFileName(DataFileFullPath);

                    if (DataFileFullPath != _savedDataFileFullPath)
                        _isNewDataFile = true;
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Failed to load data file. {ex.Message}");
            }
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                var newSetName = SetName;
                if (string.IsNullOrWhiteSpace(newSetName)) return;

                var dataFileFullPath = DataFileFullPath;

                if (string.IsNullOrWhiteSpace(dataFileFullPath)) return;
                var dataFileFormatModel = DataFileFormatModel;

                string fullNewSetFolderPath = Path.Combine(_fullProjectFolderPath, newSetName);

                bool isNameChanged = !_isCreatingNewSet && CurrentSet?.Name != newSetName;
                if ((_isCreatingNewSet || isNameChanged) && _setConfigurationService.IsSetExist(fullNewSetFolderPath))
                {
                    _messageService.ShowWarning($"Set with name \"{newSetName}\" already exists. Please choose a different name.");
                    return;
                }

                _wasSaved = false;

                var newMainParameters = GetMainParameters();
                var newExtraParameters = GetExtraParameters();

                _isSaving = true;

                if (_isCreatingNewSet)
                {
                    await CreateNewSetAsync(newSetName, fullNewSetFolderPath, dataFileFullPath, dataFileFormatModel, newMainParameters, newExtraParameters);
                }
                else
                {
                    bool shouldBeSaved = await UpdateExistingSetAsync(newSetName, fullNewSetFolderPath, dataFileFullPath, dataFileFormatModel, newMainParameters, newExtraParameters);

                    if (!shouldBeSaved) return;
                }

                _wasSaved = true;
                _isNewDataFile = false;
                _mainParametersChangesTracker.TakeModelSnapshot(newMainParameters);
                _extraParametersChangesTracker.TakeModelSnapshot(newExtraParameters);
                _dataFileFormatChangesTracker.TakeModelSnapshot(dataFileFormatModel);
                _savedDataFileFullPath = dataFileFullPath;
                _isCreatingNewSet = false;
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Failed to save changes. {ex.Message}");
            }
            finally
            {
                _isSaving = false;
                StartStructureAsyncCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanSaveChanges()
        {
            var mainParameters = GetMainParameters();
            var extraParameters = GetExtraParameters();

            return !string.IsNullOrWhiteSpace(SetName) &&
                   !string.IsNullOrWhiteSpace(DataFileFullPath) &&

                   (_mainParametersChangesTracker.HasChanges(mainParameters) ||
                   _extraParametersChangesTracker.HasChanges(extraParameters) ||
                   _dataFileFormatChangesTracker.HasChanges(DataFileFormatModel) ||
                   CurrentSet?.Name != SetName ||
                   _isNewDataFile) &&

                   !_structureInteractionService.IsRunning &&
                   !HasErrors;
        }

        private async Task CreateNewSetAsync(string newSetName, string fullNewSetFolderPath, string dataFileFullPath, DataFileFormatModel dataFileFormatModel, StructureMainParametersModel newMainParameters, StructureExtraParametersModel newExtraParameters)
        {
            try
            {
                _setConfigurationService.Create(fullNewSetFolderPath);

                var newSet = WorkflowState.CreateNewSet(newSetName);

                await _setConfigurationService.SaveConfigFileAsync(fullNewSetFolderPath, newSet);

                _structureInteractionService.PrepareStructureDirectory(fullNewSetFolderPath);

                _structureInteractionService.PrepareInputDataFile(fullNewSetFolderPath, dataFileFullPath);

                await _structureInteractionService.PrepareParameterFiles(fullNewSetFolderPath, dataFileFormatModel, newMainParameters, newExtraParameters);

                CurrentSet = null;

                SetField(ref _selectedComboBoxSet, newSet, nameof(SelectedComboBoxSet));
                WorkflowState.CurrentSet = newSet;

                //WorkflowState.NotifyNewSetCreated();

                _messageService.ShowInformation($"Set \"{newSetName}\" was successfully created.");
            }
            catch (Exception) { throw; }
        }
        private async Task<bool> UpdateExistingSetAsync(string newSetName, string fullNewSetFolderPath, string dataFileFullPath, DataFileFormatModel dataFileFormatModel, StructureMainParametersModel newMainParameters, StructureExtraParametersModel newExtraParameters)
        {
            try
            {
                if (CurrentSet == null) return false;

                string fullSavedSetFolderPath = Path.Combine(_fullProjectFolderPath, CurrentSet.Name);

                if (!_setConfigurationService.IsSetExist(fullSavedSetFolderPath))
                {
                    _messageService.ShowError($"Set with name \"{CurrentSet.Name}\" was not found. Unable to save changes.");
                    return false;
                }

                if (CurrentSet.Name != newSetName)
                {
                    CurrentSet.Name = newSetName;

                    _setConfigurationService.Rename(fullSavedSetFolderPath, fullNewSetFolderPath);

                    await _setConfigurationService.UpdateConfigFileAsync(fullSavedSetFolderPath, fullNewSetFolderPath, CurrentSet);
                }

                bool isParametersChanged = _mainParametersChangesTracker.HasChanges(newMainParameters) ||
                                           _extraParametersChangesTracker.HasChanges(newExtraParameters) ||
                                           _isNewDataFile ||
                                           _dataFileFormatChangesTracker.HasChanges(dataFileFormatModel);

                if (isParametersChanged)
                {
                    bool isProccessedByNextPrograms = CurrentSet.IsStructureHarvesterProcessed || CurrentSet.IsCLUMPPProcessed || CurrentSet.IsDistructProcessed;

                    if (isProccessedByNextPrograms)
                    {
                        var willCreateNewSet = _messageService.ShowQuetion("Changing the parameters will reset the processing progress made in later stages. Do you want to create a new set with the current settings?");

                        if (willCreateNewSet)
                        {
                            SelectedComboBoxSet = _createNewSetPlaceholder;
                        }
                        else
                        {
                            var savedMainParameters = _mainParametersChangesTracker.GetSnapshot();
                            var savedExtraParameters = _extraParametersChangesTracker.GetSnapshot();
                            var savedDataFileFormatModel = _dataFileFormatChangesTracker.GetSnapshot();

                            if (savedMainParameters is null || savedExtraParameters is null || savedDataFileFormatModel is null) throw new InvalidOperationException("The saved data of the current parameter set is missing on UpdateExistingSetAsync() step.");

                            SetMainParameters(savedMainParameters);
                            SetExtraParameters(savedExtraParameters);

                            _isNewDataFile = false;
                            DataFileFullPath = _savedDataFileFullPath;
                            DataFileFormatModel = savedDataFileFormatModel;
                            _wasSaved = true;
                            _isCreatingNewSet = false;
                        }

                        return false;
                    }
                    else
                    {
                        _structureInteractionService.PrepareInputDataFile(fullNewSetFolderPath, dataFileFullPath, _savedDataFileFullPath);

                        await _structureInteractionService.PrepareParameterFiles(fullNewSetFolderPath, dataFileFormatModel, newMainParameters, newExtraParameters);
                    }
                }
                return true;
            }
            catch (Exception) { throw; }
        }

        private async Task StartStructureAsync()
        {
            if (!_wasSaved || CurrentSet == null || HasErrorsFor(nameof(KFrom)) || HasErrorsFor(nameof(KTo))) return;

            int kFrom = KFrom;
            int kTo = KTo;
            int iterations = Iterations;

            var currentSet = CurrentSet;
            var setName = currentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var dataFileFormatModel = _dataFileFormatChangesTracker.GetSnapshot();
                if (dataFileFormatModel is null) return;

                _structureCompleted = false;
                StructureStopped = false;

                StructureProgress = 0;
                StructureProgressText = $"[{setName}] In progress... 0%";

                IsRunning = true;
                WorkflowState.SetPredefinedStructureParameters(iterations, kFrom, kTo, dataFileFormatModel.NumInds);

                await _structureInteractionService.StartExecution(kFrom, kTo, iterations, fullCurrentSetFolderPath, _coresCount);

                _structureCompleted = true;
                WorkflowState.MarkProcessedAndRefreshStage(currentSet, ProcessingStage);
                await _setConfigurationService.SaveConfigFileAsync(fullCurrentSetFolderPath, currentSet);

                StructureProgress = 100;
                StructureProgressText = $"[{setName}] Completed";
            }
            catch (OperationCanceledException)
            {
                StructureProgressText = $"[{setName}] Stopped at {StructureProgress:F0}%";
            }
            catch (Exception ex)
            {
                StructureStopped = true;
                _messageService.ShowError($"An error occurred while running Structure for set {setName}. {ex.Message}. See logs for details.");
                StructureProgressText = $"[{setName}] Stopped by error at {StructureProgress:F0}%";
            }
            finally
            {
                StopStructureCommand.NotifyCanExecuteChanged();
                IsRunning = false;
            }
        }
        private bool CanStartStructure()
        {
            return _wasSaved &&
                   CurrentSet != null &&
                   !_structureInteractionService.IsRunning &&
                   !HasErrorsFor(nameof(KFrom)) &&
                   !HasErrorsFor(nameof(KTo)) &&
                   KFrom > 0 &&
                   KTo > 0 &&
                   Iterations > 0;
        }

        private void StopStructure()
        {
            StructureStopped = true;
            StructureProgressText = $"[{_setName}] Stopping...";
            _structureInteractionService.StopExecution();
        }
        private bool CanStopStructure()
        {
            return _structureInteractionService.IsRunning && !StructureStopped;
        }

        private StructureMainParametersModel GetMainParameters()
        {
            return new StructureMainParametersModel
            {
                INFILE = DataFileName,
                //OUTFILE = "outfile_" - константа
                BURNIN = BurnInParam,
                NumReps = NumRepsParam,
                MARKOVPHASE = MarkovPhaseParam,
                PHASED = PhasedParam
            };
        }
        private StructureExtraParametersModel GetExtraParameters()
        {
            var alphaValue = InferAlphaParam ? InferAlphaValueParam : PopAlphaValueParam;
            var lambdaValue = InferLambdaParam ? InferLambdaValueParam : PopSpecificLambdaValueParam;

            return new StructureExtraParametersModel
            {
                NOADMIX = NoAdmixParam,
                LINKAGE = LinkageParam,
                USEPOPINFO = UsePopInfoParam,
                LOCPRIOR = LocPriorParam,
                ONEFST = OnefstParam,
                INFERALPHA = InferAlphaParam,
                POPALPHAS = PopAlphasParam,
                ALPHA = alphaValue,
                INFERLAMBDA = InferLambdaParam,
                POPSPECIFICLAMBDA = PopSpecificLambdaParam,
                LAMBDA = lambdaValue,
                FPRIORMEAN = FPriorMeanParam,
                FPRIORSD = FPriorSDParam,
                UNIFPRIORALPHA = UnifPriorAlphaParam,
                ALPHAMAX = AlphaMaxParam,
                ALPHAPROPSD = AlphaPropSDParam,
                ALPHAPRIORB = AlphaPriorBParam,
                ALPHAPRIORA = AlphaPriorAParam,
                LOG10RMIN = Log10rMinParam,
                LOG10RPROPSD = Log10rPropSDParam,
                LOG10RMAX = Log10rMaxParam,
                LOG10RSTART = Log10rStartParam,
                GENSBACK = GensBackParam,
                MIGRPRIOR = MigrPriorParam,
                PFROMPOPFLAGONLY = PFromPopFlagOnlyParam,
                LOCISPOP = LocIsPopParam,
                LOCPRIORINIT = LocPriorInitParam,
                MAXLOCPRIOR = MaxLocPriorParam,
                PRINTNET = PrintNetParam,
                PRINTLAMBDA = PrintLambdaParam,
                PRINTQSUM = PrintQsumParam,
                SITEBYSITE = SiteBySiteParam,
                UPDATEFREQ = UpdateFreqParam,
                PRINTLIKES = PrintLikesParam,
                INTERMEDSAVE = IntermedSaveParam,
                ECHODATA = EchoDataParam,
                PRINTQHAT = PrintQhatParam,
                ANCESTDIST = AncestDistParam,
                ANCESTPINT = AncestPintParam,
                NUMBOXES = NumBoxesParam,
                STARTATPOPINFO = StartAtPopInfoParam,
                METROFREQ = MetroFreqParam,
                FREQSCORR = FreqsCorrParam,
                COMPUTEPROB = ComputeProbParam,
                ADMBURNIN = AdmBurnInParam,
                RANDOMIZE = RandomizeParam,
                SEED = SeedParam,
                REPORTHITRATE = ReportThitRateParam
            };
        }

        private void SetMainParameters(StructureMainParametersModel mainParameters, string? dataFileName = null)
        {
            DataFileName = string.IsNullOrWhiteSpace(dataFileName) ? mainParameters.INFILE : dataFileName;
            //OUTFILE = "outfile_" - константа
            BurnInParam = mainParameters.BURNIN;
            NumRepsParam = mainParameters.NumReps;
            MarkovPhaseParam = mainParameters.MARKOVPHASE;
            PhasedParam = mainParameters.PHASED;
        }
        private void SetExtraParameters(StructureExtraParametersModel extraParameters)
        {
            NoAdmixParam = extraParameters.NOADMIX;
            LinkageParam = extraParameters.LINKAGE;
            UsePopInfoParam = extraParameters.USEPOPINFO;
            LocPriorParam = extraParameters.LOCPRIOR;
            OnefstParam = extraParameters.ONEFST;

            InferAlphaParam = extraParameters.INFERALPHA;
            PopAlphasParam = extraParameters.POPALPHAS;
            if (InferAlphaParam) InferAlphaValueParam = extraParameters.ALPHA;
            else PopAlphaValueParam = extraParameters.ALPHA;

            InferLambdaParam = extraParameters.INFERLAMBDA;
            PopSpecificLambdaParam = extraParameters.POPSPECIFICLAMBDA;
            if (InferLambdaParam) InferLambdaValueParam = extraParameters.LAMBDA;
            else PopSpecificLambdaValueParam = extraParameters.LAMBDA;

            FPriorMeanParam = extraParameters.FPRIORMEAN;
            FPriorSDParam = extraParameters.FPRIORSD;
            UnifPriorAlphaParam = extraParameters.UNIFPRIORALPHA;
            AlphaMaxParam = extraParameters.ALPHAMAX;
            AlphaPropSDParam = extraParameters.ALPHAPROPSD;
            AlphaPriorBParam = extraParameters.ALPHAPRIORB;
            AlphaPriorAParam = extraParameters.ALPHAPRIORA;
            Log10rMinParam = extraParameters.LOG10RMIN;
            Log10rPropSDParam = extraParameters.LOG10RPROPSD;
            Log10rMaxParam = extraParameters.LOG10RMAX;
            Log10rStartParam = extraParameters.LOG10RSTART;
            GensBackParam = extraParameters.GENSBACK;
            MigrPriorParam = extraParameters.MIGRPRIOR;
            PFromPopFlagOnlyParam = extraParameters.PFROMPOPFLAGONLY;
            LocIsPopParam = extraParameters.LOCISPOP;
            LocPriorInitParam = extraParameters.LOCPRIORINIT;
            MaxLocPriorParam = extraParameters.MAXLOCPRIOR;
            PrintNetParam = extraParameters.PRINTNET;
            PrintLambdaParam = extraParameters.PRINTLAMBDA;
            PrintQsumParam = extraParameters.PRINTQSUM;
            SiteBySiteParam = extraParameters.SITEBYSITE;
            UpdateFreqParam = extraParameters.UPDATEFREQ;
            PrintLikesParam = extraParameters.PRINTLIKES;
            IntermedSaveParam = extraParameters.INTERMEDSAVE;
            EchoDataParam = extraParameters.ECHODATA;
            PrintQhatParam = extraParameters.PRINTQHAT;
            AncestDistParam = extraParameters.ANCESTDIST;
            AncestPintParam = extraParameters.ANCESTPINT;
            NumBoxesParam = extraParameters.NUMBOXES;
            StartAtPopInfoParam = extraParameters.STARTATPOPINFO;
            MetroFreqParam = extraParameters.METROFREQ;
            FreqsCorrParam = extraParameters.FREQSCORR;
            ComputeProbParam = extraParameters.COMPUTEPROB;
            AdmBurnInParam = extraParameters.ADMBURNIN;
            RandomizeParam = extraParameters.RANDOMIZE;
            SeedParam = extraParameters.SEED;
            ReportThitRateParam = extraParameters.REPORTHITRATE;
        }

        protected override async Task LoadSelectedCLUMPPConfigurationAsync(CLUMPPConfigurationModel? configuration)
        {
            return;
        }
    }
}
