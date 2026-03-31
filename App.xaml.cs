using GenotypeApplication.Application_windows;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.MVVM.Validation;
using GenotypeApplication.Services;
using GenotypeApplication.Services.MVVM;
using GenotypeApplication.Services.Project;
using GenotypeApplication.View_models;
using System.Windows;
using Application = System.Windows.Application;

namespace GenotypeApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IWindowService windowService = new WindowService();

            IDirectoryService directoryService = new DirectoryService();
            IFileService fileService = new FileService();

            IDialogService dialogService = new DialogService();
            IMessageService messageService = new MessageService();
            IRecentProjectsService recentProjectsService = new RecentProjectsService();

            var projectParametersViewModel = new ProjectParametersVM(directoryService, fileService, dialogService, messageService, recentProjectsService, new NameTextValidator(), new PathTextValidator(), windowService);

            projectParametersViewModel.SetCurrentWindow(windowService.ShowWindow<ProjectConfigurationWindow, ProjectParametersVM>(projectParametersViewModel));
        }
    }

}
