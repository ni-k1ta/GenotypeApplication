#define EXPERIMENTAL

namespace GenotypeApplication.Models.Project
{
    public class RecentProjectModel
    {
        private string _name = string.Empty;
        private string _path = string.Empty;
        private DateTime _lastModified;

        public string Name
        {
            get => _name;
#if EXPERIMENTAL
            set => _name = value;
#endif
        }
        public string Path
        {
            get => _path;
#if EXPERIMENTAL
            set => _path = value;
#endif
        }
        public DateTime LastModified
        {
            get => _lastModified;
#if EXPERIMENTAL
            set => _lastModified = value;
#endif
        }

        public static RecentProjectModel FromProjectParametersModel(ProjectParametersModel project)
        {
            return new RecentProjectModel
            {
                _name = project.Name,
                _path = project.Path,
                _lastModified = project.LastModified
            };
        }
    }
}
