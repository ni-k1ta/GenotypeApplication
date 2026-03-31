using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Project;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Set;
using System.Windows;

namespace GenotypeApplication.View_models
{
    public class MainWindowVM : ViewModelErrors, IWindowAware
    {
        //private string _strutureDataFileName;

        private readonly WorkflowStateModel _workflowStateModel;

        private readonly IWindowService _windowService;
        private readonly IMessageService _messageService;
        private readonly IDialogService _dialogService;
        private readonly IValidator<string> _pathTextValidator;


        private ProjectParametersModel _projectModel;
        private readonly string _projectFolderFullPath;
        //private string _dataFileFullPath;
        //private DataFileFormatModel _dataFileFormatModel;

        private WeakReference<Window>? _currentWindowRef;

        public MainWindowVM(ProjectParametersModel projectModel, string fullProjectFolderPath, DataFileFormatModel dataFileFormatModel, int coresCount, string dataFileFullPath, IDirectoryService directoryService, IFileService fileService, IDialogService dialogService, IMessageService messageService, IValidator<string> pathTextValidator, IWindowService windowService)
        {
            //_strutureDataFileName = string.Empty;

            _workflowStateModel = new();

            _projectModel = projectModel;
            _projectFolderFullPath = fullProjectFolderPath;
            //_dataFileFullPath = dataFileFullPath;
            //_dataFileFormatModel = dataFileFormatModel;

            //LoadDataFileCommand = new RelayCommand(execute => LoadDataFile());

            _dialogService = dialogService;
            _messageService = messageService;
            _windowService = windowService;
            _pathTextValidator = pathTextValidator;

            //////////////////////////////////////////////////////////////
            var structureMainParams = new StructureMainParametersModel();
            var structureExtraParams = new StructureExtraParametersModel();
            var setService = new SetConfigurationService();
            //изменить ^^^

            StructureTabControlVM = new(_workflowStateModel, fullProjectFolderPath, coresCount, dataFileFormatModel, dataFileFullPath, structureMainParams, structureExtraParams, setService, dialogService, directoryService, fileService, messageService, pathTextValidator, windowService);
            StructureHarvesterTabControlVM = new(_workflowStateModel, coresCount, fullProjectFolderPath, dialogService, directoryService, fileService, messageService);
            CLUMPPTabControlVM = new(_workflowStateModel, coresCount, fullProjectFolderPath, dialogService, directoryService, fileService, messageService);
            DistructTabControlVM = new(_workflowStateModel, coresCount, fullProjectFolderPath, dialogService, directoryService, fileService, messageService);
        }

        public StructureTabControlVM StructureTabControlVM { get; private set; }
        public StructureHarvesterTabControlVM StructureHarvesterTabControlVM { get; private set; }
        public CLUMPPTabControlVM CLUMPPTabControlVM { get; private set; }
        public DistructTabControlVM DistructTabControlVM { get; private set; }

        // организовать загрузку проекта => загрузку наборов Set

        //public string StrutureDataFileName
        //{
        //    get => _strutureDataFileName;
        //    set { SetField(ref _strutureDataFileName, value); }
        //}

        //public DataFileFormatModel DataFileFormatModel
        //{
        //    get => _dataFileFormatModel;
        //    set { SetField(ref _dataFileFormatModel, value); }
        //}
        //public string DataFileFullPath
        //{
        //    get => _dataFileFullPath;
        //    set { SetField(ref _dataFileFullPath, value); }
        //}

        //public ICommand LoadDataFileCommand { get; }

        //private void LoadDataFile()
        //{
        //    DataFileFormatViewModel dataFileParametersViewModel = new(_dialogService, _messageService, _pathTextValidator, _windowService);

        //    bool? loadResult = _windowService.ShowDialogWindow<LoadDataFileWindow, DataFileFormatViewModel>(dataFileParametersViewModel);

        //    if (loadResult == true)
        //    {
        //        DataFileFullPath = dataFileParametersViewModel.DataFileFullPath;
        //        DataFileFormatModel = dataFileParametersViewModel.DataFileFormatModel;

        //        StrutureDataFileName = Path.GetFileName(DataFileFullPath);
        //    }
        //}

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
