using GenotypeApplication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Project
{
    public class ProjectParametersModel
    {
        public string Name { get; private set; } = string.Empty;
        public string Path { get; private set; } = string.Empty;
        public bool IsParallelEnabled { get; private set; }
        public int CoresCount { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; private set; }

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
