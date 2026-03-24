#define EXPERIMENTAL

using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.Project;
using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

//todo пару EXCEPTION и привести СТИЛЬ для окна к общему и всё это ПРОТЕСТИРОВАТЬ. Дальше нужно реализовать открытие следующего окна и двигаться по логике остального приложения

namespace GenotypeApplication.View_models
{
    public class ProjectParametersVM : ViewModelErrors, IWindowAware
    {
        private ProjectParametersModel _projectModel;

        private readonly IProjectService _projectService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IRecentProjectsService _recentProjectsService;
        private readonly IValidator<string> _nameTextValidator;
        private readonly IValidator<string> _pathTextValidator;
        private readonly IWindowService _windowService;

        private WeakReference<Window>? _currentWindowRef;

        private string _projectName;
        private string _projectPath;
        private bool _projectIsParallelEnabled;
        private int _projectCoresCount;
        private DateTime _projectCreatedAt;
        private DateTime _projectLastModified;

        private bool _isAllCores;
        private bool _isSelectionCores;
        private int _selectedCores;

        private readonly int _maxCores;
        private readonly int _minSelectedCores;
        private readonly int _maxSelectedCores;

        private RecentProjectModel _selectedRecentProject;

        private readonly string PROJECT_DEFAULT_PATH;

        private bool _isNewProject;
        private bool _isSaving;

        public ProjectParametersVM(IProjectService projectService, IDialogService dialogService, IMessageService messageService, IRecentProjectsService recentProjectsService, IValidator<string> nameTextValidator, IValidator<string> pathTextValidator, IWindowService windowService)
        {
            _projectModel = new();

            _projectService = projectService;
            _dialogService = dialogService;
            _messageService = messageService;
            _recentProjectsService = recentProjectsService;
            _nameTextValidator = nameTextValidator;
            _pathTextValidator = pathTextValidator;
            _windowService = windowService;

            _projectName = string.Empty;
            _projectPath = string.Empty;

            RecentProjects = _recentProjectsService.GetRecentProjects();

            ProjectIsParallelEnabled = false;
            IsAllCores = false;
            IsSelectionCores = true;
            SelectedCores = 1;

            _maxCores = AppConstants.MAX_CORES;
            _minSelectedCores = 1;
            _maxSelectedCores = _maxCores - 1;

            _selectedRecentProject = new();

            OpenProjectCommand = new AsyncRelayCommand(execute => OpenProjectAsync());
            SaveChangesCommand = new AsyncRelayCommand(execute => SaveChangesAsync(), canExecute => CanSaveChanges());
            ChangeProjectPathCommand = new RelayCommand(execute => ChangeProjectPath());
            RemoveRecentProjectCommand = new RelayCommand(RemoveRecentProject);
            OpenRecentProjectCommand = new AsyncRelayCommand(OpenRecentProjectAsync);
            DeleteProjectCommand = new RelayCommand(DeleteProject);
            ShowInExplorerCommand = new RelayCommand(ShowProjectInExplorer);

            PROJECT_DEFAULT_PATH = PathConstants.PROJECT_DEFAULT_PATH;

            _isNewProject = true;

#if EXPERIMENTAL
            AddTestData();
#endif
        }

        private void AddTestData()
        {
            RecentProjects.Add(new RecentProjectModel
            {
                Name = "MyProject.prj",
                Path = @"C:\Projects\MyProject",
                LastModified = DateTime.Now.AddHours(-2)
            });

            RecentProjects.Add(new RecentProjectModel
            {
                Name = "TestProject.prj",
                Path = @"D:\Work\TestProject",
                LastModified = DateTime.Now.AddDays(-1)
            });

            RecentProjects.Add(new RecentProjectModel
            {
                Name = "Demo.prj",
                Path = @"C:\Users\UserName\Documents\Demo",
                LastModified = DateTime.Now.AddDays(-3)
            });
            RecentProjects.Add(new RecentProjectModel
            {
                Name = "Demo.prj",
                Path = @"C:\Users\UserName\Documents\Demo",
                LastModified = DateTime.Now.AddDays(-3)
            });
            RecentProjects.Add(new RecentProjectModel
            {
                Name = "Demo.prj",
                Path = @"C:\Users\UserName\Documents\Demo",
                LastModified = DateTime.Now.AddDays(-3)
            });
            RecentProjects.Add(new RecentProjectModel
            {
                Name = "Demo.prj",
                Path = @"C:\Users\UserName\Documents\Demo",
                LastModified = DateTime.Now.AddDays(-3)
            });
        }


