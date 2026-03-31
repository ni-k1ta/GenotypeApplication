using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.View_models.External_programs_tabs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class CLUMPPTabControlVM : ExternalProgramTabVMBase
    {
        private bool _isCreatingNewConfiguration;
        private string _savedConfigurationName;
        private bool _wasSaved;

        private int _kFrom;
        private int _kTo;

        #region Configuration parameters
        private string _configurationName;

        private bool _isPop;
        private bool _savedIsPop;
        private bool _isIndv;
        private bool _savedIsIndv;

        private int _popsCount;
        private int _savedPopsCount;
        private int _indvsCount;
        private int _savedIndvsCount;

        //private int _k;
        private int _r;
        private bool _w;
        private bool _s; // 1 or 2
        private int _repeats = 1000;
        private string _permutationFile;
        private bool _printEveryPerm;
        private bool _printRandomInputorder;
        private bool _overrideWarnings;
        private int _orderByRun = 1;

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

        private CLUMPPConfigurationModel? _selectedConfigurationParameters = new();
        private ObservableCollection<CLUMPPConfigurationModel> _savedConfigurationParametersItems = new();
        #endregion

        private readonly CLUMPPInteractionService _clumppInteractionService;

        public CLUMPPTabControlVM(WorkflowStateModel workflowState, int coresCount, string fullProjectFolderPath, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService) : base(workflowState, SetProcessingStage.CLUMPP, coresCount, fullProjectFolderPath, directoryService, fileService, messageService, dialogService)
        {
            SaveChangesAsyncCommand = new AsyncRelayCommand(execute => SaveChangesAsync(), canExecute => CanSaveChanges());
            StartCLUMPPAsyncCommand = new AsyncRelayCommand(execute => StartCLUMPPAsync(), canExecute => CanStartCLUMPP());
            StopCLUMPPCommand = new RelayCommand(execute => StopCLUMPP(), canExecute => CanStopCLUMPP());

            _clumppInteractionService = new CLUMPPInteractionService(directoryService, fileService);

            RebuildConfigurationParametersItems();
            SelectedConfigurationParameters = ConfigurationParametersItems.LastOrDefault();

            _isCreatingNewConfiguration = true;
        }

        public int KFrom
        {
            get => _kFrom;
            set { SetField(ref _kFrom, value); }
        }
        public int KTo
        {
            get => _kTo;
            set { SetField(ref _kTo, value); }
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
        //public int K//
        //{
        //    get => _k;
        //    set { SetField(ref _k, value); }
        //}
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
            set { SetField(ref _permutationFile, value); }
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
        public bool OverrideWarnings//
        {
            get => _overrideWarnings;
            set { SetField(ref _overrideWarnings, value); }
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
            set { SetField(ref _selectedAlgorithm, value); }
        }

        public Dictionary<int, string> GreedyOptions => _greedyOptions;//
        public int SelectedGreedyOption//
        {
            get => _selectedGreedyOption;
            set { SetField(ref _selectedGreedyOption, value); }
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
            set { SetField(ref _configurationName, value); }
        }
        public CLUMPPConfigurationModel? SelectedConfigurationParameters
        {
            get => _selectedConfigurationParameters;
            set
            { 
                if (SetField(ref _selectedConfigurationParameters, value))
                {
                    if (value == CreateNewSetPlaceholder)
                    {
                        if (!_isCreatingNewConfiguration) return; //todo
                        
                        _isCreatingNewConfiguration = true;
                    }
                }
            }
        }
        public ObservableCollection<CLUMPPConfigurationModel> ConfigurationParametersItems { get; } = new();
        public static readonly CLUMPPConfigurationModel CreateNewSetPlaceholder = new() { ParametersName = "Create new" };
        #endregion

        #region Commands properties
        public ICommand SaveChangesAsyncCommand { get; }
        public AsyncRelayCommand StartCLUMPPAsyncCommand { get; }
        public RelayCommand StopCLUMPPCommand { get; }
        #endregion

        private void RebuildConfigurationParametersItems()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                ConfigurationParametersItems.Clear();

                foreach (CLUMPPConfigurationModel item in _savedConfigurationParametersItems)
                    ConfigurationParametersItems.Add(item);

                ConfigurationParametersItems.Add(CreateNewSetPlaceholder);
            });
        }

        protected override void LoadSelectedSetParameters(SetModel? set)
        {
            
        }

        private (CLUMPPConfigurationModel, bool IsPop, int PopCount, bool IsIndv, int IndvCount) GetConfigurationParameters()
        {
            return (new CLUMPPConfigurationModel
            {
                ParametersName = ConfigurationName,
                //DATATYPE,
                //INDFILE
                //POPFILE
                //OUTFILE
                //MISCFILE
                //K = K,
                //C
                R = R,
                M = SelectedAlgorithm,
                W = W,
                S = S ? 2 : 1,
                GREEDY_OPTION = SelectedGreedyOption,
                REPEATS = Repeats,
                PERMUTATIONFILE = PermutationFile,
                PRINT_PERMUTED_DATA = SelectedPrintPermutedData,
                //PERMUTED_DATAFILE
                PRINT_EVERY_PERM = PrintEveryPerm,
                //EVERY_PERMFILE
                PRINT_RANDOM_INPUTORDER = PrintRandomInputorder,
                //RANDOM_INPUTORDERFILE
                OVERRIDE_WARNINGS = OverrideWarnings,
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
            OverrideWarnings = model.OVERRIDE_WARNINGS;
            OrderByRun = model.ORDER_BY_RUN;
        }

        private async Task SaveChangesAsync()
        {
            if (CurrentSet == null) return;
            var newParametersName = ConfigurationName;
            if (string.IsNullOrWhiteSpace(newParametersName)) return;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            _clumppInteractionService.PrepareCLUMPPDirectory(fullCurrentSetFolderPath);

            bool isNameChanged = !_isCreatingNewConfiguration && _savedConfigurationName != newParametersName;
            if ((_isCreatingNewConfiguration || isNameChanged) && _clumppInteractionService.IsConfigurationExist(fullCurrentSetFolderPath, newParametersName))
            {
                _messageService.ShowWarning($"Configuration with name \"{newParametersName}\" already exists. Please choose a different name.");
                return;
            }

            try
            {
                _wasSaved = false;

                var (configurationParameters, isPop, popCount, isIndv, indvCount)= GetConfigurationParameters();

                if (_isCreatingNewConfiguration)
                {
                    await CreateNewConfigurationAsync(configurationParameters, fullCurrentSetFolderPath, isPop, popCount, isIndv, indvCount);
                }
                else
                {
                    bool shouldBeSaved = await UpdateExistingConfigurationAsync(configurationParameters, fullCurrentSetFolderPath);

                    if (!shouldBeSaved) return;
                }

                _wasSaved = true;
                _savedConfigurationName = newParametersName;
                _isCreatingNewConfiguration = false;
                _savedIndvsCount = indvCount;
                _savedIsIndv = isIndv;
                _savedIsPop = isPop;
                _savedPopsCount = popCount;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                StartCLUMPPAsyncCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanSaveChanges()
        {
            return CurrentSet != null && !string.IsNullOrWhiteSpace(ConfigurationName);
        }
        private async Task CreateNewConfigurationAsync(CLUMPPConfigurationModel configurationModel, string fullCurrentSetFolderPath, bool isPop, int popCount, bool isIndv, int indvCount)
        {
            await _clumppInteractionService.PrepareConfiguration(fullCurrentSetFolderPath, configurationModel, isPop, popCount, isIndv, indvCount);

            _savedConfigurationParametersItems.Add(configurationModel);
            RebuildConfigurationParametersItems();
            SelectedConfigurationParameters = configurationModel;

            _messageService.ShowInformation($"Configuration \"{configurationModel.ParametersName}\" was successfully created.");
        }
        private async Task<bool> UpdateExistingConfigurationAsync(CLUMPPConfigurationModel configurationParametersModel, string fullCurrentSetFolderPath)
        {
            return false;
        }

        private async Task StartCLUMPPAsync()
        {
            if (!_wasSaved)
                return;

            if (CurrentSet == null) return;
            if (SelectedConfigurationParameters == null) return;

            int kFrom = KFrom;
            int kTo = KTo;
            var configurationName = _savedConfigurationName;
            var isPop = _savedIsPop;
            var isIndv = _savedIsIndv;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            try
            {
                await _clumppInteractionService.StartExecution(configurationName, isPop, isIndv, kFrom, kTo, fullCurrentSetFolderPath, _coresCount);

                WorkflowState.MarkProcessedAndRefreshStage(CurrentSet, ProcessingStage);
            }
            catch (Exception)
            {
                //todo
                throw;
            }
            finally
            {
                StopCLUMPPCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanStartCLUMPP()
        {
            return true;
        }

        private void StopCLUMPP()
        {
        }
        private bool CanStopCLUMPP()
        {
            return _clumppInteractionService.IsRunning;
        }
    }
}
