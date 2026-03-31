using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace GenotypeApplication.View_models.External_programs_tabs
{
    public abstract class ExternalProgramTabVMBase : ViewModelBase
    {
        protected readonly WorkflowStateModel WorkflowState;
        protected readonly SetProcessingStage ProcessingStage;

        protected SetModel? _currentSet;
        private bool _isSyncing; // защита от циклов

        protected readonly string _fullProjectFolderPath;
        protected readonly int _coresCount;

        protected int PredefinedIterationsLimit
        {
            get;
            private set;
        }
        protected int PredefinedKStart
        {
            get;
            private set;
        }
        protected int PredefinedKEnd
        {
            get;
            private set;
        }
        protected int PredefinedPopCount
        {
            get;
            private set;
        }
        protected int PredefinedIndvCount
        {
            get;
            private set;
        }

        public ICollectionView FilteredSetModelsList { get; }

        public SetModel? CurrentSet
        {
            get => _currentSet;
            set
            {
                if (SetField(ref _currentSet, value))
                {
                    //если это пользовательский выбор (не синхронизация) — сообщаем сервису
                    if (!_isSyncing) WorkflowState.CurrentSet = value;

                    LoadSelectedSetParameters(value);
                }
            }
        }

        protected IMessageService _messageService;
        protected IDirectoryService _directoryService;
        protected IFileService _fileService;
        protected IDialogService _dialogService;

        protected ExternalProgramTabVMBase(WorkflowStateModel workflowState, SetProcessingStage stage, int coresCount, string fullProjectFolderPath, IDirectoryService directoryService, IFileService fileService, IMessageService messageService, IDialogService dialogService)
        {
            WorkflowState = workflowState;
            ProcessingStage = stage;

            _coresCount = coresCount;
            _fullProjectFolderPath = fullProjectFolderPath;

            //создаём фильтрованное представление
            FilteredSetModelsList = new ListCollectionView(workflowState.SetModelsList)
            {
                Filter = obj => obj is SetModel s && s.IsAvailableForStage(ProcessingStage)
            };

            //подписка на смену текущего Set
            workflowState.CurrentSetChanged += OnCurrentSetChanged;

            //подписка на обновление фильтров
            workflowState.StateRefreshed += () => UIDispatcherHelper.RunOnUI(() => FilteredSetModelsList.Refresh());

            _directoryService = directoryService;
            _fileService = fileService;
            _messageService = messageService;
            _dialogService = dialogService;
        }

        private void OnCurrentSetChanged(SetModel? newSet)
        {
            if (newSet != null && !newSet.IsAvailableForStage(ProcessingStage))
            {
                return;
            }

            _isSyncing = true;
            CurrentSet = newSet;
            _isSyncing = false;
        }

        //переопределяется в каждой VM — загрузка параметров Set
        protected abstract void LoadSelectedSetParameters(SetModel? set);
    }
}
