using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.Application_configuration;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.Models.Structure.Data_file.Highlights;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services;
using GenotypeApplication.Services.Application_configuration;
using GenotypeApplication.Services.Application_configuration.Data_file;
using GenotypeApplication.Services.Parsers;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class DataFileFormatVM : ViewModelErrors, IWindowAware
    {
        private readonly string DATA_FILE_DEFAULT_PATH;

        private string _dataFileFullPath;

        private int _numIndsParam;
        private int _numLociParam;
        private int _ploidyParam;
        private int _missingParam;

        private bool _markerNamesParam;
        private bool _recessiveAllelesParam;
        private bool _notAmbiguousParam;
        private int _notAmbiguousValueParam;
        private bool _mapDistancesParam;
        private bool _phaseInfoParam;
        private bool _oneRowPerIndFileParam;

        private bool _labelParam;
        private bool _popDataParam;
        private bool _popFlagParam;
        private bool _locDataParam;
        private bool _phenotypeParam;
        private bool _extraColsParam;
        private int _extraColsCountParam;

        private DataTableModel? _parsedDataTable;
        private DataFileFormatModel _dataFileFormatModel;
        private DataFileFormatModel _calculatedDataFileFormatModel;

        private readonly IValidator<string> _pathTextValidator;

        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IDataTableService _dataTableParser;
        private readonly IDataFormatDetectionService _dataFormatDetectionService;
        private readonly IHighlightCalculationService _highlightCalculationService;
        private readonly IWindowService _windowService;

        private HighlightMapModel? _highlightMap;
        private bool _isLiveHighlightEnabled;

        private bool _isSaving;

        private CancellationTokenSource? _debounceCts;
        private CancellationTokenSource? _calculationCts;

        private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(300);

        private WeakReference<Window>? _currentWindowRef;

        public DataFileFormatVM(IDialogService dialogService, IMessageService messageService, IValidator<string> pathTextValidator, IWindowService windowService)
        {
            _dataFileFormatModel = new();
            _calculatedDataFileFormatModel = new();

            _pathTextValidator = pathTextValidator;

            _dialogService = dialogService;
            _messageService = messageService;
            _dataTableParser = new DataTableParser();
            _dataFormatDetectionService = new DataFormatDetectionService();
            _highlightCalculationService = new HighlightCalculationService();

            DATA_FILE_DEFAULT_PATH = PathConstants.DEFAULT_DOCUMENTS_PATH;

            _dataFileFullPath = string.Empty;

            _numIndsParam = 0;
            _numLociParam = 0;
            _ploidyParam = 0;
            _missingParam = 0;

            _markerNamesParam = false;
            _recessiveAllelesParam = false;
            _notAmbiguousParam = false;
            _mapDistancesParam = false;
            _phaseInfoParam = false;
            _oneRowPerIndFileParam = false;

            _labelParam = false;
            _popDataParam = false;
            _popFlagParam = false;
            _locDataParam = false;
            _phenotypeParam = false;
            _extraColsParam = false;
            _extraColsCountParam = 0;

            _isSaving = false;

            SelectDataFileAsyncCommand = new AsyncRelayCommand(execute => SelectDataFileAsync());
            SaveDataFileParametersAsyncCommand = new AsyncRelayCommand(execute => SaveDataFileParametersAsync(), canExecute => CanSaveDataFileParameters());

            _windowService = windowService;
        }

        public DataFileFormatModel DataFileFormatModel
        {
            get => _dataFileFormatModel;
            set { SetField(ref _dataFileFormatModel, value); }
        }

        public string DataFileFullPath
        {
            get => _dataFileFullPath;
            set
            {
                if (SetField(ref _dataFileFullPath, value))
                {
                    ValidateProperty(value, _pathTextValidator.Validate);
                    OnPropertyChanged(nameof(IsPathValid));
                }
            }
        }

        public int NumIndsParam
        {
            get => _numIndsParam;
            set
            {
                if (SetField(ref _numIndsParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public int NumLociParam
        {
            get => _numLociParam;
            set
            {
                if (SetField(ref _numLociParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public int PloidyParam
        {
            get => _ploidyParam;
            set
            {
                if (SetField(ref _ploidyParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public int MissingParam
        {
            get => _missingParam;
            set
            {
                if (SetField(ref _missingParam, value))
                {
                    RequestRecalculation();
                }
            }
        }

        public bool MarkerNamesParam
        {
            get => _markerNamesParam;
            set
            {
                if (SetField(ref _markerNamesParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool RecessiveAllelesParam
        {
            get => _recessiveAllelesParam;
            set
            {
                if (SetField(ref _recessiveAllelesParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool NotAmbiguousParam
        {
            get => _notAmbiguousParam;
            set
            {
                if (SetField(ref _notAmbiguousParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public int NotAmbiguousValueParam
        {
            get => _notAmbiguousValueParam;
            set
            {
                if (SetField(ref _notAmbiguousValueParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool MapDistancesParam
        {
            get => _mapDistancesParam;
            set
            {
                if (SetField(ref _mapDistancesParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool PhaseInfoParam
        {
            get => _phaseInfoParam;
            set
            {
                if (SetField(ref _phaseInfoParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool OneRowPerIndFileParam
        {
            get => _oneRowPerIndFileParam;
            set
            {
                if (SetField(ref _oneRowPerIndFileParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool LabelParam
        {
            get => _labelParam;
            set
            {
                if (SetField(ref _labelParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool PopDataParam
        {
            get => _popDataParam;
            set
            {
                if (SetField(ref _popDataParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool PopFlagParam
        {
            get => _popFlagParam;
            set
            {
                if (SetField(ref _popFlagParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool LocDataParam
        {
            get => _locDataParam;
            set
            {
                if (SetField(ref _locDataParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool PhenotypeParam
        {
            get => _phenotypeParam;
            set
            {
                if (SetField(ref _phenotypeParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public bool ExtraColsParam
        {
            get => _extraColsParam;
            set
            {
                if (SetField(ref _extraColsParam, value))
                {
                    RequestRecalculation();
                }
            }
        }
        public int ExtraColsCountParam
        {
            get => _extraColsCountParam;
            set
            {
                if (SetField(ref _extraColsCountParam, value))
                {
                    RequestRecalculation();
                }
            }
        }

        private DataTableModel? ParsedDataTable
        {
            get => _parsedDataTable;
            set
            {
                if (SetField(ref _parsedDataTable, value)) OnPropertyChanged(nameof(DataTableSource));
            }
        }

        public DataTable? DataTableSource => ParsedDataTable?.RawData;

        public bool IsPathValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_dataFileFullPath) && !HasErrors;
            }
        }

        public HighlightMapModel? HighlightMap
        {
            get => _highlightMap;
            private set => SetField(ref _highlightMap, value);
        }

        //public bool IsLiveHighlightEnabled
        //{
        //    get => _isLiveHighlightEnabled;
        //    set
        //    {
        //        if (SetField(ref _isLiveHighlightEnabled, value))
        //        {
        //            // При включении автообновления — если были накоплены изменения, сразу запустить пересчёт, чтобы "догнать" текущее состояние
        //            if (value) RequestRecalculation();
        //        }
        //    }
        //}

        public ICommand SelectDataFileAsyncCommand { get; }
        private async Task SelectDataFileAsync()
        {
            string fullDataFilePath = _dialogService.SelectFile(DATA_FILE_DEFAULT_PATH);

            if (string.IsNullOrWhiteSpace(fullDataFilePath)) return;

            try
            {
                // Сбрасываем выделение до смены данных
                HighlightMap = null;
                _calculatedDataFileFormatModel = new();

                var parsedDataTable = _dataTableParser.Parse(fullDataFilePath);

                ParsedDataTable = new DataTableModel(parsedDataTable);

                _calculatedDataFileFormatModel = _dataFormatDetectionService.StartFormatDetection(ParsedDataTable);

                _isLiveHighlightEnabled = false;

                SetFormatValues(_calculatedDataFileFormatModel);

                await ExecuteCalculationAsync();

                _isLiveHighlightEnabled = true;
            }
            catch (FileNotFoundException) { }
            catch (InvalidDataException) { }
            catch (FormatException) { }
            catch (Exception)
            {
                //todo
                throw;
            }

            DataFileFullPath = fullDataFilePath;
        }

        private void SetFormatValues(DataFileFormatModel dataFileFormatModel)
        {
            NumIndsParam = dataFileFormatModel.NumInds;
            NumLociParam = dataFileFormatModel.NumLoci;
            PloidyParam = dataFileFormatModel.Ploidy;
            MissingParam = dataFileFormatModel.Missing;
            MarkerNamesParam = dataFileFormatModel.MarkerNames;
            RecessiveAllelesParam = dataFileFormatModel.RecessiveAlleles;

            if (RecessiveAllelesParam)
            {
                NotAmbiguousParam = dataFileFormatModel.NOTAMBIGUOUS;
                if (NotAmbiguousParam)
                    NotAmbiguousValueParam = dataFileFormatModel.NotAmbiguousValue;
            }

            MapDistancesParam = dataFileFormatModel.MapDistances;
            PhaseInfoParam = dataFileFormatModel.PHASEINFO;
            OneRowPerIndFileParam = dataFileFormatModel.OneRowPerInd;
            LabelParam = dataFileFormatModel.Label;
            PopDataParam = dataFileFormatModel.PopData;
            PopFlagParam = dataFileFormatModel.PopFlag;
            LocDataParam = dataFileFormatModel.LocData;
            PhenotypeParam = dataFileFormatModel.Phenotype;
            ExtraColsCountParam = dataFileFormatModel.ExtraCols;

            if (ExtraColsCountParam > 0) ExtraColsParam = true;
            else ExtraColsParam = false;

            DataFileFormatModel = dataFileFormatModel;
        }
        private DataFileFormatModel GetFormatValues()
        {
            var ExtraColsCount = ExtraColsParam ? ExtraColsCountParam : 0;
            var NotAmbiguousValue = NotAmbiguousValueParam == 0 ? -999 : NotAmbiguousValueParam;

            return new DataFileFormatModel
            {
                MarkerNames = MarkerNamesParam,
                RecessiveAlleles = RecessiveAllelesParam,
                MapDistances = MapDistancesParam,
                Label = LabelParam,
                PopData = PopDataParam,
                PopFlag = PopFlagParam,
                LocData = LocDataParam,
                Phenotype = PhenotypeParam,
                ExtraCols = ExtraColsCount,
                NumInds = NumIndsParam,
                NumLoci = NumLociParam,
                Ploidy = PloidyParam,
                OneRowPerInd = OneRowPerIndFileParam,
                PHASEINFO = PhaseInfoParam,
                Missing = MissingParam,
                NOTAMBIGUOUS = NotAmbiguousParam,
                NotAmbiguousValue = NotAmbiguousValue
            };
        }

        private async void RequestRecalculation()
        {
            if (!_isLiveHighlightEnabled) return;

            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var debounceToken = _debounceCts.Token;

            try
            {
                await Task.Delay(DebounceDelay, debounceToken);
            }
            catch (OperationCanceledException) { return; }

            await ExecuteCalculationAsync();
        }
        private async Task ExecuteCalculationAsync()
        {
            _calculationCts?.Cancel();
            _calculationCts = new CancellationTokenSource();
            var ct = _calculationCts.Token;

            DataFileFormatModel = GetFormatValues();

            try
            {
                var result = await Task.Run(() => _highlightCalculationService.Calculate(DataFileFormatModel, DataTableSource, ct), ct);

                if (!ct.IsCancellationRequested) HighlightMap = result;
            }
            catch (OperationCanceledException) { }
        }

        public ICommand SaveDataFileParametersAsyncCommand { get; }
        private async Task SaveDataFileParametersAsync()
        {
            if (!CanSaveDataFileParameters()) return;

            _isSaving = true;

            var dataFileFormatModel = GetFormatValues();

            if (!_dataFormatDetectionService.IsFormatMatchesWithData(ParsedDataTable, dataFileFormatModel))
            {
                _messageService.ShowWarning("The specified format parameters do not match the data. Please review your selections.");
                return;
            }
            else if (dataFileFormatModel != _calculatedDataFileFormatModel)
            {
                var result = _messageService.ShowQuetion("The specified format parameters differ from the automatically detected format. Do you want to proceed with the specified parameters?");

                if (!result)
                {
                    _isSaving = false;
                    return;
                }
            }

            DataFileFormatModel = dataFileFormatModel;
            _isSaving = false;

            if (_currentWindowRef != null && _currentWindowRef.TryGetTarget(out var window))
            {
                _windowService.CloseDialogWindow(window, true);
            }

        }
        private bool CanSaveDataFileParameters()
        {
            return !_isSaving && !HasErrors && !string.IsNullOrWhiteSpace(DataFileFullPath);
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
