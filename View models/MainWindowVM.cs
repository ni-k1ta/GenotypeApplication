using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Project;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.MVVM.TreeView;
using GenotypeApplication.MVVM.Validation;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.Set;
using System.Windows;

namespace GenotypeApplication.View_models
{
    public class MainWindowVM : ViewModelErrors, IWindowAware
    {
        private readonly WorkflowStateModel _workflowStateModel;

        private readonly IWindowService _windowService;
        private readonly IMessageService _messageService;
        private readonly IDialogService _dialogService;
        private readonly SetConfigurationService _setService;
        private readonly IValidator<string> _pathValidator;
        private readonly IValidator<string> _parameterNameValidator;
        private readonly IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> _kRangeValidator;

        private ProjectParametersModel _projectModel;
        private readonly string _projectFolderFullPath;

        private WeakReference<Window>? _currentWindowRef;

        public LoggerService Logger { get; private set; }

        private ProjectExplorerViewModel _projectExplorer;

        public MainWindowVM(ProjectParametersModel projectModel, string fullProjectFolderPath, int coresCount, IDirectoryService directoryService, IFileService fileService, IDialogService dialogService, IMessageService messageService, IValidator<string> pathValidator, IValidator<string> parameterNameValidator, IWindowService windowService)
        {
            _projectExplorer = new() { ProjectPath = fullProjectFolderPath };
            _setService = new SetConfigurationService();

            _workflowStateModel = new(_projectExplorer);

            _projectModel = projectModel;
            _projectFolderFullPath = fullProjectFolderPath;

            _dialogService = dialogService;
            _messageService = messageService;
            _windowService = windowService;
            _pathValidator = pathValidator;
            _parameterNameValidator = parameterNameValidator;
            _kRangeValidator = new KRangeValidator();

            Logger = new() { ProjectPath = fullProjectFolderPath };

            StructureTabControlVM = new(_workflowStateModel, fullProjectFolderPath, coresCount, _setService, dialogService, directoryService, fileService, messageService, pathValidator, parameterNameValidator, windowService, Logger, _kRangeValidator);
            StructureHarvesterTabControlVM = new(_workflowStateModel, coresCount, fullProjectFolderPath, _setService, dialogService, directoryService, fileService, messageService, pathValidator, parameterNameValidator, Logger, _kRangeValidator);
            CLUMPPTabControlVM = new(_workflowStateModel, coresCount, fullProjectFolderPath, _setService, dialogService, directoryService, fileService, messageService, pathValidator, parameterNameValidator, Logger, _kRangeValidator);
            DistructTabControlVM = new(_workflowStateModel, coresCount, fullProjectFolderPath, _setService, dialogService, directoryService, fileService, messageService, pathValidator, parameterNameValidator, Logger, _kRangeValidator, windowService);

            _workflowStateModel.CanChangeActiveSet = () =>
                !StructureTabControlVM.IsRunning &&
                !StructureHarvesterTabControlVM.IsRunning &&
                !CLUMPPTabControlVM.IsRunning &&
                !DistructTabControlVM.IsRunning;

            _workflowStateModel.CanChangeActiveConfiguration = () =>
                !CLUMPPTabControlVM.IsRunning &&
                !DistructTabControlVM.IsRunning;

            _workflowStateModel.ActiveSetChangeBlocked += () =>
                _messageService.ShowWarning("Cannot switch configurations while processing is running.");
        }

        public ProjectExplorerViewModel ProjectExplorer { get => _projectExplorer; }

        public StructureTabControlVM StructureTabControlVM { get; private set; }
        public StructureHarvesterTabControlVM StructureHarvesterTabControlVM { get; private set; }
        public CLUMPPTabControlVM CLUMPPTabControlVM { get; private set; }
        public DistructTabControlVM DistructTabControlVM { get; private set; }

        public async Task LoadProjectSets()
        {
            try
            {
                var (setsList, allIsOk) = await _setService.LoadSetsList(_projectFolderFullPath);

                if (!allIsOk) _messageService.ShowWarning("No configuration files were found for some parameter sets. These parameter sets were skipped during the loading process.");

                _workflowStateModel.LoadSetModelsList(setsList);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"An error occurred while loading parameter sets: {ex.Message}");
            }
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
