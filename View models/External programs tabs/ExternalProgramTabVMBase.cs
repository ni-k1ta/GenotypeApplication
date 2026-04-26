using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace GenotypeApplication.View_models.External_programs_tabs
{
    public abstract class ExternalProgramTabVMBase : ViewModelErrors
    {
        protected readonly WorkflowStateModel WorkflowState;
        protected readonly SetProcessingStage ProcessingStage;

        protected SetModel? _currentSet;
        private bool _isSyncing; // защита от циклов

        public SetModel? CurrentSet 
        {   
            get => _currentSet;
            set
            {
                if (SetField(ref _currentSet, value))
                {
                    OnPropertyChanged(nameof(ConfigurationEnabled));

                    if (value != null && !IsValidSet(value))
                    {
                        _messageService.ShowWarning($"Set folder with name \"{value.Name}\" was not found.");

                        WorkflowState.RemoveSet(value);
                        return;
                    }

                    _ = LoadSelectedSetParametersAsync(value);
                    //если это пользовательский выбор (не синхронизация) — сообщаем сервису
                    if (!_isSyncing) WorkflowState.CurrentSet = value;
                }
            }
        }
        public bool ConfigurationEnabled => CurrentSet != null;
        public bool CLUMPPConfigurationEnabled => CurrentSet != null && CurrentCLUMPPConfigurationModel != null;
        public ICollectionView FilteredSetModelsList { get; }

        // Каждая вкладка знает, запущено ли у неё приложение
        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetField(ref _isRunning, value);
        }

        protected CLUMPPConfigurationModel? _currentCLUMPPConfigurationModel;
        private bool _isSyncingCLUMPPConfiguration; // защита от циклов

        public ICollectionView FilteredCLUMPPConfigurationModelsList { get; }
        public CLUMPPConfigurationModel? CurrentCLUMPPConfigurationModel
        {
            get => _currentCLUMPPConfigurationModel;
            set
            {
                if (SetField(ref _currentCLUMPPConfigurationModel, value))
                {
                    OnPropertyChanged(nameof(CLUMPPConfigurationEnabled));

                    if (value != null && !IsValidCLUMPPConfiguration(value))
                    {
                        _messageService.ShowWarning($"CLUMPP configuration folder with name \"{value.ParametersName}\" was not found.");

                        WorkflowState.RemoveCLUMPPConfiguration(value);
                        return;
                    }

                    _ = LoadSelectedCLUMPPConfigurationAsync(value);
                    if (!_isSyncingCLUMPPConfiguration) WorkflowState.CurrentCLUMPPConfigurationModel = value;
                }
            }
        }


        protected IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> _kRangeValidator;
        protected IValidator<string> _pathValidator;
        protected IValidator<string> _parameterNameValidator;

        protected ExternalProgramTabVMBase(WorkflowStateModel workflowState, SetProcessingStage stage, int coresCount, string fullProjectFolderPath, SetConfigurationService setConfigurationService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService, IDialogService dialogService, LoggerService loggerService, IValidator<string> pathValidator, IValidator<string> parameterNameValidator, IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> kRangeValidator)
        {
            WorkflowState = workflowState;
            ProcessingStage = stage;

            //создаём фильтрованное представление
            FilteredSetModelsList = new ListCollectionView(workflowState.SetModelsList)
            {
                Filter = obj => obj is SetModel s && s.IsAvailableForStage(ProcessingStage)
            };
            //подписка на смену текущего Set
            workflowState.CurrentSetChanged += OnCurrentSetChanged;
            //подписка на обновление фильтров
            workflowState.StateRefreshed += () => UIDispatcherHelper.RunOnUI(() => FilteredSetModelsList.Refresh());


            FilteredCLUMPPConfigurationModelsList = new ListCollectionView(workflowState.CLUMPPConfigurationModelsList)
            {
                Filter = obj => obj is CLUMPPConfigurationModel m && m.IsAvailableForStage(ProcessingStage)
            };
            workflowState.CurrentCLUMPPConfigurationChanged += OnCurrentCLUMPPConfigurationModelChanged;
            workflowState.CLUMPPConfigurationListRefreshed += () => UIDispatcherHelper.RunOnUI(() => FilteredCLUMPPConfigurationModelsList.Refresh());


            _coresCount = coresCount;
            _fullProjectFolderPath = fullProjectFolderPath;

            _setConfigurationService = setConfigurationService;
            _directoryService = directoryService;
            _fileService = fileService;
            _messageService = messageService;
            _dialogService = dialogService;

            _logger = loggerService.CreateLogger(stage);

            workflowState.PredefinedStructureParametersChanged += OnPredefinedStructureParametersChanged;
            workflowState.PredefinedCLUMPPParametersChanged += OnPredefinedCLUMPPParametersChanged;
            workflowState.PredifinedPopCountChanged += OnPredefinedPopCountChanged;

            _kRangeValidator = kRangeValidator;
            _parameterNameValidator = parameterNameValidator;
            _pathValidator = pathValidator;
        }

        private void OnCurrentSetChanged(SetModel? newSet)
        {
            if (newSet != null && !newSet.IsAvailableForStage(ProcessingStage))
            {
                SetField(ref _currentSet, null, nameof(CurrentSet));
                ResetProgress();
                return;
            }

            _isSyncing = true;
            CurrentSet = newSet;
            _isSyncing = false;

            var setName = newSet?.Name;
            _logger.ChangeSet(setName);
        }
        protected abstract void ResetProgress();

        protected virtual bool IsValidSet(SetModel set)
        {
            var setName = set.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            return _setConfigurationService.IsSetExist(fullSetFolderPath) && !_directoryService.IsDirectoryEmpty(fullSetFolderPath);
        }

        //переопределяется в каждой VM — загрузка параметров Set
        protected abstract Task LoadSelectedSetParametersAsync(SetModel? set);




        private void OnCurrentCLUMPPConfigurationModelChanged(CLUMPPConfigurationModel? newConfiguration)
        {
            if (newConfiguration != null && !newConfiguration.IsAvailableForStage(ProcessingStage)) return;

            _isSyncingCLUMPPConfiguration = true;
            CurrentCLUMPPConfigurationModel = newConfiguration;
            _isSyncingCLUMPPConfiguration = false;
        }
        protected abstract Task LoadSelectedCLUMPPConfigurationAsync(CLUMPPConfigurationModel? configuration);
        protected virtual bool IsValidCLUMPPConfiguration(CLUMPPConfigurationModel configuration)
        {
            if (CurrentSet == null) return false;

            var configName = configuration.ParametersName;
            var fullConfigFolderPath = Path.Combine(_fullProjectFolderPath, CurrentSet.Name, CLUMPPConstants.CLUMPP_FOLDER_NAME, configName);
            return _directoryService.IsDirectoryExist(fullConfigFolderPath) && !_directoryService.IsDirectoryEmpty(fullConfigFolderPath);
        }


        protected readonly string _fullProjectFolderPath;
        protected readonly int _coresCount;

        private int _predefinedIterationsLimit;
        public int PredefinedIterationsLimit
        {
            get => _predefinedIterationsLimit;
            private set => SetField(ref _predefinedIterationsLimit, value);
        }

        private int _predefinedStructureKStart;
        public int PredefinedStructureKStart
        {
            get => _predefinedStructureKStart;
            private set
            {
                if (value == 1)
                {
                    SetField(ref _predefinedStructureKStart, 2);
                    return;
                }

                SetField(ref _predefinedStructureKStart, value);
            }
        }

        private int _predefinedStructureKEnd;
        public int PredefinedStructureKEnd
        {
            get => _predefinedStructureKEnd;
            private set => SetField(ref _predefinedStructureKEnd, value);
        }

        private int _predefinedPopCount;
        public int PredefinedPopCount
        {
            get => _predefinedPopCount;
            private set => SetField(ref _predefinedPopCount, value);
        }

        private int _predefinedIndvCount;
        public int PredefinedIndvCount
        {
            get => _predefinedIndvCount;
            private set => SetField(ref _predefinedIndvCount, value);
        }

        private int _predefinedCLUMPPKStart;
        public int PredefinedCLUMPPKStart
        {
            get => _predefinedCLUMPPKStart;
            set => SetField(ref _predefinedCLUMPPKStart, value);
        }

        private int _predefinedCLUMPPKEnd;
        public int PredefinedCLUMPPKEnd
        {
            get => _predefinedCLUMPPKEnd;
            set => SetField(ref _predefinedCLUMPPKEnd, value);
        }

        protected SetConfigurationService _setConfigurationService;
        protected IMessageService _messageService;
        protected IDirectoryService _directoryService;
        protected IFileService _fileService;
        protected IDialogService _dialogService;

        protected ProgramLogger _logger { get; }

        private void OnPredefinedStructureParametersChanged(int predefinedIterationsLimit, int predefinedKStart, int predefinedKEnd, int predefinedIndvCount)
        {
            PredefinedIterationsLimit = predefinedIterationsLimit;
            PredefinedStructureKEnd = predefinedKEnd;
            PredefinedStructureKStart = predefinedKStart;
            PredefinedIndvCount = predefinedIndvCount;
        }
        private void OnPredefinedCLUMPPParametersChanged(int predefinedKStart, int predefinedKEnd)
        {
            PredefinedCLUMPPKStart = predefinedKStart;
            PredefinedCLUMPPKEnd = predefinedKEnd;
        }
        private void OnPredefinedPopCountChanged(int predefinedPopCount)
        {
            PredefinedPopCount = predefinedPopCount;
        }
    }
}
