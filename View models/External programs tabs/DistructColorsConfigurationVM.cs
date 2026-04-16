using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Distruct;
using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GenotypeApplication.View_models.External_programs_tabs
{
    public class DistructColorsConfigurationVM : ViewModelBase, IWindowAware
    {
        private bool _isGrayscale;
        private int _kMax;

        public int KMax
        {
            get => _kMax;
            set { SetField(ref _kMax, value); }
        }

        public bool IsGrayscale
        {
            get => _isGrayscale;
            set { SetField(ref _isGrayscale, value); }
        }

        public ObservableCollection<ClusterColorItem> Items { get; } = new();

        private static readonly string[] DefaultColors =
        {
            "orange", "blue", "yellow", "pink", "green",
            "purple", "red", "light_green", "dark_blue", "light_purple",
            "light_yellow", "brown", "light_blue", "olive_green", "peach",
            "sea_green", "yellow_green", "blue_purple", "blue_green", "gray",
            "dark_green", "light_gray", "red2", "light_blue2", "light_orange",
            "dark_gray", "light_pink", "dark_brown", "dark_orange", "dark_purple"
        };

        public List<string> AvailableColors { get; } = new(DefaultColors);


        private readonly IWindowService _windowService;
        private WeakReference<Window>? _currentWindowRef;

        public DistructColorsConfigurationVM(int kMax, bool isGrayscale, IWindowService windowService)
        {
            KMax = kMax;

            for (int i = 1; i <= kMax; i++)
            {
                Items.Add(new ClusterColorItem
                {
                    ClusterIndex = i,
                    ColorName = DefaultColors[(i - 1) % DefaultColors.Length],
                    GrayscaleValue = (double)i / (kMax + 1)
                });
            }

            IsGrayscale = isGrayscale;

            SaveConfigurationPaletteAsyncCommand = new AsyncRelayCommand(execute => SaveConfigurationPaletteAsync());

            _windowService = windowService;
        }
        public ICommand SaveConfigurationPaletteAsyncCommand { get; }

        private async Task SaveConfigurationPaletteAsync()
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