        public string ProjectName
        {

            get => _projectName;
            set
            {
                if (SetField(ref _projectName, value))
                {
                    ValidateProperty(value, _nameTextValidator.Validate);

                    if (string.IsNullOrWhiteSpace(_projectPath)) ProjectPath = PROJECT_DEFAULT_PATH;
                }
            }
        }
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (SetField(ref _projectPath, value))
                    ValidateProperty(value, _pathTextValidator.Validate);
            }
        }
        public bool ProjectIsParallelEnabled
        {
            get => _projectIsParallelEnabled;
            set
            {
                if (SetField(ref _projectIsParallelEnabled, value))
                {
                    if (value)
                    {
                        IsAllCores = true;
                    }
                    else
                    {
                        IsSelectionCores = true;
                        SelectedCores = 1;
                    }
                }
            }
        }
        public int ProjectCoresCount
        {
            get => _projectCoresCount;
            private set => SetField(ref _projectCoresCount, value);
        }
        public DateTime ProjectCreatedAt
        {
            get => _projectCreatedAt;
            set => SetField(ref _projectCreatedAt, value);
        }
        public DateTime ProjectLastModified
        {
            get => _projectLastModified;
            set => SetField(ref _projectLastModified, value);
        }
        public bool IsAllCores
        {
            get => _isAllCores;
            set
            {
                if (SetField(ref _isAllCores, value))
                    if (value) ProjectCoresCount = _maxCores;
            }
        }
        public bool IsSelectionCores
        {
            get => _isSelectionCores;
            set
            {
                if (SetField(ref _isSelectionCores, value))
                {
                    if (value)
                        if (_projectCoresCount != _selectedCores) ProjectCoresCount = SelectedCores;
                }
            }
        }
        public int SelectedCores
        {
            get => _selectedCores;
            set
            {
                if (SetField(ref _selectedCores, value))
                    ProjectCoresCount = value;
            }
        }
        public int MinSelectedCores => _minSelectedCores;
        public int MaxSelectedCores => _maxSelectedCores;
        public RecentProjectModel SelectedRecentProject 
        { 
            get => _selectedRecentProject;
            set => SetField(ref _selectedRecentProject, value);
        }

        public ICommand OpenProjectCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand ChangeProjectPathCommand { get; }
        public ICommand RemoveRecentProjectCommand { get; }
        public ICommand OpenRecentProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand ShowInExplorerCommand { get; }

        private void RemoveRecentProject(object? parameter)
        {
            if (parameter is RecentProjectModel recentProject) _recentProjectsService.RemoveProject(recentProject);
        }
        private async Task OpenRecentProjectAsync(object? parameter)
        {
            if (parameter is RecentProjectModel recentProject)
            {
                string fullProjectFolderPath = Path.Combine(recentProject.Path, recentProject.Name);

                if (!_projectService.IsProjectExist(fullProjectFolderPath))
                {
                    _messageService.ShowWarning("The selected project was not found. It may have been deleted or modified.");
                    return;
                }

                await LoadProjectAsync(fullProjectFolderPath);
            }
        }
        private void DeleteProject(object? parameter)
        {
            if (parameter is RecentProjectModel recentProject)
            {
                if (!_messageService.ShowQuetion($"Are you sure you want to delete the project \"{recentProject.Name}\"?")) return;

                string fullProjectFolderPath = Path.Combine(recentProject.Path, recentProject.Name);

                if (!_projectService.IsProjectExist(fullProjectFolderPath)) _messageService.ShowWarning("The selected project was not found. It may have been deleted or modified.");

                try
                {
                    _projectService.Remove(fullProjectFolderPath);
                }
                catch (Exception)
                {
                    //todo
                    throw;
                }
                finally { RemoveRecentProject(recentProject); }
            }
        }

        public ObservableCollection<RecentProjectModel> RecentProjects { get; }

        private async Task OpenProjectAsync()
        {
            string fullProjectFolderPath = _dialogService.SelectFolder(PROJECT_DEFAULT_PATH);
            if (string.IsNullOrWhiteSpace(fullProjectFolderPath)) return;

            await LoadProjectAsync(fullProjectFolderPath);
        }
        private async Task LoadProjectAsync(string fullProjectFolderPath)
        {
            try
            {
                _projectModel = await _projectService.LoadAsync(fullProjectFolderPath);
                if (_projectModel == null)
                {
                    _messageService.ShowError("Invalid project configuration data.");
                    return;
                }

                string fullProjectLoadedFolerPath = Path.Combine(_projectModel.Path, _projectModel.Name);

                if (!string.Equals(fullProjectFolderPath, fullProjectLoadedFolerPath)) ProjectPath = fullProjectFolderPath;
                else ProjectPath = _projectModel.Path;

                ProjectName = _projectModel.Name;
                ProjectIsParallelEnabled = _projectModel.IsParallelEnabled;
                ProjectCreatedAt = _projectModel.CreatedAt;
                ProjectLastModified = _projectModel.LastModified;

                if (ProjectIsParallelEnabled)
                {
                    if (_projectModel.CoresCount == _maxCores)
                    {
                        IsAllCores = true;
                    }
                    else
                    {
                        SelectedCores = _projectModel.CoresCount;
                        IsSelectionCores = true;
                    }
                }
                else SelectedCores = 1;

                _isNewProject = false;
            }
            catch (InvalidDataException ide)
            {
                _messageService.ShowError(ide.Message);
                return;
            }
            catch (FileNotFoundException fnfe)
            {
                _messageService.ShowError(fnfe.Message);
                return;
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                _messageService.ShowError($"Unexpected error when attempting to load the project configuration file: {ex.Message}");
                return;
            }
        }
        private void ShowProjectInExplorer(object? parameter)
        {
            if (parameter is RecentProjectModel project)
            {
                var fullProjectFolderPath = Path.Combine(project.Path, project.Name);

                if (!_projectService.IsProjectExist(fullProjectFolderPath))
                {
                    _messageService.ShowWarning("The selected project was not found. It may have been deleted or modified.");
                    return;
                }

                Process.Start("explorer.exe", $"/select,\"{fullProjectFolderPath}\"");
            }
        }

        private async Task SaveChangesAsync()
        {
            if (!CanSaveChanges()) return;

            var projectName = _projectName;
            var projectPath = _projectPath;
            var isParallelEnabled = _projectIsParallelEnabled;
            var coresCount = _projectCoresCount;

            try
            {
                _isSaving = true;

                if (_isNewProject) await CreateProjectAsync(projectName, projectPath, isParallelEnabled, coresCount);
                else await UpdateProjectAsync(projectName, projectPath, isParallelEnabled, coresCount);
            }
            catch (Exception)
            {
                //todo
                throw;
            }
            finally { _isSaving = false; }

            //todo загрузку установленных параметров для программ, если проект не новый
            DataFileFormatModel dataFileFormatModel = new();
            string filePath = string.Empty;
            //изменить ^^^

            string fullProjectFolderPath = Path.Combine(projectPath, projectName);

            MainWindowVM mainWindowViewModel = new(_projectModel, fullProjectFolderPath, dataFileFormatModel, _projectModel.CoresCount, filePath, _dialogService, _messageService, _pathTextValidator, _windowService);

            var mainWindow = _windowService.ShowWindow<MainWindow, MainWindowVM>(mainWindowViewModel);
            mainWindowViewModel.SetCurrentWindow(mainWindow);

            Application.Current.MainWindow = mainWindow;

            if (_currentWindowRef != null && _currentWindowRef.TryGetTarget(out var window))
            {
                _windowService.CloseWindow(window);
            }
        }
        private async Task CreateProjectAsync(string projectName, string projectPath, bool isParallelEnabled, int coresCount)
        {
            string fullProjectFolderPath = Path.Combine(projectPath, projectName);
            if (_projectService.IsProjectExist(fullProjectFolderPath))
            {
                _messageService.ShowWarning("A project with this name already exists.");
                return;
            }

            var projectModel = ProjectParametersModel.Create(projectName, projectPath, isParallelEnabled, coresCount);
            await _projectService.CreateAsync(projectModel);

            _projectModel = projectModel;
            ProjectCreatedAt = _projectModel.CreatedAt;
            ProjectLastModified = _projectModel.LastModified;

            if (ProjectIsParallelEnabled)
            {
                if (_projectModel.CoresCount == _maxCores)
                {
                    IsAllCores = true;
                }
                else
                {
                    SelectedCores = _projectModel.CoresCount;
                    IsSelectionCores = true;
                }
            }
            else SelectedCores = 1;

                _isNewProject = false;

            _recentProjectsService.AddProject(_projectModel);
        }
        private async Task UpdateProjectAsync(string projectName, string projectPath, bool isParallelEnabled, int coresCount)
        {
            ProjectParametersModel oldProjectModel = _projectModel;

            if (oldProjectModel.Name != projectName || oldProjectModel.Path != projectPath)
            {
                string fullProjectFolderNewPath = Path.Combine(projectPath, projectName);

                if (_projectService.IsProjectExist(fullProjectFolderNewPath))
                {
                    _messageService.ShowWarning("A project with this name already exists.");
                    return;
                }
            }

            try
            {
                _projectModel.Update(projectName, projectPath, isParallelEnabled, coresCount);
                _projectService.Actualize(_projectModel, oldProjectModel);

                await _projectService.SaveAsync(_projectModel);

                ProjectLastModified = _projectModel.LastModified;

                _recentProjectsService.UpdateProject(oldProjectModel, _projectModel);
            }
            catch (Exception)
            {
                _projectModel = oldProjectModel;
                throw;
            }
        }

        private void ChangeProjectPath()
        {
            string projectFolderPath = _dialogService.SelectFolder(PROJECT_DEFAULT_PATH);
            if (string.IsNullOrWhiteSpace(projectFolderPath)) return;

            ProjectPath = projectFolderPath;
        }

        private bool CanSaveChanges()
        {
            return !_isSaving && !HasErrors && !string.IsNullOrWhiteSpace(_projectName) && !string.IsNullOrWhiteSpace(_projectPath);
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindowRef = new WeakReference<Window>(window);
        }
    }
}
