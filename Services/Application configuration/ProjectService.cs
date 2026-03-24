using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Project;
using System.IO;

namespace GenotypeApplication.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string PROJECT_FILE_EXTENSION = AppConstants.PROJECT_FILE_EXTENSION;
        //private readonly string[] ADDITIONAL_PROGRAMS = AppConstants.ADDITIONAL_PROGRAMS;

        private readonly IDirectoryService _directoryService = new DirectoryService();
        private readonly IFileService _fileService = new FileService();

        public async Task CreateAsync(ProjectParametersModel projectModel)
        {
            ArgumentNullException.ThrowIfNull(projectModel);

            string fullProjectPath = Path.Combine(projectModel.Path, projectModel.Name);
            Directory.CreateDirectory(fullProjectPath);

            //foreach (var program in ADDITIONAL_PROGRAMS)
            //{
            //    string fullAdditionalProgramPath = Path.Combine(fullProjectPath, program);
            //    Directory.CreateDirectory(fullAdditionalProgramPath);
            //}

            await SaveAsync(projectModel);
        }

        public async Task<ProjectParametersModel> LoadAsync(string fullProjectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullProjectPath)) throw new OperationCanceledException();

                string projectName = Path.GetFileName(fullProjectPath);
                string projectConfigFileName = Path.ChangeExtension(projectName, PROJECT_FILE_EXTENSION);

                string fullProjectConfigFilePath = Path.Combine(fullProjectPath, projectConfigFileName);

                return await _fileService.ReadJsonAsync<ProjectParametersModel>(fullProjectConfigFilePath);
            }
            catch (FileNotFoundException fnfe)
            {
                throw new FileNotFoundException("Project configuration file not found.", fnfe.FileName);
            }
            catch (InvalidDataException)
            {
                throw new InvalidDataException("Invalid project configuration data.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveAsync(ProjectParametersModel projectModel)
        {
            string projectConfigFileName = Path.ChangeExtension(projectModel.Name, PROJECT_FILE_EXTENSION);

            string fullProjectConfigFilePath = Path.Combine(projectModel.Path, projectModel.Name, projectConfigFileName);

            await _fileService.WriteJsonAsync(projectModel, fullProjectConfigFilePath);
        }

        public void Actualize(ProjectParametersModel newProjectModel, ProjectParametersModel oldProjectModel)
        {
            ArgumentNullException.ThrowIfNull(newProjectModel);
            ArgumentNullException.ThrowIfNull(oldProjectModel);

            try
            {
                if (newProjectModel.Path != oldProjectModel.Path)
                {
                    string fullNewProjectPath = Path.Combine(newProjectModel.Path, oldProjectModel.Name);
                    string fullOldProjectPath = Path.Combine(oldProjectModel.Path, oldProjectModel.Name);

                    _directoryService.CopyDirectory(fullOldProjectPath, fullNewProjectPath);
                }

                if (newProjectModel.Name != oldProjectModel.Name)
                {
                    string fullOldProjectName = Path.Combine(newProjectModel.Path, oldProjectModel.Name);
                    string fullNewProjectName = Path.Combine(newProjectModel.Path, newProjectModel.Name);

                    Directory.Move(fullOldProjectName, fullNewProjectName);

                    string oldProjectConfigFileName = Path.ChangeExtension(oldProjectModel.Name, PROJECT_FILE_EXTENSION);

                    string fullOldProjectConfigFilePath = Path.Combine(fullNewProjectName, oldProjectConfigFileName);

                    if (File.Exists(fullOldProjectConfigFilePath))
                    {
                        string newProjectConfigFileName = Path.ChangeExtension(newProjectModel.Name, PROJECT_FILE_EXTENSION);

                        string fullNewProjectConfigFilePath = Path.Combine(fullNewProjectName, newProjectConfigFileName);

                        File.Move(fullOldProjectConfigFilePath, fullNewProjectConfigFilePath);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Remove(string fullProjectFolderPath)
        {
            if (!IsProjectExist(fullProjectFolderPath)) return;

            try
            {
                _directoryService.DeleteDirectory(fullProjectFolderPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool IsProjectExist(string fullProjectFolderPath)
        {
            return _directoryService.IsDirectoryExist(fullProjectFolderPath);
        }
    }
}
