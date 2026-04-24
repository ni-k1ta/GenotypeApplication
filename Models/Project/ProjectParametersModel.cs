namespace GenotypeApplication.Models.Project
{
    public class ProjectParametersModel
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsParallelEnabled { get; set; }
        public int CoresCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }

        public ProjectParametersModel() { }

        public ProjectParametersModel(ProjectParametersModel other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Name = other.Name;
            Path = other.Path;
            IsParallelEnabled = other.IsParallelEnabled;
            CoresCount = other.CoresCount;
            CreatedAt = other.CreatedAt;
            LastModified = other.LastModified;
        }

        public static ProjectParametersModel Create(string name, string path, bool isParallelEnabled, int coresCount)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Project name can't be empty string!");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Project path can't be empty string!");
            if (coresCount < 1 || coresCount > Environment.ProcessorCount) throw new ArgumentException($"The parallel project execution configuration cannot contain a number of cores beyond the available number of cores (current value: {coresCount}).");

            return new ProjectParametersModel
            {
                Name = name,
                Path = path,
                IsParallelEnabled = isParallelEnabled,
                CoresCount = coresCount,
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now
            };
        }

        public void Update(string name, string path, bool isParallelEnabled, int coresCount)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Project name can't be empty string!");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Project path can't be empty string!");
            if (coresCount < 1 || coresCount > Environment.ProcessorCount) throw new ArgumentException($"The parallel project execution configuration cannot contain a number of cores beyond the available number of cores (current value: {coresCount}).");

            Name = name;
            Path = path;
            IsParallelEnabled = isParallelEnabled;
            CoresCount = coresCount;
            LastModified = DateTime.Now;
        }
    }
}
