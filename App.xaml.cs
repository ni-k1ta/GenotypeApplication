using GenotypeApplication.Application_windows;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.MVVM.Validation;
using GenotypeApplication.Services;
using GenotypeApplication.Services.MVVM;
using GenotypeApplication.Services.Project;
using GenotypeApplication.View_models;
using System.IO;
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

            var requiredFiles = new[]
            {
                "CLUMPP.exe",
                "distructWindows1.1.exe",
                "structure.exe",
                "structureHarvester.exe",
                "gswin64c.exe",
                "gsdll64.dll"
            };

            var externalDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External programs");
            if (!Directory.Exists(externalDir))
            {
                MessageBox.Show(
                    "Folder 'External programs' not found!",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
                return;
            }

            var missing = requiredFiles
                .Where(f => !File.Exists(Path.Combine(externalDir, f)))
                .ToList();

            if (missing.Count > 0)
            {
                MessageBox.Show(
                    $"Missing required files in 'External programs':\n\n{string.Join("\n", missing)}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
                return;
            }

            IWindowService windowService = new WindowService();

            IDirectoryService directoryService = new DirectoryService();
            IFileService fileService = new FileService();

            IDialogService dialogService = new DialogService();
            IMessageService messageService = new MessageService();

            RecentProjectsService recentProjectsService = new RecentProjectsService();

            var projectParametersViewModel = new ProjectParametersVM(directoryService, fileService, dialogService, messageService, recentProjectsService, new NameTextValidator(), new PathTextValidator(), windowService);

            projectParametersViewModel.SetCurrentWindow(windowService.ShowWindow<ProjectConfigurationWindow, ProjectParametersVM>(projectParametersViewModel));
        }

        public static CancellationTokenSource GlobalCts { get; } = new();

        protected override void OnExit(ExitEventArgs e)
        {
            GlobalCts.Cancel();
            GlobalCts.Dispose();
            base.OnExit(e);
        }
    }

}
