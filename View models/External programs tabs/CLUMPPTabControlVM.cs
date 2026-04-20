using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.Services.Application_configuration.External_programs_interaction;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using GenotypeApplication.View_models.External_programs_tabs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class CLUMPPTabControlVM : ExternalProgramTabVMBase
    {
        private bool _isCreatingNewConfiguration;
        private bool _wasSaved;

        private int _kFrom;
        private int _kTo;

        #region Configuration parameters
        private string _configurationName;

        private bool _isPop;
        private bool _isIndv;

        private int _popsCount;
        private int _indvsCount;

        private int _r;
        private bool _w;
        private bool _s; // 1 or 2
        private int _repeats;
        private string _permutationFile;
        private bool _printEveryPerm;
        private bool _printRandomInputorder;
        private int _orderByRun;

        private readonly Dictionary<int, string> _algorithms = new()
        {
            { 1, "FullSearch" },
            { 2, "Greedy" },
            { 3, "LargeKGreedy" }
        };
        private int _selectedAlgorithm = 1; //m value

        private readonly Dictionary<int, string> _greedyOptions = new()
        {
            { 1, "All input orders" },
            { 2, "Random input orders" },
            { 3, "Predefined order" }
        };
        private int _selectedGreedyOption = 1; //GREEDY_OPTION value

        private readonly Dictionary<int, string> _printPermutedDataOptions = new()
        {
            { 0, "Don't print" },
            { 1, "All in one file" },
            { 2, "One file per Q-matrix" }
        };
        private int _selectedPrintPermutedData = 1; //PRINT_PERMUTED_DATA value

        private CLUMPPConfigurationModel? _selectedComboBoxConfigurationParameters = new();
        #endregion

        private readonly CLUMPPInteractionService _clumppInteractionService;

        private readonly ParametersChangesTracker<CLUMPPConfigurationModel> _changesTracker = new();
        private (bool savedIsPop, int savedCPop, bool savedisIndv, int savedCIndv) _savedDataTypeParameters;

        #region Progress parameters
        private double _clumppProgress;
        public double CLUMPPProgress
        {
            get => _clumppProgress;
            set { SetField(ref _clumppProgress, value); }
        }

        private string _clumppProgressText = "Not started";
        public string CLUMPPProgressText
        {
            get => _clumppProgressText;
            set { SetField(ref _clumppProgressText, value); }
        }

        private bool _clumppStopped;
        public bool CLUMPPStopped
        {
            get => _clumppStopped;
            set { SetField(ref _clumppStopped, value); }
        }

        private bool _clumppCompleted;
        public bool CLUMPPCompleted
        {
            get => _clumppCompleted;
            set { SetField(ref _clumppCompleted, value); }
        }
        #endregion

        private bool _userSure;

        public CLUMPPTabControlVM(WorkflowStateModel workflowState, int coresCount, string fullProjectFolderPath, SetConfigurationService setConfigurationService, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService, IValidator<string> pathValidator, IValidator<string> parameterNameValidator, LoggerService loggerService, IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> kRangeValidator) : base(workflowState, SetProcessingStage.CLUMPP, coresCount, fullProjectFolderPath, setConfigurationService, directoryService, fileService, messageService, dialogService, loggerService, pathValidator, parameterNameValidator, kRangeValidator)
        {
            _configurationName = string.Empty;

            _isPop = false;
            _isIndv = false;

            _popsCount = 0;
            _indvsCount = 0;

            _savedDataTypeParameters = (false, 0, false, 0);

            _kFrom = 2;
            _kTo = 3;

            _permutationFile = string.Empty;
            WorkflowState.CLUMPPConfigurationModelsList.CollectionChanged += (_, _) => RebuildComboBoxItems();
            RebuildComboBoxItems();
            SelectedComboBoxConfigurationParameters = ConfigurationParametersComboBoxList.LastOrDefault(); // здесь через сеттер устаеовятся _isCreatingNewSet и _wasSaved

            PropertyChanged += OnLimitedValuesChanged;

            SaveChangesAsyncCommand = new AsyncRelayCommand(execute => SaveChangesAsync(), canExecute => CanSaveChanges());
            StartCLUMPPAsyncCommand = new AsyncRelayCommand(execute => StartCLUMPPAsync(), canExecute => CanStartCLUMPP());
            StopCLUMPPCommand = new RelayCommand(execute => StopCLUMPP(), canExecute => CanStopCLUMPP());
            SelectPermutationFileCommand = new RelayCommand(execute => SelectPermutationFile());

            _clumppInteractionService = new CLUMPPInteractionService(directoryService, fileService, _logger);
            _clumppInteractionService.ProgressChanged += value =>
            {
                if (_clumppCompleted || CLUMPPStopped) return;
                if (value >= 100) return;

                UIDispatcherHelper.RunOnUI(() =>
                {
                    if (_clumppCompleted || CLUMPPStopped) return;
                    CLUMPPProgress = value;
                    CLUMPPProgressText = $"[{_configurationName}] In progress... {value:F0}%";
                });
            };
        }

        private void OnLimitedValuesChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null) return;

            if (e.PropertyName == nameof(PredefinedIterationsLimit))
                R = PredefinedIterationsLimit;

            if (e.PropertyName == nameof(PredefinedStructureKStart))
                KFrom = PredefinedStructureKStart;

            if (e.PropertyName == nameof(PredefinedStructureKEnd))
                KTo = PredefinedStructureKEnd;

            if (e.PropertyName == nameof(PredefinedIndvCount))
                IndvsCount = PredefinedIndvCount;

            if (e.PropertyName == nameof(PredefinedPopCount))
                PopsCount = PredefinedPopCount;
        }

        public int KFrom
        {
            get => _kFrom;
            set
            {
                if (SetField(ref _kFrom, value))
                    ValidateKRange();
            }
        }
        public int KTo
        {
            get => _kTo;
            set
            {
                if (SetField(ref _kTo, value))
                    ValidateKRange();
            }
        }

        private void ValidateKRange()
        {
            var args = (KFrom, KTo, int.Max(2, PredefinedStructureKStart), PredefinedStructureKEnd);
            ValidateProperty(args, _kRangeValidator.Validate, nameof(KFrom));
            ValidateProperty(args, _kRangeValidator.Validate, nameof(KTo));
        }

        #region Configuration parameters properties
        public bool IsPop
        {
            get => _isPop;
            set { SetField(ref _isPop, value); }
        }
        public bool IsIndv
        {
            get => _isIndv;
            set { SetField(ref _isIndv, value); }
        }
        public int PopsCount
        {
            get => _popsCount;
            set { SetField(ref _popsCount, value); }
        }
        public int IndvsCount
        {
            get => _indvsCount;
            set { SetField(ref _indvsCount, value); }
        }
        public int R//
        {
            get => _r;
            set { SetField(ref _r, value); }
        }
        public bool W//
        {
            get => _w;
            set { SetField(ref _w, value); }
        }
        public bool S//
        {
            get => _s;
            set { SetField(ref _s, value); }
        }
        public int Repeats//
        {
            get => _repeats;
            set { SetField(ref _repeats, value); }
        }
        public string PermutationFile//
        {
            get => _permutationFile;
            set
            { 
                if (SetField(ref _permutationFile, value))
                    _pathValidator.Validate(value);
            }
        }
        public bool PrintEveryPerm//
        {
            get => _printEveryPerm;
            set { SetField(ref _printEveryPerm, value); }
        }
        public bool PrintRandomInputorder//
        {
            get => _printRandomInputorder;
            set { SetField(ref _printRandomInputorder, value); }
        }

        public int OrderByRun//
        {
            get => _orderByRun;
            set { SetField(ref _orderByRun, value); }
        }

        public Dictionary<int, string> Algorithms => _algorithms;//
        public int SelectedAlgorithm//
        {
            get => _selectedAlgorithm;
            set 
            { 
                if (SetField(ref _selectedAlgorithm, value))
                {
                    OnPropertyChanged(nameof(GreedyOptionEnabled));
                    OnPropertyChanged(nameof(RepeatsEnabled));
                    OnPropertyChanged(nameof(PermutationFileEnabled));
                }
            }
        }

        public Dictionary<int, string> GreedyOptions => _greedyOptions;//
        public int SelectedGreedyOption//
        {
            get => _selectedGreedyOption;
            set 
            { 
                if (SetField(ref _selectedGreedyOption, value))
                {
                    OnPropertyChanged(nameof(RepeatsEnabled));
                    OnPropertyChanged(nameof(PermutationFileEnabled));
                }
            }
        }

        public Dictionary<int, string> PrintPermutedDataOptions => _printPermutedDataOptions;//
        public int SelectedPrintPermutedData//
        {
            get => _selectedPrintPermutedData;
            set { SetField(ref _selectedPrintPermutedData, value); }
        }

        public string ConfigurationName
        {
            get => _configurationName;
            set 
            {
                if (SetField(ref _configurationName, value))
                    _parameterNameValidator.Validate(value);
            }
        }
        public CLUMPPConfigurationModel? SelectedComboBoxConfigurationParameters
        {
            get => _selectedComboBoxConfigurationParameters;
            set
            {
                if (SetField(ref _selectedComboBoxConfigurationParameters, value))
                {
                    if (value == _createNewSetPlaceholder)
                    {
                        if (!_isCreatingNewConfiguration) ResetParameters();

                        _isCreatingNewConfiguration = true;
                        _wasSaved = false;
                        ConfigurationName = string.Empty;
                    }
                    else if (value != null)
                    {
                        CurrentCLUMPPConfigurationModel = value;
                    }
                }
            }
        }
        public ObservableCollection<CLUMPPConfigurationModel> ConfigurationParametersComboBoxList { get; } = new();
        private static readonly CLUMPPConfigurationModel _createNewSetPlaceholder = new() { ParametersName = "Create new" };
        #endregion

        #region Commands properties
        public ICommand SaveChangesAsyncCommand { get; }
        public AsyncRelayCommand StartCLUMPPAsyncCommand { get; }
        public RelayCommand StopCLUMPPCommand { get; }
        public ICommand SelectPermutationFileCommand { get; }
        #endregion

        #region Enabled properties
        public bool GreedyOptionEnabled => SelectedAlgorithm != 1;
        public bool RepeatsEnabled => SelectedAlgorithm != 1 && SelectedGreedyOption != 1;
        public bool PermutationFileEnabled => SelectedAlgorithm != 1 && SelectedGreedyOption == 3;
        #endregion
        private void SelectPermutationFile()
        {
            PermutationFile = _dialogService.SelectFile(PathConstants.DEFAULT_DOCUMENTS_PATH);
        }

        private void RebuildComboBoxItems()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                ConfigurationParametersComboBoxList.Clear();

                foreach (CLUMPPConfigurationModel item in FilteredCLUMPPConfigurationModelsList)
                    ConfigurationParametersComboBoxList.Add(item);

                ConfigurationParametersComboBoxList.Add(_createNewSetPlaceholder);
            });
        }
        private void ResetParameters()
        {
            var newConfiguration = new CLUMPPConfigurationModel();

            ConfigurationName = string.Empty;
            SetConfigurationParameters(newConfiguration, false, PredefinedPopCount, false, PredefinedIndvCount);

            KFrom = PredefinedStructureKStart;
            KTo = PredefinedStructureKEnd;
        }

        protected override async Task LoadSelectedSetParametersAsync(SetModel? set)
        {
            if (set == null || !set.IsCLUMPPProcessed) return;

            var setName = set.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var configurations = await _clumppInteractionService.LoadConfigurationsListAsync(fullSetFolderPath);

                WorkflowState.LoadCLUMPPConfigurationModelsList(configurations);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading CLUMPP configurations for set {set.Name}: {ex.Message}");
            }
        }
        protected override async Task LoadSelectedCLUMPPConfigurationAsync(CLUMPPConfigurationModel? configuration)
        {
            if (configuration == null || CurrentSet == null) return;
            if (!configuration.IsProcessed) return;

            var setName = CurrentSet.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var (configurationModel, isPop, isIndv, kStart, kEnd) = await _clumppInteractionService.LoadConfiguration(fullSetFolderPath, configuration.ParametersName);

                SetField(ref _selectedComboBoxConfigurationParameters, configuration, nameof(SelectedComboBoxConfigurationParameters));

                SetConfigurationParameters(configurationModel, isPop, PredefinedPopCount, isIndv, PredefinedIndvCount);

                IsPop = isPop;
                IsIndv = isIndv;

                _isCreatingNewConfiguration = false;
                _wasSaved = true;

                _changesTracker.TakeModelSnapshot(configurationModel);
                _savedDataTypeParameters = (isPop, PredefinedPopCount, isIndv, PredefinedIndvCount);

                if (kEnd == 0 || kStart == 0)
                {
                    KFrom = 2;
                    KTo = 3;

                    UIDispatcherHelper.RunOnUI(() =>
                    {
                        CLUMPPProgressText = $"[{_configurationName}] Not started";
                        CLUMPPProgress = 0;
                    });
                    CLUMPPStopped = false;
                    _clumppCompleted = false;
                    return;
                }

                KFrom = kStart;
                KTo = kEnd;
                UIDispatcherHelper.RunOnUI(() =>
                {
                    CLUMPPProgress = 100;
                    CLUMPPProgressText = $"[{_configurationName}] Completed";
                });
                CLUMPPStopped = false;
                _clumppCompleted = true;
                WorkflowState.SetPredefinedCLUMPPParameters(kStart, kEnd);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading CLUMPP configuration \"{configuration.ParametersName}\" for set \"{CurrentSet.Name}\": {ex.Message}.");
            }
        }

        private (CLUMPPConfigurationModel, bool IsPop, int PopCount, bool IsIndv, int IndvCount) GetConfigurationParameters()
        {
            return (new CLUMPPConfigurationModel
            {
                ParametersName = ConfigurationName,
                R = R,
                M = SelectedAlgorithm,
                W = W,
                S = S ? 2 : 1,
                GREEDY_OPTION = SelectedGreedyOption,
                REPEATS = Repeats,
                PERMUTATIONFILE = PermutationFile,
                PRINT_PERMUTED_DATA = SelectedPrintPermutedData,
                PRINT_EVERY_PERM = PrintEveryPerm,
                PRINT_RANDOM_INPUTORDER = PrintRandomInputorder,
                ORDER_BY_RUN = OrderByRun
            }, IsPop, PopsCount, IsIndv, IndvsCount);
        }
        private void SetConfigurationParameters(CLUMPPConfigurationModel model, bool isPop, int popCount, bool isIndv, int indvCount)
        {
            ConfigurationName = model.ParametersName;
            IsIndv = isIndv;
            IsPop = isPop;
            PopsCount = popCount;
            IndvsCount = indvCount;
            //OperatingMode;
            //K = model.K;
            //C = model.C;
            R = model.R;
            SelectedAlgorithm = model.M;
            W = model.W;
            S = model.S == 2;
            SelectedGreedyOption = model.GREEDY_OPTION;
            Repeats = model.REPEATS;
            PermutationFile = model.PERMUTATIONFILE;
            SelectedPrintPermutedData = model.PRINT_PERMUTED_DATA;
            PrintEveryPerm = model.PRINT_EVERY_PERM;
            PrintRandomInputorder = model.PRINT_RANDOM_INPUTORDER;
            OrderByRun = model.ORDER_BY_RUN;
        }

        private async Task SaveChangesAsync()
        {
            var (configuration, isPop, popCount, isIndv, indvCount) = GetConfigurationParameters();

            if (CurrentSet == null ||
                HasErrors ||
                (!_changesTracker.HasChanges(configuration) && _savedDataTypeParameters == (isPop, popCount, isIndv, indvCount)) ||
                string.IsNullOrWhiteSpace(configuration.ParametersName) ||
                ((isIndv && indvCount == 0) || (isPop && popCount == 0)) ||
                configuration.R == 0)
                return;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            bool isNameChanged = !_isCreatingNewConfiguration && CurrentCLUMPPConfigurationModel?.ParametersName != configuration.ParametersName;
            if ((_isCreatingNewConfiguration || isNameChanged) && _clumppInteractionService.IsConfigurationExist(fullCurrentSetFolderPath, configuration.ParametersName))
            {
                _messageService.ShowWarning($"Configuration with name \"{configuration.ParametersName}\" already exists. Please choose a different name.");
                return;
            }

            try
            {
                if (configuration.GREEDY_OPTION == 1 && !IsCLUMPPFullSearchOptimal.Calculate(KTo, configuration.R))
                {
                    var result = _messageService.ShowQuetion("With the current values of K and number of iterations (R), FullSearch computations may take a considerable amount of time. Consider reducing the maximum K value or switching the method to Greedy / LargeKGreedy (for K ≥ 15). Are you sure you want to continue with the current settings?");

                    if (result == false) return;

                    _userSure = true;
                }

                if (isPop && popCount != PredefinedPopCount)
                {
                    var result = _messageService.ShowExpandedQuetion("Changing value of populations may cause CLUMPP to malfunction. Are you sure you want to proceed with the current value? (“No” - restore default value and continue.)");

                    if (result == false)
                    {
                        popCount = PredefinedPopCount;
                        PopsCount = popCount;
                    }
                    else if (result == null) return;
                }
                if (isIndv && indvCount != PredefinedIndvCount)
                {
                    var result = _messageService.ShowExpandedQuetion("Changing value of individuals may cause CLUMPP to malfunction. Are you sure you want to proceed with the current value? (“No” - restore default value and continue.)");

                    if (result == false)
                    {
                        indvCount = PredefinedIndvCount;
                        IndvsCount = indvCount;
                    }
                    else if (result == null) return;
                }
                if (configuration.R != PredefinedIterationsLimit)
                {
                    var result = _messageService.ShowExpandedQuetion("Changing value of iterations over K may cause CLUMPP to malfunction. Are you sure you want to proceed with the current value? (“No” - restore default value and continue.)");

                    if (result == false)
                    {
                        configuration.R = PredefinedIterationsLimit;
                        R = PredefinedIterationsLimit;
                    }
                    else if (result == null) return;
                }

                _wasSaved = false;

                if (_isCreatingNewConfiguration)
                {
                    await CreateNewConfigurationAsync(configuration, fullCurrentSetFolderPath, isPop, popCount, isIndv, indvCount);
                }
                else
                {
                    bool shouldBeSaved = await UpdateExistingConfigurationAsync(configuration, fullCurrentSetFolderPath, isPop, popCount, isIndv, indvCount);

                    if (!shouldBeSaved) return;
                }

                _changesTracker.TakeModelSnapshot(configuration);
                _savedDataTypeParameters = (isPop, popCount, isIndv, indvCount);

                _wasSaved = true;
                _isCreatingNewConfiguration = false;
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error saving CLUMPP configuration \"{configuration.ParametersName}\" for set \"{CurrentSet.Name}\": {ex.Message}.");
            }
            finally
            {
                StartCLUMPPAsyncCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanSaveChanges()
        {
            var (configurationParameters, isPops, popCount, isIndv, indvCount) = GetConfigurationParameters();

            return CurrentSet != null &&
                   !HasErrors &&
                   !string.IsNullOrWhiteSpace(configurationParameters.ParametersName) &&
                   (_changesTracker.HasChanges(configurationParameters) || _savedDataTypeParameters != (isPops, popCount, isIndv, indvCount)) &&
                   ((isIndv && indvCount != 0) || (isPops && popCount != 0)) &&
                   configurationParameters.R != 0;
        }
        private async Task CreateNewConfigurationAsync(CLUMPPConfigurationModel configurationModel, string fullCurrentSetFolderPath, bool isPop, int popCount, bool isIndv, int indvCount)
        {
            try
            {
                _clumppInteractionService.PrepareCLUMPPDirectory(fullCurrentSetFolderPath);

                await _clumppInteractionService.PrepareConfiguration(fullCurrentSetFolderPath, configurationModel, isPop, popCount, isIndv, indvCount);

                WorkflowState.AddNewCLUMPPConfiguration(configurationModel);
                SetField(ref _currentCLUMPPConfigurationModel, configurationModel, null);
                SetField(ref _selectedComboBoxConfigurationParameters, configurationModel, nameof(SelectedComboBoxConfigurationParameters));
                WorkflowState.CurrentCLUMPPConfigurationModel = configurationModel;

                _messageService.ShowInformation($"Configuration \"{configurationModel.ParametersName}\" was successfully created.");
            }
            catch (Exception) { throw; }
        }
        private async Task<bool> UpdateExistingConfigurationAsync(CLUMPPConfigurationModel configurationModel, string fullCurrentSetFolderPath, bool isPop, int popCount, bool isIndv, int indvCount)
        {
            try
            {
                if (CurrentSet == null) throw new InvalidOperationException("Current set is null on UpdateExistingConfigurationAsync() step.");
                if (CurrentCLUMPPConfigurationModel == null) throw new InvalidOperationException("Current configuration is null on UpdateExistingConfigurationAsync() step.");

                if (!_clumppInteractionService.IsConfigurationExist(fullCurrentSetFolderPath, CurrentCLUMPPConfigurationModel.ParametersName))
                {
                    _messageService.ShowError($"Configuration with name \"{CurrentCLUMPPConfigurationModel.ParametersName}\" was not found. Unable to save changes.");
                    return false;
                }

                if (CurrentCLUMPPConfigurationModel.ParametersName != configurationModel.ParametersName)
                {
                    CurrentCLUMPPConfigurationModel.ParametersName = configurationModel.ParametersName;

                    await _clumppInteractionService.RenameConfiguration(fullCurrentSetFolderPath, CurrentCLUMPPConfigurationModel.ParametersName, configurationModel.ParametersName);
                }

                if (_changesTracker.HasChanges(configurationModel))
                {
                    if (CurrentCLUMPPConfigurationModel.IsProcessed)
                    {
                        var willCreateNewConfiguration = _messageService.ShowQuetion("Changing the parameters will reset the processing progress made in later stages. Do you want to create a new configuration with the current settings?");

                        if (willCreateNewConfiguration)
                        {
                            SelectedComboBoxConfigurationParameters = _createNewSetPlaceholder;
                        }
                        else
                        {
                            var savedConfiguration = _changesTracker.GetSnapshot();

                            if (savedConfiguration is null) throw new InvalidOperationException("Saved configuration snapshot is null on UpdateExistingConfigurationAsync() step.");

                            SetConfigurationParameters(savedConfiguration, isPop, popCount, isIndv, indvCount);

                            _isCreatingNewConfiguration = false;
                            _wasSaved = true;
                        }
                        return false;
                    }
                    else
                    {
                        await _clumppInteractionService.PrepareConfiguration(fullCurrentSetFolderPath, configurationModel, isPop, popCount, isIndv, indvCount);
                    }
                }
                return true;
            }
            catch (Exception) { throw; }
        }

        private async Task StartCLUMPPAsync()
        {
            if (!_wasSaved ||
                _clumppInteractionService.IsRunning ||
                CurrentCLUMPPConfigurationModel == null ||
                CurrentSet == null ||
                HasErrorsFor(nameof(KFrom)) ||
                HasErrorsFor(nameof(KTo)) ||
                ((!IsPop || IsPop != _savedDataTypeParameters.savedIsPop) &&
                (!IsIndv || IsIndv != _savedDataTypeParameters.savedisIndv)))
                return;

            int kFrom = KFrom;
            int kTo = KTo;
            var configurationName = CurrentCLUMPPConfigurationModel.ParametersName;
            var isPop = IsPop;
            var isIndv = IsIndv;

            if (!_userSure && CurrentCLUMPPConfigurationModel.GREEDY_OPTION == 1 && !IsCLUMPPFullSearchOptimal.Calculate(kTo, CurrentCLUMPPConfigurationModel.R))
            {
                var result = _messageService.ShowQuetion("With the current values of K and number of iterations (R), FullSearch computations may take a considerable amount of time. Consider reducing the maximum K value or switching the method to Greedy / LargeKGreedy (for K ≥ 15). Are you sure you want to continue with the current settings?");

                if (result == false) return;
            }

            var setName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                _clumppCompleted = false;
                CLUMPPStopped = false;

                CLUMPPProgress = 0;
                CLUMPPProgressText = $"[{_configurationName}] In progress... 0%";

                await _clumppInteractionService.StartExecution(configurationName, isPop, isIndv, kFrom, kTo, fullCurrentSetFolderPath, _coresCount);

                _clumppCompleted = true;
                WorkflowState.MarkProcessedAndRefreshStage(CurrentSet, ProcessingStage);
                await _setConfigurationService.SaveConfigFileAsync(fullCurrentSetFolderPath, CurrentSet);

                WorkflowState.SetPredefinedCLUMPPParameters(kFrom, kTo);

                bool hasPopResults = _clumppInteractionService.HasPopResults(fullCurrentSetFolderPath, configurationName);

                WorkflowState.MarkCLUMPPConfigurationProcessed(CurrentCLUMPPConfigurationModel, hasPopResults);

                CLUMPPProgress = 100;
                CLUMPPProgressText = $"[{_configurationName}] Completed";

                _userSure = false;
            }
            catch (OperationCanceledException)
            {
                CLUMPPProgressText = $"[{_configurationName}] Stopped at {CLUMPPProgress:F0}%";
            }
            catch (Exception ex)
            {
                CLUMPPStopped = true;
                _messageService.ShowError($"An error occurred while running CLUMPP for set {setName}. {ex.Message}. See logs for details.");
                CLUMPPProgressText = $"[{setName}] Stopped by error at {CLUMPPProgress:F0}%";
            }
            finally
            {
                StopCLUMPPCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanStartCLUMPP()
        {
            return _wasSaved &&
                   !_clumppInteractionService.IsRunning &&
                   CurrentSet != null &&
                   CurrentCLUMPPConfigurationModel != null &&
                   !HasErrorsFor(nameof(KFrom)) &&
                   !HasErrorsFor(nameof(KTo)) &&
                   ((IsPop && IsPop == _savedDataTypeParameters.savedIsPop) ||
                    (IsIndv && IsIndv == _savedDataTypeParameters.savedisIndv));
        }

        private void StopCLUMPP()
        {
            CLUMPPStopped = true;
            CLUMPPProgressText = $"[{_configurationName}] Stopping...";
            _clumppInteractionService.StopExecution();
        }
        private bool CanStopCLUMPP()
        {
            return _clumppInteractionService.IsRunning;
        }
    }
}
