using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Interfaces
{
    public interface IDirectoryService
    {
        void CopyDirectory(string sourceDir, string targetDir);
        void DeleteDirectory(string dir);
        bool IsDirectoryExist(string directoryPath);
    }
}
