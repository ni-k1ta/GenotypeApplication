using GenotypeApplication.Application_windows;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.MVVM.Validation;
using GenotypeApplication.Services;
using GenotypeApplication.Services.MVVM;
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

            IProjectService projectService = new ProjectService();
            IDialogService dialogService = new DialogService();
            IMessageService messageService = new MessageService();
            IRecentProjectsService recentProjectsService = new RecentProjectsService();

            var projectParametersViewModel = new ProjectParametersViewModel(projectService, dialogService, messageService, recentProjectsService, new NameTextValidator(), new PathTextValidator(), windowService);

            projectParametersViewModel.SetCurrentWindow(windowService.ShowWindow<ProjectConfigurationWindow, ProjectParametersViewModel>(projectParametersViewModel));
        }
    }

}
