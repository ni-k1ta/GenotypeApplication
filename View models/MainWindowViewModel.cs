using GenotypeApplication.Application_windows;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Project;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;
using System.Windows;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class MainWindowViewModel : ViewModelErrors, IWindowAware
    {
        private readonly IWindowService _windowService;
        private readonly IMessageService _messageService;
        private readonly IDialogService _dialogService;
        private readonly IValidator<string> _pathTextValidator;


        private ProjectParametersModel _projectModel;
        private string _dataFileFullPath;
        private DataFileFormatModel _dataFileFormatModel;

        private WeakReference<Window>? _currentWindowRef;

        public MainWindowViewModel(ProjectParametersModel projectModel, DataFileFormatModel dataFileFormatModel, string dataFileFullPath, IDialogService dialogService, IMessageService messageService, IValidator<string> pathTextValidator, IWindowService windowService)
        {
            _projectModel = projectModel;
            _dataFileFullPath = dataFileFullPath;
            _dataFileFormatModel = dataFileFormatModel;

            LoadDataFileCommand = new RelayCommand(execute => LoadDataFile());

            _dialogService = dialogService;
            _messageService = messageService;
            _windowService = windowService;
            _pathTextValidator = pathTextValidator;
        }

        public DataFileFormatModel DataFileFormatModel
        {
            get => _dataFileFormatModel;
            set { SetField(ref _dataFileFormatModel, value); }
        }
        public string DataFileFullPath
        {
            get => _dataFileFullPath;
            set { SetField(ref _dataFileFullPath, value); }
        }

        public ICommand LoadDataFileCommand { get; }

        private void LoadDataFile()
        {
            DataFileFormatViewModel dataFileParametersViewModel = new(_dialogService, _messageService, _pathTextValidator, _windowService);

            bool? loadResult = _windowService.ShowDialogWindow<LoadDataFileWindow, DataFileFormatViewModel>(dataFileParametersViewModel);

            if (loadResult == true)
            {
                DataFileFullPath = dataFileParametersViewModel.DataFileFullPath;
                DataFileFormatModel = dataFileParametersViewModel.DataFileFormatModel;
            }
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
