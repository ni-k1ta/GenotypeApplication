using GenotypeApplication.Constants;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.MVVM.TreeView;
using System.Collections.ObjectModel;

namespace GenotypeApplication.Models.Project
{
    public class WorkflowStateModel : ViewModelBase
    {
        private SetModel? _currentSet;
        private ObservableCollection<SetModel> _setModelsList = new();

        private ProjectExplorerViewModel _projectExplorer;

        public WorkflowStateModel(ProjectExplorerViewModel projectExplorer)
        {
            _projectExplorer = projectExplorer;
        }

        // Функция проверки — устанавливается снаружи
        public Func<bool>? CanChangeActiveSet { get; set; }

        public ObservableCollection<SetModel> SetModelsList => _setModelsList;
        public SetModel? CurrentSet
        {
            get => _currentSet;
            set
            {
                if (_currentSet == value) return;

                if (CanChangeActiveSet != null && !CanChangeActiveSet())
                {
                    ActiveSetChangeBlocked?.Invoke();
                    return;
                }

                if (_currentSet != null) _currentSet.IsCurrent = false;

                SetField(ref _currentSet, value);

                if (_currentSet != null) _currentSet.IsCurrent = true;

                CurrentSetChanged?.Invoke(_currentSet);

                _projectExplorer.SetName = value?.Name;
            }
        }

        // Событие для уведомления UI о блокировке
        public event Action? ActiveSetChangeBlocked;

        public event Action<SetModel?>? CurrentSetChanged;
        public event Action? StateRefreshed;

        public void MarkProcessedAndRefreshStage(SetModel set, SetProcessingStage completedStage)
        {
            set.MarkAsProcessedForStage(completedStage);

            StateRefreshed?.Invoke();
            CurrentSetChanged?.Invoke(set);
        }

        public void MarkUnprocessedAndRefreshStage(SetModel set, SetProcessingStage stage)
        {
            set.MarkAsUnprocessedForStage(stage);

            StateRefreshed?.Invoke();
            CurrentSetChanged?.Invoke(set);
        }

        public SetModel CreateNewSet(string name)
        {
            var newSet = new SetModel { Name = name, IsCurrent = true };
            SetModelsList.Add(newSet);
            return newSet;
        }
        public void RemoveSet(SetModel set)
        {
            if (!SetModelsList.Contains(set)) return;
            if (CurrentSet == set) CurrentSet = null;
            SetModelsList.Remove(set);

            StateRefreshed?.Invoke();
        }
        public void LoadSetModelsList(List<SetModel> setModels)
        {
            SetModelsList.Clear();
            SetModel? markedAsCurrent = null;

            foreach (var set in setModels)
            {
                if (set.IsCurrent) markedAsCurrent = set;

                SetModelsList.Add(set);
            }

            //сначала добавляем всё, потом устанавливаем текущий — чтобы фильтры уже имели данные к моменту уведомления
            StateRefreshed?.Invoke();

            if (markedAsCurrent != null) CurrentSet = markedAsCurrent;
        }





        private CLUMPPConfigurationModel? _currentCLUMPPConfigurationModel;
        private ObservableCollection<CLUMPPConfigurationModel> _clumppConfigurationModelsList = new();

        public ObservableCollection<CLUMPPConfigurationModel> CLUMPPConfigurationModelsList => _clumppConfigurationModelsList;
        public CLUMPPConfigurationModel? CurrentCLUMPPConfigurationModel
        {
            get => _currentCLUMPPConfigurationModel;
            set
            {
                SetField(ref _currentCLUMPPConfigurationModel, value);
                CurrentCLUMPPConfigurationChanged?.Invoke(_currentCLUMPPConfigurationModel);
            }
        }

        public event Action<CLUMPPConfigurationModel?>? CurrentCLUMPPConfigurationChanged;
        public event Action? CLUMPPConfigurationListRefreshed;
        public void MarkCLUMPPConfigurationProcessed(CLUMPPConfigurationModel configuration, bool hasPopResults)
        {
            configuration.IsProcessed = true;
            configuration.HasPopResults = hasPopResults;

            CLUMPPConfigurationListRefreshed?.Invoke();
            CurrentCLUMPPConfigurationChanged?.Invoke(configuration);
        }
        public void AddNewCLUMPPConfiguration(CLUMPPConfigurationModel configurationModel)
        {
            CLUMPPConfigurationModelsList.Add(configurationModel);
            //CLUMPPConfigurationListRefreshed?.Invoke();
        }
        public void RemoveCLUMPPConfiguration(CLUMPPConfigurationModel configurationModel)
        {
            if (!CLUMPPConfigurationModelsList.Contains(configurationModel)) return;
            if (CurrentCLUMPPConfigurationModel == configurationModel) CurrentCLUMPPConfigurationModel = null;
            CLUMPPConfigurationModelsList.Remove(configurationModel);
            CLUMPPConfigurationListRefreshed?.Invoke();
        }
        public void LoadCLUMPPConfigurationModelsList(List<CLUMPPConfigurationModel> configurationModels)
        {
            CLUMPPConfigurationModelsList.Clear();

            foreach (var config in configurationModels)
            {
                CLUMPPConfigurationModelsList.Add(config);
            }

            CLUMPPConfigurationListRefreshed?.Invoke();
        }




        public event Action<int, int, int, int>? PredefinedStructureParametersChanged;
        public event Action<int, int>? PredefinedCLUMPPParametersChanged;
        public event Action<int>? PredifinedPopCountChanged;

        public void SetPredefinedStructureParameters(int iterationsLimit, int kStart, int kEnd, int indvCount)
        {
            PredefinedStructureParametersChanged?.Invoke(iterationsLimit, kStart, kEnd, indvCount);
        }
        public void SetPredefinedCLUMPPParameters(int kStart, int kEnd)
        {
            PredefinedCLUMPPParametersChanged?.Invoke(kStart, kEnd);
        }
        public void SetPredefinedPopCount(int popCount)
        {
            PredifinedPopCountChanged?.Invoke(popCount);
        }



        //public event Action? NewSetCreated;
        //public void NotifyNewSetCreated()
        //{
        //    NewSetCreated?.Invoke();
        //}
    }
}
