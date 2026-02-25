using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Interfaces
{
    public interface IFileService
    {
        Task<T> ReadJsonAsync<T>(string filePath);
        Task WriteJsonAsync<T>(T model, string path);
        IEnumerable<string> ReadFile(string filePath);
    }
}
