using GenotypeApplication.Models.Project;

namespace GenotypeApplication.Interfaces
{
    public interface IProjectService
    {
        Task CreateAsync(ProjectParametersModel projectModel);
        Task<ProjectParametersModel> LoadAsync(string fullProjectPath);
        Task SaveAsync(ProjectParametersModel projectModel);
        void Actualize(ProjectParametersModel newProjectModel, ProjectParametersModel oldProjectModel);
        void Remove(string fullProjectFolderPath);
        bool IsProjectExist(string fullProjectFolderPath);
    }
}
