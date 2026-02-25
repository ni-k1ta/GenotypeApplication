using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.Application_configuration;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.Models.Structure.Data_file;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services;
using GenotypeApplication.Services.Application_configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class DataFileFormatViewModel : ViewModelErrors, IWindowAware
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

        private readonly IValidator<string> _pathTextValidator;

        private readonly IDialogService _dialogService;
        private readonly IDataTableService _dataTableService;
        private readonly IDataFormatDetectionService _dataFormatDetectionService;
        private readonly IWindowService _windowService;

        private WeakReference<Window>? _currentWindowRef;

        public DataFileFormatViewModel(IDialogService dialogService, IValidator<string> pathTextValidator, IWindowService windowService)
        {
            _pathTextValidator = pathTextValidator;

            _dialogService = dialogService;
            _dataTableService = new DataTableService();
            _dataFormatDetectionService = new DataFormatDetectionService();

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

            SelectDataFileCommand = new RelayCommand(execute => SelectDataFileAsync());
            _windowService = windowService;
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
            set { SetField(ref _numIndsParam, value); }
        }
        public int NumLociParam
        {
            get => _numLociParam;
            set { SetField(ref _numLociParam, value); }
        }
        public int PloidyParam
        {
            get => _ploidyParam;
            set { SetField(ref _ploidyParam, value); }
        }
        public int MissingParam
        {
            get => _missingParam;
            set { SetField(ref _missingParam, value); }
        }

        public bool MarkerNamesParam
        {
            get => _markerNamesParam;
            set { SetField(ref _markerNamesParam, value); }
        }
        public bool RecessiveAllelesParam
        {
            get => _recessiveAllelesParam;
            set { SetField(ref _recessiveAllelesParam, value); }
        }
        public bool NotAmbiguousParam
        {
            get => _notAmbiguousParam;
            set { SetField(ref _notAmbiguousParam, value); }
        }
        public int NotAmbiguousValueParam
        {
            get => _notAmbiguousValueParam;
            set { SetField(ref _notAmbiguousValueParam, value); }
        }
        public bool MapDistancesParam
        {
            get => _mapDistancesParam;
            set { SetField(ref _mapDistancesParam, value); }
        }
        public bool PhaseInfoParam
        {
            get => _phaseInfoParam;
            set { SetField(ref _phaseInfoParam, value); }
        }
        public bool OneRowPerIndFileParam
        {
            get => _oneRowPerIndFileParam;
            set { SetField(ref _oneRowPerIndFileParam, value); }
        }

        public bool LabelParam
        {
            get => _labelParam;
            set { SetField(ref _labelParam, value); }
        }
        public bool PopDataParam
        {
            get => _popDataParam;
            set { SetField(ref _popDataParam, value); }
        }
        public bool PopFlagParam
        {
            get => _popFlagParam;
            set { SetField(ref _popFlagParam, value); }
        }
        public bool LocDataParam
        {
            get => _locDataParam;
            set { SetField(ref _locDataParam, value); }
        }
        public bool PhenotypeParam
        {
            get => _phenotypeParam;
            set { SetField(ref _phenotypeParam, value); }
        }
        public bool ExtraColsParam
        {
            get => _extraColsParam;
            set { SetField(ref _extraColsParam, value); }
        }
        public int ExtraColsCountParam
        {
            get => _extraColsCountParam;
            set { SetField(ref _extraColsCountParam, value); }
        }

        /// <summary>
        /// Распарсенные данные файла.
        /// </summary>
        public DataTableModel? ParsedDataTable
        {
            get => _parsedDataTable;
            set
            {
                if (SetField(ref _parsedDataTable, value)) OnPropertyChanged(nameof(DataTableSource));
            }
        }
        /// <summary>
        /// DataTable для привязки к DataGrid.
        /// </summary>
        public DataTable? DataTableSource => ParsedDataTable?.RawData;

        public bool IsPathValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_dataFileFullPath) && !HasErrors;
            }
        }

        public ICommand SelectDataFileCommand { get; }
        private void SelectDataFileAsync()
        {
            string fullDataFilePath = _dialogService.SelectFile(DATA_FILE_DEFAULT_PATH);

            if (string.IsNullOrWhiteSpace(fullDataFilePath)) return;

            try
            {
                var parsedDataTable = _dataTableService.Load(fullDataFilePath);

                ParsedDataTable = new DataTableModel(parsedDataTable);

                var detectedFormat = _dataFormatDetectionService.StartParameterDetection(ParsedDataTable);
                SetFormatValues(detectedFormat);
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
            OneRowPerIndFileParam = dataFileFormatModel.OneRowPerInd == true;
            LabelParam = dataFileFormatModel.Label;
            PopDataParam = dataFileFormatModel.PopData;
            PopFlagParam = dataFileFormatModel.PopFlag;
            LocDataParam = dataFileFormatModel.LocData;
            PhenotypeParam = dataFileFormatModel.Phenotype;
            ExtraColsCountParam = dataFileFormatModel.ExtraCols;
            if (ExtraColsCountParam > 0)
                ExtraColsParam = true;
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
