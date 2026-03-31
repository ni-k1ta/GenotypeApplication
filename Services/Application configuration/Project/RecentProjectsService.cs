using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Project;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace GenotypeApplication.Services.Project
{
    public class RecentProjectsService : IRecentProjectsService
    {
        private readonly string RECENT_PROJECTS_FILE_DEFAULT_PATH = PathConstants.RECENT_PROJECTS_FILE_DEFAULT_PATH;
        private const int MAX_RECENT_PROJECTS = AppConstants.MAX_RECENT_PROJECTS;

        private ObservableCollection<RecentProjectModel> _recentProjects;

        public RecentProjectsService()
        {
            _recentProjects = LoadFromFile();
        }

        public ObservableCollection<RecentProjectModel> GetRecentProjects()
        {
            return _recentProjects;
        }

        public void AddProject(ProjectParametersModel projectModel)
        {
            AddOrUpdateProject(projectModel);
        }

        public void UpdateProject(ProjectParametersModel oldProjectModel, ProjectParametersModel newProjectModel)
        {
            AddOrUpdateProject(newProjectModel, oldProjectModel);
        }

        private void AddOrUpdateProject(ProjectParametersModel projectModel, ProjectParametersModel? searchModel = null)
        {
            searchModel ??= projectModel;

            var existing = _recentProjects.FirstOrDefault(p =>
                string.Equals(Path.Combine(p.Path, p.Name),
                Path.Combine(searchModel.Path, searchModel.Name),
                StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                _recentProjects.Remove(existing);

            _recentProjects.Insert(0, RecentProjectModel.FromProjectParametersModel(projectModel));

            while (_recentProjects.Count > MAX_RECENT_PROJECTS)
                _recentProjects.RemoveAt(_recentProjects.Count - 1);

            SaveToFile();
        }

        public void RemoveProject(RecentProjectModel recentProject)
        {
            var recentProjectModel = _recentProjects.FirstOrDefault(p =>
                string.Equals(Path.Combine(p.Path, p.Name),
                Path.Combine(recentProject.Path, recentProject.Name),
                StringComparison.OrdinalIgnoreCase));

            if (recentProjectModel != null)
            {
                if (!_recentProjects.Remove(recentProjectModel))
                    return;

                SaveToFile();
            }
        }

        private ObservableCollection<RecentProjectModel> LoadFromFile()
        {
            try
            {
                if (File.Exists(RECENT_PROJECTS_FILE_DEFAULT_PATH))
                {
                    var json = File.ReadAllText(RECENT_PROJECTS_FILE_DEFAULT_PATH);
                    var projects = JsonSerializer.Deserialize<List<RecentProjectModel>>(json);
                    return new ObservableCollection<RecentProjectModel>(projects ?? new List<RecentProjectModel>());
                }
            }
            catch (Exception ex)
            {
                //todo
                // Логирование ошибки
                Console.WriteLine($"Error loading recent projects: {ex.Message}");
            }

            return new ObservableCollection<RecentProjectModel>();
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_recentProjects, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(RECENT_PROJECTS_FILE_DEFAULT_PATH, json);
            }
            catch (Exception ex)
            {
                //todo
                // Логирование ошибки
                Console.WriteLine($"Error saving recent projects: {ex.Message}");
            }
        }
    }
}
