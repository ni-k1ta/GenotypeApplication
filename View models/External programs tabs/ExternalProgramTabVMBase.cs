using GenotypeApplication.Constants;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
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

        public ICollectionView FilteredSetModelsList { get; }

        public SetModel? CurrentSet
        {
            get => _currentSet;
            set
            {
                if (_currentSet == value) return;
                SetField(ref _currentSet, value);

                //если это пользовательский выбор (не синхронизация) — сообщаем сервису
                if (!_isSyncing) WorkflowState.CurrentSet = value;

                LoadSelectedSetParameters(value);
            }
        }

        protected ExternalProgramTabVMBase(WorkflowStateModel workflowState, SetProcessingStage stage, int coresCount, string fullProjectFolderPath)
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
            workflowState.StateRefreshed += () =>
                Application.Current.Dispatcher.Invoke(() => FilteredSetModelsList.Refresh());
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
