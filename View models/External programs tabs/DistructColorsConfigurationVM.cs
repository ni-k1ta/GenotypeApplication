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
            "dark_gray", "light_pink", "dark_brown", "dark_orange", "dark_purple",

            "color32", "color33", "color34", "color35", "color36",
            "color37", "color38", "color39", "color40", "color41",
            "color42", "color43", "color44", "color45", "color46",
            "color47", "color48", "color49", "color50", "color51",
            "color52", "color53", "color54", "color55", "color56",
            "color57", "color58", "color59", "color60", "white",

            "color101", "color102", "color103", "color104", "color105",
            "color106", "color107", "color108", "color109", "color110",
            "color111", "color112", "color113", "color114", "color115",
            "color116", "color117", "color118", "color119", "color120",
            "color121", "color122", "color123", "color124", "color125",
            "color126", "color127", "color128", "color129", "color130"
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
