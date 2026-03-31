using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.View_models.External_programs_tabs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class DistructTabControlVM : ExternalProgramTabVMBase
    {
        private bool _isCreatingNewConfiguration;
        private string _savedConfigurationName;
        private bool _wasSaved;

        #region Configuration parameters
        private string _parametersName;

        //private string _infile_popq = string.Empty;
        //private string _infile_indivq = string.Empty;
        private string _infile_label_atop = string.Empty;
        private string _infile_label_below = string.Empty;
        private string _infile_clust_perm = string.Empty;
        //private string _outfile = string.Empty;

        private int _kFrom;
        private int _kTo;
        //private int _k;

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
        //private int _orientation;

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

        private DistructInteractionService _distructInteractionService;

        public DistructTabControlVM(WorkflowStateModel workflowState, int coresCount, string fullProjectFolderPath, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService) : base(workflowState, SetProcessingStage.Distruct, coresCount, fullProjectFolderPath, directoryService, fileService, messageService, dialogService)
        {

            _distructInteractionService = new DistructInteractionService(directoryService, fileService);

            SaveChangesAsyncCommand = new AsyncRelayCommand(execute => SaveChangesAsync(), canExecute => CanSaveChanges());
            StartDistructAsyncCommand = new AsyncRelayCommand(execute => StartDistructAsync(), canExecute => CanStartDistruct());
            StopDistructCommand = new RelayCommand(execute => StopDistruct(), canExecute => CanStopDistruct());


            _isCreatingNewConfiguration = true;

            RebuildConfigurationParametersItems();
            SelectedConfigurationParameters = ConfigurationParametersItems.LastOrDefault();
        }

        #region Configuration parameters properties
        //public string INFILE_POPQ
        //{
        //    get => _infile_popq;
        //    set { SetField(ref _infile_popq, value); }
        //}
        //public string INFILE_INDIVQ
        //{
        //    get => _infile_indivq;
        //    set { SetField(ref _infile_indivq, value); }
        //}
        public string INFILE_LABEL_ATOP //
        {
            get => _infile_label_atop;
            set { SetField(ref _infile_label_atop, value); }
        }
        public string INFILE_LABEL_BELOW //
        {
            get => _infile_label_below;
            set { SetField(ref _infile_label_below, value); }
        }
        public string INFILE_CLUST_PERM //
        {
            get => _infile_clust_perm;
            set { SetField(ref _infile_clust_perm, value); }
        }
        //public string OUTFILE
        //{
        //    get => _outfile;
        //    set { SetField(ref _outfile, value); }
        //}

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
        //public int K
        //{
        //    get => _k;
        //    set { SetField(ref _k, value); }
        //}

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
            set { SetField(ref _selectedOrientation, value); }
        }
        //public int ORIENTATION
        //{
        //    get => _orientation;
        //    set { SetField(ref _orientation, value); }
        //}
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

        private DistructConfigurationModel? _selectedConfigurationParameters = new();
        private ObservableCollection<DistructConfigurationModel> _savedConfigurationParametersItems = new();
        #endregion

        #region Commands properties
        public ICommand SaveChangesAsyncCommand { get; }
        public AsyncRelayCommand StartDistructAsyncCommand { get; }
        public RelayCommand StopDistructCommand { get; }
        #endregion

        public string ParametersName
        {
            get => _parametersName;
            set { SetField(ref _parametersName, value); }
        }
        public DistructConfigurationModel? SelectedConfigurationParameters
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
        public ObservableCollection<DistructConfigurationModel> ConfigurationParametersItems { get; } = new();
        public static readonly DistructConfigurationModel CreateNewSetPlaceholder = new() { ParametersName = "Create new" };

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

        protected override void LoadSelectedSetParameters(SetModel? set)
        {
           
        }


        private async Task SaveChangesAsync()
        {
            if (CurrentSet == null) return;
            var newParametersName = ParametersName;
            if (string.IsNullOrWhiteSpace(newParametersName)) return;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            _distructInteractionService.PrepareDistructDirectory(fullCurrentSetFolderPath);

            bool isNameChanged = !_isCreatingNewConfiguration && _savedConfigurationName != newParametersName;
            if ((_isCreatingNewConfiguration || isNameChanged) && _distructInteractionService.IsConfigurationExist(fullCurrentSetFolderPath, newParametersName))
            {
                _messageService.ShowWarning($"Configuration with name \"{newParametersName}\" already exists. Please choose a different name.");
                return;
            }

            try
            {
                _wasSaved = false;

                var configurationParameters = GetConfigurationParameters();

                if (_isCreatingNewConfiguration)
                {
                    await CreateNewConfigurationAsync(configurationParameters, fullCurrentSetFolderPath);
                }
                else
                {
                    bool shouldBeSaved = await UpdateExistingConfigurationAsync();

                    if (!shouldBeSaved) return;
                }

                _wasSaved = true;
                _savedConfigurationName = newParametersName;
                _isCreatingNewConfiguration = false;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                StartDistructAsyncCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanSaveChanges()
        {
            return true;
        }
        private async Task CreateNewConfigurationAsync(DistructConfigurationModel configurationModel, string fullCurrentSetFolderPath)
        {
            await _distructInteractionService.PrepareConfiguration(fullCurrentSetFolderPath, configurationModel);

            _savedConfigurationParametersItems.Add(configurationModel);
            RebuildConfigurationParametersItems();
            SelectedConfigurationParameters = configurationModel;

            _messageService.ShowInformation($"Configuration \"{configurationModel.ParametersName}\" was successfully created.");
        }
        private async Task<bool> UpdateExistingConfigurationAsync()
        {
            return false;
        }


        private async Task StartDistructAsync()
        {
            if (!_wasSaved)
                return;

            if (CurrentSet == null) return;
            if (SelectedConfigurationParameters == null) return;

            int kFrom = KFrom;
            int kTo = KTo;
            var configurationName = _savedConfigurationName;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                StopDistructCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanStartDistruct()
        {
            return true;
        }

        private void StopDistruct()
        {
        }
        private bool CanStopDistruct()
        {
            return _distructInteractionService.IsRunning;
        }

        private DistructConfigurationModel GetConfigurationParameters()
        {
            return new DistructConfigurationModel
            {
                ParametersName = ParametersName,
                INFILE_LABEL_ATOP = INFILE_LABEL_ATOP,
                INFILE_LABEL_BELOW = INFILE_LABEL_BELOW,
                INFILE_CLUST_PERM = INFILE_CLUST_PERM,
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
            ParametersName = configuration.ParametersName;
            INFILE_LABEL_ATOP = configuration.INFILE_LABEL_ATOP;
            INFILE_LABEL_BELOW = configuration.INFILE_LABEL_BELOW;
            INFILE_CLUST_PERM = configuration.INFILE_CLUST_PERM;
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
