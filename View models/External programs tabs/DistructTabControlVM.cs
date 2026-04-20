using GenotypeApplication.Application_windows;
using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Models.Distruct;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using GenotypeApplication.View_models.External_programs_tabs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GenotypeApplication.View_models
{
    public class DistructTabControlVM : ExternalProgramTabVMBase
    {
        #region Status parameters
        private bool _isCreatingNewConfiguration;
        private string _savedConfigurationName;
        private bool _wasSaved;
        #endregion

        #region Configuration parameters
        private string _parametersName;

        private string _infile_label_atop = string.Empty;
        private string _infile_label_below = string.Empty;
        private string _infile_clust_perm = string.Empty;

        private int _kFrom;
        private int _kTo;

        private int _numpops;
        private int _numinds;
        private bool _print_indivs;
        private bool _print_label_atop;
        private bool _print_label_below;
        private bool _print_sep;
        private double _fontheight;
        private double _dist_above;
        private double _dist_below;
        private double _boxheight;
        private double _indivwidth;

        private readonly Dictionary<int, string> _orientationOptions = new()
        {
            { 0, "Horizontal" },
            { 1, "Vertical" },
            { 2, "Reverse horizontal" },
            { 3, "Reverse vertical" }
        };
        private int _selectedOrientation = 0;

        private double _xorigin;
        private double _yorigin;
        private double _xscale;
        private double _yscale;
        private double _angle_label_atop;
        private double _angle_label_below;
        private double _linewidth_rim;
        private double _linewidth_sep;
        private double _linewidth_ind;
        private bool _grayscale;
        private bool _echo_data;
        private bool _reprint_data;
        private bool _print_infile_name;
        private bool _print_color_brewer;
        #endregion

        #region Additional parameters
        private bool _isPDF;
        private bool _isPNG;
        private bool _isJPEG;
        private bool _isBMP;
        private int _selectedKForExport;
        private List<int> _kForExportItems = new();
        private bool _isExporting;

        private int _selectedKForPreview;
        private List<int> _kForPreviewItems = new();
        private bool _isPreviewing;
        #endregion

        #region Services parameters
        private ParametersChangesTracker<DistructConfigurationModel> _changesTracker = new();
        private DistructInteractionService _distructInteractionService;
        private readonly IWindowService _windowService;
        #endregion

        public DistructTabControlVM(WorkflowStateModel workflowState, int coresCount, string fullProjectFolderPath, SetConfigurationService setConfigurationService, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService, IValidator<string> pathValidator, IValidator<string> parameterNameValidator, LoggerService loggerService, IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> kRangeValidator, IWindowService windowService) : base(workflowState, SetProcessingStage.Distruct, coresCount, fullProjectFolderPath, setConfigurationService, directoryService, fileService, messageService, dialogService, loggerService, pathValidator, parameterNameValidator, kRangeValidator)
        {
            _parametersName = string.Empty;
            _savedConfigurationName = string.Empty;

            _kFrom = 2;
            _kTo = 3;

            RebuildConfigurationParametersItems();
            SelectedConfigurationParameters = ConfigurationParametersItems.LastOrDefault();

            PropertyChanged += OnLimitedValuesChanged;

            SaveChangesAsyncCommand = new AsyncRelayCommand(execute => SaveChangesAsync(), canExecute => CanSaveChanges());
            StartDistructAsyncCommand = new AsyncRelayCommand(execute => StartDistructAsync(), canExecute => CanStartDistruct());
            StopDistructCommand = new RelayCommand(execute => StopDistruct(), canExecute => CanStopDistruct());
            SelectLabelsAtopFileCommand = new RelayCommand(execute => SelectLabelsAtopFile());
            SelectLabelsBelowFileCommand = new RelayCommand(execute => SelectLabelsBelowFile());
            ConfigureColorPaletteCommand = new RelayCommand(execute => ConfigureColorPalette());
            ExportCommand = new AsyncRelayCommand(execute => Export(), canExecute => CanExport());
            GeneratePreviewCommand = new AsyncRelayCommand(execute => GeneratePreview(), canExecute => CanGeneratePreview());
            DetermineOptimalValuesCommand = new RelayCommand(execute => DetermineOptimalValues());
            SelectClustPermFileCommand = new RelayCommand(execute => SelectClustPermFile());

            _windowService = windowService;


            _distructInteractionService = new DistructInteractionService(directoryService, fileService, _logger);

            _distructInteractionService.ProgressChanged += value =>
            {
                if (_distructCompleted || DistructStopped) return;
                if (value >= 100) return;

                UIDispatcherHelper.RunOnUI(() =>
                {
                    if (_distructCompleted || DistructStopped) return;
                    DistructProgress = value;
                    DistructProgressText = $"[{_savedConfigurationName}] In progress... {value:F0}%";
                });
            };
        }

        private void OnLimitedValuesChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null) return;

            if (e.PropertyName == nameof(PredefinedCLUMPPKStart))
                KFrom = PredefinedCLUMPPKStart;

            if (e.PropertyName == nameof(PredefinedCLUMPPKEnd))
                KTo = PredefinedCLUMPPKEnd;

            if (e.PropertyName == nameof(PredefinedIndvCount))
                NUMINDS = PredefinedIndvCount;

            if (e.PropertyName == nameof(PredefinedPopCount))
                NUMPOPS = PredefinedPopCount;
        }
        private void ValidateKRange()
        {
            var args = (KFrom, KTo, int.Max(2, PredefinedStructureKStart), PredefinedStructureKEnd);
            ValidateProperty(args, _kRangeValidator.Validate, nameof(KFrom));
            ValidateProperty(args, _kRangeValidator.Validate, nameof(KTo));
        }

        #region Configuration parameters properties
        public string ConfigurationName
        {
            get => _parametersName;
            set 
            { 
                if (SetField(ref _parametersName, value))
                    _parameterNameValidator.Validate(value);
            }
        }
        public string INFILE_LABEL_ATOP //
        {
            get => _infile_label_atop;
            set 
            { 
                if (SetField(ref _infile_label_atop, value))
                    _pathValidator.Validate(value);
            }
        }
        public string INFILE_LABEL_BELOW //
        {
            get => _infile_label_below;
            set 
            { 
                if (SetField(ref _infile_label_below, value))
                    _pathValidator.Validate(value);
            }
        }
        public string INFILE_CLUST_PERM //
        {
            get => _infile_clust_perm;
            set { SetField(ref _infile_clust_perm, value); }
        }

        private ObservableCollection<ClusterColorItem> _clusterColorItems = new();

        public int KFrom
        {
            get => _kFrom;
            set
            {
                if (SetField(ref _kFrom, value))
                {
                    ValidateKRange();
                    if (!HasErrorsFor(nameof(KFrom)) && !HasErrorsFor(nameof(KTo)))
                        RebuildKForPreviewItems();
                }
            }
        }
        public int KTo
        {
            get => _kTo;
            set
            {
                if (SetField(ref _kTo, value))
                {
                    ValidateKRange();
                    if (!HasErrorsFor(nameof(KFrom)) && !HasErrorsFor(nameof(KTo)))
                        RebuildKForPreviewItems();
                }
            }
        }

        public int NUMPOPS//
        {
            get => _numpops;
            set { SetField(ref _numpops, value); }
        }
        public int NUMINDS//
        {
            get => _numinds;
            set { SetField(ref _numinds, value); }
        }
        public bool PRINT_INDIVS//
        {
            get => _print_indivs;
            set { SetField(ref _print_indivs, value); }
        }
        public bool PRINT_LABEL_ATOP //
        {
            get => _print_label_atop;
            set { SetField(ref _print_label_atop, value); }
        }
        public bool PRINT_LABEL_BELOW //
        {
            get => _print_label_below;
            set { SetField(ref _print_label_below, value); }
        }
        public bool PRINT_SEP //
        {
            get => _print_sep;
            set { SetField(ref _print_sep, value); }
        }
        public double FONTHEIGHT //
        {
            get => _fontheight;
            set { SetField(ref _fontheight, value); }
        }
        public double DIST_ABOVE //
        {
            get => _dist_above;
            set { SetField(ref _dist_above, value); }
        }
        public double DIST_BELOW//
        {
            get => _dist_below;
            set { SetField(ref _dist_below, value); }
        }
        public double BOXHEIGHT //
        {
            get => _boxheight;
            set { SetField(ref _boxheight, value); }
        }
        public double INDIVWIDTH //
        {
            get => _indivwidth;
            set { SetField(ref _indivwidth, value); }
        }

        public Dictionary<int, string> OrientationOptions => _orientationOptions;//
        public int SelectedOrientation//
        {
            get => _selectedOrientation;
            set
            {
                if (SetField(ref _selectedOrientation, value))
                {
                    OnPropertyChanged(nameof(PrintInfileNameEnabled));

                    if (value != 0) PRINT_INFILE_NAME = false;
                }
            }
        }

        public double XORIGIN //
        {
            get => _xorigin;
            set { SetField(ref _xorigin, value); }
        }
        public double YORIGIN //
        {
            get => _yorigin;
            set { SetField(ref _yorigin, value); }
        }
        public double XSCALE //
        {
            get => _xscale;
            set { SetField(ref _xscale, value); }
        }
        public double YSCALE//
        {
            get => _yscale;
            set { SetField(ref _yscale, value); }
        }
        public double ANGLE_LABEL_ATOP//
        {
            get => _angle_label_atop;
            set { SetField(ref _angle_label_atop, value); }
        }
        public double ANGLE_LABEL_BELOW//
        {
            get => _angle_label_below;
            set { SetField(ref _angle_label_below, value); }
        }
        public double LINEWIDTH_RIM//
        {
            get => _linewidth_rim;
            set { SetField(ref _linewidth_rim, value); }
        }
        public double LINEWIDTH_SEP//
        {
            get => _linewidth_sep;
            set { SetField(ref _linewidth_sep, value); }
        }
        public double LINEWIDTH_IND //
        {
            get => _linewidth_ind;
            set { SetField(ref _linewidth_ind, value); }
        }
        public bool GRAYSCALE //
        {
            get => _grayscale;
            set { SetField(ref _grayscale, value); }
        }
        public bool ECHO_DATA//
        {
            get => _echo_data;
            set { SetField(ref _echo_data, value); }
        }
        public bool REPRINT_DATA //
        {
            get => _reprint_data;
            set { SetField(ref _reprint_data, value); }
        }
        public bool PRINT_INFILE_NAME //
        {
            get => _print_infile_name;
            set { SetField(ref _print_infile_name, value); }
        }
        public bool PRINT_COLOR_BREWER//
        {
            get => _print_color_brewer;
            set { SetField(ref _print_color_brewer, value); }
        }
        #endregion

        #region Commands properties
        public ICommand SaveChangesAsyncCommand { get; }
        public AsyncRelayCommand StartDistructAsyncCommand { get; }
        public RelayCommand StopDistructCommand { get; }
        public ICommand SelectLabelsAtopFileCommand { get; }
        public ICommand SelectLabelsBelowFileCommand { get; }
        public ICommand ConfigureColorPaletteCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand GeneratePreviewCommand { get; }
        public ICommand DetermineOptimalValuesCommand { get; }
        public ICommand SelectClustPermFileCommand { get; }

        #endregion

        #region Enable properties
        public bool PrintInfileNameEnabled => SelectedOrientation == 0;
        #endregion

        private void DetermineOptimalValues()
        {
            double plotWidth = NUMINDS * INDIVWIDTH;
            double margin = 28;

            switch (SelectedOrientation)
            {
                case 0: // rotate 0 — вправо и вверх
                    XORIGIN = margin;
                    YORIGIN = margin;
                    break;

                case 1: // rotate 90 — вверх и влево
                    XORIGIN = BOXHEIGHT + margin;
                    YORIGIN = margin;
                    break;

                case 2: // rotate 180 — влево и вниз
                    XORIGIN = plotWidth + margin;
                    YORIGIN = BOXHEIGHT + margin;
                    break;

                case 3: // rotate 270 — вниз и вправо
                    XORIGIN = margin;
                    YORIGIN = plotWidth + margin;
                    break;
            }
        }

        #region Additional parameters properties
        public bool IsPDF
        {
            get => _isPDF;
            set { SetField(ref _isPDF, value); }
        }
        public bool IsPNG
        {
            get => _isPNG;
            set { SetField(ref _isPNG, value); }
        }
        public bool IsJPEG
        {
            get => _isJPEG;
            set { SetField(ref _isJPEG, value); }
        }
        public bool IsBMP
        {
            get => _isBMP;
            set { SetField(ref _isBMP, value); }
        }
        public int SelectedKForExport
        {
            get => _selectedKForExport;
            set { SetField(ref _selectedKForExport, value); }
        }
        public List<int> KForExportItems
        {
            get => _kForExportItems;
            set { SetField(ref _kForExportItems, value); }
        }

        public int SelectedKForPreview
        {
            get => _selectedKForPreview;
            set { SetField(ref _selectedKForPreview, value); }
        }
        public List<int> KForPreviewItems
        {
            get => _kForPreviewItems;
            set { SetField(ref _kForPreviewItems, value); }
        }
        #endregion

        #region Rebuilders methods
        private void ResetParameters()
        {
            var newConfiguration = new DistructConfigurationModel();

            ConfigurationName = string.Empty;
            SetConfigurationParameters(newConfiguration);

            NUMINDS = PredefinedIndvCount;
            NUMPOPS = PredefinedPopCount;

            KFrom = PredefinedCLUMPPKStart;
            KTo = PredefinedCLUMPPKEnd;
        }
        private void RebuildConfigurationParametersItems()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                ConfigurationParametersItems.Clear();

                foreach (DistructConfigurationModel item in _savedConfigurationParametersItems)
                    ConfigurationParametersItems.Add(item);

                ConfigurationParametersItems.Add(CreateNewSetPlaceholder);
            });
        }
        private void RebuildKForExportItems(int kFrom, int kTo)
        {
            List<int> items = new();
            for (int i = kFrom; i <= kTo; i++)
                items.Add(i);

            UIDispatcherHelper.RunOnUI(() =>
            {
                KForExportItems = items;
                if (!KForExportItems.Contains(SelectedKForExport))
                    SelectedKForExport = KForExportItems.FirstOrDefault();
            });
        }
        private void RebuildKForPreviewItems()
        {
            List<int> items = new();
            for (int i = KFrom; i <= KTo; i++)
                items.Add(i);
            UIDispatcherHelper.RunOnUI(() =>
            {
                KForPreviewItems = items;
                if (!KForPreviewItems.Contains(SelectedKForPreview))
                    SelectedKForPreview = KForPreviewItems.FirstOrDefault();
            });
        }
        #endregion

        #region Preview region
        private BitmapImage? _previewImage;
        public BitmapImage? PreviewImage
        {
            get => _previewImage;
            set
            {
                _previewImage = value;
                OnPropertyChanged(nameof(PreviewImage));
            }
        }

        private async Task GeneratePreview()
        {
            if (CurrentSet == null || CurrentCLUMPPConfigurationModel == null) return;

            var configurationName = _savedConfigurationName;
            var clumppConfigurationName = CurrentCLUMPPConfigurationModel.ParametersName;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            try
            {
                _isPreviewing = true;
                PreviewImage = null;
                BitmapImage btmImage = new();

                btmImage = await _distructInteractionService.GeneratePreviewForKAsync(fullCurrentSetFolderPath, clumppConfigurationName, configurationName, SelectedKForPreview, INFILE_CLUST_PERM, _clusterColorItems, GRAYSCALE);

                PreviewImage = btmImage;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _messageService.ShowError($"An error occurred while generating the preview image: {ex.Message}");
            }
            finally
            {
                _isPreviewing = false;
            }
        }
        private bool CanGeneratePreview()
        {
            return CurrentSet != null &&
                   CurrentCLUMPPConfigurationModel != null &&
                   SelectedConfigurationParameters != null &&
                   _wasSaved &&
                   !_isPreviewing;
        }
        #endregion

        #region Export region
        private async Task Export()
        {
            if (CurrentSet == null || CurrentCLUMPPConfigurationModel == null) return;

            var configurationName = _savedConfigurationName;
            var clumppConfigurationName = CurrentCLUMPPConfigurationModel.ParametersName;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            var (format, k) = GetExportOptions();
            if (format == DistructConstants.OutputFormat.None) return;

            var configuration = _changesTracker.GetSnapshot();
            if (configuration == null) return;

            try
            {
                _isExporting = true;
                await _distructInteractionService.ExportResultsAsync(fullCurrentSetFolderPath, clumppConfigurationName, configurationName, format, k, configuration.ORIENTATION);

                _messageService.ShowInformation($"Results for K={k} were successfully exported in selected format(s).");

                var exportedFolderPath = Path.Combine(fullCurrentSetFolderPath, DistructConstants.DISTRUCT_FOLDER_NAME, clumppConfigurationName, configurationName, DistructConstants.DISTRUCT_RESULTS_FOLDER_NAME, "Exported");
                Process.Start("explorer.exe", $"/select,\"{exportedFolderPath}\"");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _isExporting = false;
            }
        }
        private bool CanExport()
        {
            return CurrentSet != null &&
                   CurrentCLUMPPConfigurationModel != null &&
                   SelectedConfigurationParameters != null &&
                   _distructCompleted &&
                   (IsPDF || IsPNG || IsJPEG || IsBMP) &&
                   !_isExporting;
        }
        private (DistructConstants.OutputFormat format, int k) GetExportOptions()
        {
            DistructConstants.OutputFormat format = DistructConstants.OutputFormat.None;
            if (IsPDF) format |= DistructConstants.OutputFormat.Pdf;
            if (IsPNG) format |= DistructConstants.OutputFormat.Png;
            if (IsJPEG) format |= DistructConstants.OutputFormat.Jpeg;
            if (IsBMP) format |= DistructConstants.OutputFormat.Bmp;

            return (format, SelectedKForExport);
        }
        #endregion

        private void SelectLabelsAtopFile()
        {
            INFILE_LABEL_ATOP = _dialogService.SelectFile(PathConstants.DEFAULT_DOCUMENTS_PATH);
        }
        private void SelectLabelsBelowFile()
        {
            INFILE_LABEL_BELOW = _dialogService.SelectFile(PathConstants.DEFAULT_DOCUMENTS_PATH);
        }
        private void SelectClustPermFile()
        {
            INFILE_CLUST_PERM = _dialogService.SelectFile(PathConstants.DEFAULT_DOCUMENTS_PATH);
        }

        private void ConfigureColorPalette()
        {
            DistructColorsConfigurationVM distructColorsConfigurationVM = new(KTo, GRAYSCALE, _windowService);

            bool? configurationResult = _windowService.ShowDialogWindow<DistructColorsConfigurationWindow, DistructColorsConfigurationVM>(distructColorsConfigurationVM);

            if (configurationResult == true)
            {
                _clusterColorItems = distructColorsConfigurationVM.Items;
            }
        }

        #region ComboBox of configuration parameters
        private DistructConfigurationModel? _selectedConfigurationParameters = new();
        private ObservableCollection<DistructConfigurationModel> _savedConfigurationParametersItems = new();

        public DistructConfigurationModel? SelectedConfigurationParameters
        {
            get => _selectedConfigurationParameters;
            set
            {
                if (SetField(ref _selectedConfigurationParameters, value))
                {
                    if (value == CreateNewSetPlaceholder)
                    {
                        if (!_isCreatingNewConfiguration) ResetParameters();

                        _isCreatingNewConfiguration = true;
                        _wasSaved = false;
                        ConfigurationName = string.Empty;
                    }
                    else if (value != null)
                    {
                        if (!IsValidDistructConfiguration(value))
                        {
                            _messageService.ShowWarning($"Distruct configuration folder with name \"{value.ParametersName}\" was not found.");

                            _savedConfigurationParametersItems.Remove(value);
                            RebuildConfigurationParametersItems();
                            return;
                        }

                        _ = LoadSelectedDistructConfigurationAsync(value);
                    }
                }
            }
        }
        private bool IsValidDistructConfiguration(DistructConfigurationModel configuration)
        {
            if (CurrentSet == null || CurrentCLUMPPConfigurationModel == null) return false;
            var setName = CurrentSet.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);
            return _distructInteractionService.IsConfigurationExist(fullSetFolderPath, CurrentCLUMPPConfigurationModel.ParametersName, configuration.ParametersName) && !_directoryService.IsDirectoryEmpty(Path.Combine(fullSetFolderPath, DistructConstants.DISTRUCT_FOLDER_NAME, CurrentCLUMPPConfigurationModel.ParametersName, configuration.ParametersName));
        }
        public ObservableCollection<DistructConfigurationModel> ConfigurationParametersItems { get; } = new();
        public static readonly DistructConfigurationModel CreateNewSetPlaceholder = new() { ParametersName = "Create new" };
        #endregion

        #region Progress parameters
        private double _distructProgress;
        public double DistructProgress
        {
            get => _distructProgress;
            set { SetField(ref _distructProgress, value); }
        }

        private string _distructProgressText = "Not started";
        public string DistructProgressText
        {
            get => _distructProgressText;
            set { SetField(ref _distructProgressText, value); }
        }

        private bool _distructStopped;
        public bool DistructStopped
        {
            get => _distructStopped;
            set { SetField(ref _distructStopped, value); }
        }
        private bool _distructCompleted;
        public bool DistructCompleted
        {
            get => _distructCompleted;
            set { SetField(ref _distructCompleted, value); }
        }
        #endregion

        #region Load parameters methods
        protected override async Task LoadSelectedSetParametersAsync(SetModel? set)
        {
            return;
        }
        protected override async Task LoadSelectedCLUMPPConfigurationAsync(CLUMPPConfigurationModel? configuration)
        {
            if (configuration == null || CurrentSet == null || !CurrentSet.IsDistructProcessed || !configuration.IsProcessed || !configuration.HasPopResults) return;

            var setName = CurrentSet.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var configurations = await _distructInteractionService.LoadConfigurationsListAsync(fullSetFolderPath, configuration.ParametersName);

                _savedConfigurationParametersItems.Clear();

                foreach (var config in configurations)
                    _savedConfigurationParametersItems.Add(config);

                RebuildConfigurationParametersItems();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"An error occurred while loading Distruct configurations: {ex.Message}");
            }
        }
        private async Task LoadSelectedDistructConfigurationAsync(DistructConfigurationModel? configuration)
        {
            if (configuration == null || CurrentSet == null || CurrentCLUMPPConfigurationModel == null) return;

            var setName = CurrentSet.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var (configurationModel, kFrom, kTo) = await _distructInteractionService.LoadConfigurationAsync(fullSetFolderPath, CurrentCLUMPPConfigurationModel.ParametersName, configuration.ParametersName);

                SetConfigurationParameters(configurationModel);
                _savedConfigurationName = configurationModel.ParametersName;

                _isCreatingNewConfiguration = false;
                _wasSaved = true;

                _changesTracker.TakeModelSnapshot(configurationModel);

                if (kFrom == 0 || kTo == 0)
                {
                    KFrom = 2;
                    KTo = 3;

                    UIDispatcherHelper.RunOnUI(() =>
                    {
                        DistructProgressText = $"[{_savedConfigurationName}] Not started";
                        DistructProgress = 0;
                    });
                    DistructStopped = false;
                    _distructCompleted = false;
                    return;
                }

                KFrom = kFrom;
                KTo = kTo;
                UIDispatcherHelper.RunOnUI(() =>
                {
                    DistructProgress = 100;
                    DistructProgressText = $"[{_savedConfigurationName}] Completed";
                });
                DistructStopped = false;
                _distructCompleted = true;
                RebuildKForExportItems(KFrom, KTo);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"An error occurred while loading Distruct configuration with name \"{configuration.ParametersName}\": {ex.Message}");
            }
        }
        #endregion

        #region Save parameters methods
        private async Task SaveChangesAsync()
        {
            var configuration = GetConfigurationParameters();

            if (CurrentSet == null ||
                CurrentCLUMPPConfigurationModel == null ||
                HasErrors ||
                string.IsNullOrEmpty(configuration.ParametersName))
                return;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            var clumppConfigurationName = CurrentCLUMPPConfigurationModel.ParametersName;

            bool isNameChanged = !_isCreatingNewConfiguration && _savedConfigurationName != configuration.ParametersName;
            if ((_isCreatingNewConfiguration || isNameChanged) && _distructInteractionService.IsConfigurationExist(fullCurrentSetFolderPath, clumppConfigurationName, configuration.ParametersName))
            {
                _messageService.ShowWarning($"Configuration with name \"{configuration.ParametersName}\" already exists. Please choose a different name.");
                return;
            }

            try
            {
                if (configuration.NUMPOPS != PredefinedPopCount)
                {
                    var result = _messageService.ShowExpandedQuetion("Changing value of populations may cause Distruct to malfunction. Are you sure you want to proceed with the current value? (“No” - restore default value and continue.)");

                    if (result == false)
                    {
                        configuration.NUMPOPS = PredefinedPopCount;
                        NUMPOPS = PredefinedPopCount;
                    }
                    else if (result == null) return;
                }
                if (configuration.NUMINDS != PredefinedIndvCount)
                {
                    var result = _messageService.ShowExpandedQuetion("Changing value of individuals may cause Distruct to malfunction. Are you sure you want to proceed with the current value? (“No” - restore default value and continue.)");

                    if (result == false)
                    {
                        configuration.NUMINDS = PredefinedIndvCount;
                        NUMINDS = PredefinedIndvCount;
                    }
                    else if (result == null) return;
                }

                _wasSaved = false;

                if (_isCreatingNewConfiguration)
                {
                    await CreateNewConfigurationAsync(configuration, clumppConfigurationName, fullCurrentSetFolderPath);
                }
                else
                {
                    bool shouldBeSaved = await UpdateExistingConfigurationAsync(configuration, clumppConfigurationName, fullCurrentSetFolderPath);

                    if (!shouldBeSaved) return;
                }

                _wasSaved = true;
                _savedConfigurationName = configuration.ParametersName;
                _isCreatingNewConfiguration = false;
                _changesTracker.TakeModelSnapshot(configuration);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"An error occurred while saving the configuration \"{configuration.ParametersName}\": {ex.Message}");
            }
            finally
            {
                StartDistructAsyncCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanSaveChanges()
        {
            var configurationParameters = GetConfigurationParameters();

            return CurrentSet != null &&
                   CurrentCLUMPPConfigurationModel != null &&
                   !HasErrors &&
                   !string.IsNullOrEmpty(configurationParameters.ParametersName) &&
                   _changesTracker.HasChanges(configurationParameters);
        }
        private async Task CreateNewConfigurationAsync(DistructConfigurationModel configuration, string clumppConfigurationName, string fullCurrentSetFolderPath)
        {
            try
            {
                _distructInteractionService.PrepareDistructDirectory(fullCurrentSetFolderPath);

                await _distructInteractionService.PrepareConfiguration(fullCurrentSetFolderPath, clumppConfigurationName, configuration, INFILE_CLUST_PERM);

                _savedConfigurationParametersItems.Add(configuration);
                RebuildConfigurationParametersItems();

                SetField(ref _selectedConfigurationParameters, configuration, nameof(SelectedConfigurationParameters));

                _messageService.ShowInformation($"Configuration \"{configuration.ParametersName}\" was successfully created.");
            }
            catch (Exception) { throw; }
        }
        private async Task<bool> UpdateExistingConfigurationAsync(DistructConfigurationModel configuration, string clumppConfigurationName, string fullCurrentSetFolderPath)
        {
            if (CurrentSet == null) return false;

            if (CurrentCLUMPPConfigurationModel == null) return false;

            if (!_distructInteractionService.IsConfigurationExist(fullCurrentSetFolderPath, clumppConfigurationName, _savedConfigurationName))
            {
                _messageService.ShowError($"Configuration with name \"{_savedConfigurationName}\" was not found. Unable to save changes.");
                return false;
            }

            try
            {
                if (_savedConfigurationName != configuration.ParametersName)
                {
                    await _distructInteractionService.RenameConfiguration(fullCurrentSetFolderPath, clumppConfigurationName, _savedConfigurationName, configuration.ParametersName);
                }

                if (_changesTracker.HasChanges(configuration))
                {
                    await _distructInteractionService.PrepareConfiguration(fullCurrentSetFolderPath, clumppConfigurationName, configuration, INFILE_CLUST_PERM);
                }

                return true;
            }
            catch (Exception) { throw; }
        }
        #endregion

        #region Distruct execution methods
        private async Task StartDistructAsync()
        {
            if (CurrentSet == null ||
                CurrentCLUMPPConfigurationModel == null ||
                !_wasSaved ||
                _distructInteractionService.IsRunning ||
                SelectedConfigurationParameters == null ||
                HasErrorsFor(nameof(KFrom)) ||
                HasErrorsFor(nameof(KTo)))
                return;

            int kFrom = KFrom;
            int kTo = KTo;
            var configurationName = _savedConfigurationName;
            var clumppConfigurationName = CurrentCLUMPPConfigurationModel.ParametersName;

            var setName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                _distructCompleted = false;
                DistructStopped = false;

                DistructProgress = 0;
                DistructProgressText = $"[{_savedConfigurationName}] In progress... 0%";

                await _distructInteractionService.StartExecution(configurationName, kFrom, kTo, fullCurrentSetFolderPath, clumppConfigurationName, INFILE_CLUST_PERM, _clusterColorItems, GRAYSCALE, _coresCount);
                _distructCompleted = true;

                WorkflowState.MarkProcessedAndRefreshStage(CurrentSet, ProcessingStage);
                await _setConfigurationService.SaveConfigFileAsync(fullCurrentSetFolderPath, CurrentSet);
                RebuildKForExportItems(KFrom, KTo);

                DistructProgress = 100;
                DistructProgressText = $"[{_savedConfigurationName}] Completed";
            }
            catch (OperationCanceledException)
            {
                DistructProgressText = $"[{_savedConfigurationName}] Stopped at {DistructProgress:F0}%";
            }
            catch (Exception ex)
            {
                DistructStopped = true;
                _messageService.ShowError($"An error occurred while running Distruct for set {setName}. {ex.Message}. See logs for details.");
                DistructProgressText = $"[{setName}] Stopped by error at {DistructProgress:F0}%";
            }
            finally
            {
                StopDistructCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanStartDistruct()
        {
            return CurrentSet != null &&
                   CurrentCLUMPPConfigurationModel != null &&
                   _wasSaved &&
                   !_distructInteractionService.IsRunning &&
                   SelectedConfigurationParameters != null &&
                   !HasErrorsFor(nameof(KFrom)) &&
                   !HasErrorsFor(nameof(KTo));
        }

        private void StopDistruct()
        {
            DistructStopped = true;
            DistructProgressText = $"[{_savedConfigurationName}] Stopping...";
            _distructInteractionService.StopExecution();
        }
        private bool CanStopDistruct()
        {
            return _distructInteractionService.IsRunning;
        }
        #endregion

        private DistructConfigurationModel GetConfigurationParameters()
        {
            return new DistructConfigurationModel
            {
                ParametersName = ConfigurationName,
                INFILE_LABEL_ATOP = INFILE_LABEL_ATOP,
                INFILE_LABEL_BELOW = INFILE_LABEL_BELOW,
                NUMPOPS = NUMPOPS,
                NUMINDS = NUMINDS,
                PRINT_INDIVS = PRINT_INDIVS,
                PRINT_LABEL_ATOP = PRINT_LABEL_ATOP,
                PRINT_LABEL_BELOW = PRINT_LABEL_BELOW,
                PRINT_SEP = PRINT_SEP,
                FONTHEIGHT = FONTHEIGHT,
                DIST_ABOVE = DIST_ABOVE,
                DIST_BELOW = DIST_BELOW,
                BOXHEIGHT = BOXHEIGHT,
                INDIVWIDTH = INDIVWIDTH,
                ORIENTATION = SelectedOrientation,
                XORIGIN = XORIGIN,
                YORIGIN = YORIGIN,
                XSCALE = XSCALE,
                YSCALE = YSCALE,
                ANGLE_LABEL_ATOP = ANGLE_LABEL_ATOP,
                ANGLE_LABEL_BELOW = ANGLE_LABEL_BELOW,
                LINEWIDTH_RIM = LINEWIDTH_RIM,
                LINEWIDTH_SEP = LINEWIDTH_SEP,
                LINEWIDTH_IND = LINEWIDTH_IND,
                GRAYSCALE = GRAYSCALE,
                ECHO_DATA = ECHO_DATA,
                REPRINT_DATA = REPRINT_DATA,
                PRINT_INFILE_NAME = PRINT_INFILE_NAME,
                PRINT_COLOR_BREWER = PRINT_COLOR_BREWER
            };
        }
        private void SetConfigurationParameters(DistructConfigurationModel configuration)
        {
            ConfigurationName = configuration.ParametersName;
            INFILE_LABEL_ATOP = configuration.INFILE_LABEL_ATOP;
            INFILE_LABEL_BELOW = configuration.INFILE_LABEL_BELOW;
            NUMPOPS = configuration.NUMPOPS;
            NUMINDS = configuration.NUMINDS;
            PRINT_INDIVS = configuration.PRINT_INDIVS;
            PRINT_LABEL_ATOP = configuration.PRINT_LABEL_ATOP;
            PRINT_LABEL_BELOW = configuration.PRINT_LABEL_BELOW;
            PRINT_SEP = configuration.PRINT_SEP;
            FONTHEIGHT = configuration.FONTHEIGHT;
            DIST_ABOVE = configuration.DIST_ABOVE;
            DIST_BELOW = configuration.DIST_BELOW;
            BOXHEIGHT = configuration.BOXHEIGHT;
            INDIVWIDTH = configuration.INDIVWIDTH;
            SelectedOrientation = configuration.ORIENTATION;
            XORIGIN = configuration.XORIGIN;
            YORIGIN = configuration.YORIGIN;
            XSCALE = configuration.XSCALE;
            YSCALE = configuration.YSCALE;
            ANGLE_LABEL_ATOP = configuration.ANGLE_LABEL_ATOP;
            ANGLE_LABEL_BELOW = configuration.ANGLE_LABEL_BELOW;
            LINEWIDTH_RIM = configuration.LINEWIDTH_RIM;
            LINEWIDTH_SEP = configuration.LINEWIDTH_SEP;
            LINEWIDTH_IND = configuration.LINEWIDTH_IND;
            GRAYSCALE = configuration.GRAYSCALE;
            ECHO_DATA = configuration.ECHO_DATA;
            REPRINT_DATA = configuration.REPRINT_DATA;
            PRINT_INFILE_NAME = configuration.PRINT_INFILE_NAME;
            PRINT_COLOR_BREWER = configuration.PRINT_COLOR_BREWER;
        }
    }
}
