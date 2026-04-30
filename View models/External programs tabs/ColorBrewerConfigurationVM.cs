using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Distruct;
using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GenotypeApplication.View_models.External_programs_tabs
{
    public class ColorBrewerConfigurationVM : ViewModelBase, IWindowAware
    {
        private ColorBrewerScheme? _selectedScheme;
        private int _kMax;

        public int KMax
        {
            get => _kMax;
            set { SetField(ref _kMax, value); }
        }

        public List<ColorBrewerScheme> AvailableSchemes { get; }

        public ColorBrewerScheme? SelectedScheme
        {
            get => _selectedScheme;
            set
            {
                if (SetField(ref _selectedScheme, value))
                {
                    RebuildItems();
                }
            }
        }

        public ObservableCollection<ClusterColorItem> Items { get; } = new();

        public ICommand SaveColorBrewerConfigurationAsyncCommand { get; }

        public ColorBrewerConfigurationVM(int kMax, IWindowService windowService)
        {
            KMax = kMax;
            AvailableSchemes = ColorBrewerData.GetAvailableSchemes(kMax);

            if (AvailableSchemes.Count != 0)
                _selectedScheme = AvailableSchemes.First();

            RebuildItems();

            _windowService = windowService;
            SaveColorBrewerConfigurationAsyncCommand = new AsyncRelayCommand(execute => SaveColorBrewerConfigurationAsync());
        }

        private readonly IWindowService _windowService;
        private WeakReference<Window>? _currentWindowRef;

        private void RebuildItems()
        {
            Items.Clear();
            if (SelectedScheme == null) return;

            var colorNames = SelectedScheme.GetColorNames(KMax);
            for (int i = 0; i < colorNames.Count; i++)
            {
                Items.Add(new ClusterColorItem
                {
                    ClusterIndex = i + 1,
                    ColorName = colorNames[i]
                });
            }
        }

        private async Task SaveColorBrewerConfigurationAsync()
        {
            if (_currentWindowRef != null && _currentWindowRef.TryGetTarget(out var window))
            {
                _windowService.CloseDialogWindow(window, true);
            }
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
