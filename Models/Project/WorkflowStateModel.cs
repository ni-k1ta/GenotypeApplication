using GenotypeApplication.Constants;
using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;

namespace GenotypeApplication.Models.Project
{
    public class WorkflowStateModel : ViewModelBase
    {
        private SetModel? _currentSet;
        private ObservableCollection<SetModel> _setModelsList = new();

        public ObservableCollection<SetModel> SetModelsList => _setModelsList;
        public SetModel? CurrentSet
        {
            get => _currentSet;
            set
            {
                if (_currentSet == value) return;

                if (_currentSet != null) _currentSet.IsCurrent = false;

                SetField(ref _currentSet, value);

                if (_currentSet != null) _currentSet.IsCurrent = true;

                CurrentSetChanged?.Invoke(_currentSet);
            }
        }

        public event Action<SetModel?>? CurrentSetChanged;
        public event Action? StateRefreshed;

        public void MarkProcessedAndRefreshStage(SetModel set, SetProcessingStage completedStage)
        {
            set.MarkAsProcessedForStage(completedStage);

            //обновить фильтры во всех вкладках
            StateRefreshed?.Invoke();

            //установить этот Set как текущий (чтобы он подхватился следующей вкладкой)
            CurrentSet = set;
        }
        public SetModel CreateNewSet(string name)
        {
            var newSet = new SetModel { Name = name };
            SetModelsList.Add(newSet);
            //CurrentSet = newSet;
            return newSet;
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
    }
}
