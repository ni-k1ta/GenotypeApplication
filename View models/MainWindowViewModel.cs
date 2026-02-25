using GenotypeApplication.Application_windows;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;
using System.Windows;
using System.Windows.Input;

namespace GenotypeApplication.View_models
{
    public class MainWindowViewModel : ViewModelErrors, IWindowAware
    {
        private readonly IWindowService _windowService;
        private readonly IDialogService _dialogService;
        private readonly IValidator<string> _pathTextValidator;

        private StructureMainParametersModel _structureMainParametersModel;

        private WeakReference<Window>? _currentWindowRef;

        public MainWindowViewModel(StructureMainParametersModel structureMainParametersModel, IDialogService dialogService, IValidator<string> pathTextValidator, IWindowService windowService)
        {
            _structureMainParametersModel = structureMainParametersModel;

            LoadDataFileCommand = new RelayCommand(execute => LoadDataFile());

            _dialogService = dialogService;
            _windowService = windowService;
            _pathTextValidator = pathTextValidator;
        }

        public ICommand LoadDataFileCommand { get; }

        private void LoadDataFile()
        {
            DataFileFormatViewModel dataFileParametersViewModel = new(_dialogService, _pathTextValidator, _windowService);

            dataFileParametersViewModel.SetCurrentWindow(_windowService.ShowDialogWindow<LoadDataFileWindow, DataFileFormatViewModel>(dataFileParametersViewModel));
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
