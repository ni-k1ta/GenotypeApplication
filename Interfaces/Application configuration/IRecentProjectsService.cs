using GenotypeApplication.Models.Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Interfaces
{
    public interface IRecentProjectsService
    {
        ObservableCollection<RecentProjectModel> GetRecentProjects();
        void AddProject(ProjectParametersModel project);
        void UpdateProject(ProjectParametersModel oldProjectModel, ProjectParametersModel newProjectModel);
        void RemoveProject(RecentProjectModel recentProject);
    }
}
