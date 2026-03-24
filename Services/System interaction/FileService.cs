using GenotypeApplication.Interfaces;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GenotypeApplication.Services
{
    public class FileService : IFileService
    {
        public async Task<T> ReadJsonAsync<T>(string filePath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(filePath);
            if (!File.Exists(filePath)) throw new FileNotFoundException("JSON file was not found.", filePath);

            var json = await File.ReadAllTextAsync(filePath);
            var result = JsonSerializer.Deserialize<T>(json, GetJsonOptions());

            return result ?? throw new InvalidDataException("Invalid JSON file data.");
        }
        public async Task WriteJsonAsync<T>(T model, string path)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(model);

            var json = JsonSerializer.Serialize(model, GetJsonOptions());
            await File.WriteAllTextAsync(path, json); //перезаписывает файл
        }

        public IEnumerable<string> ReadFile(string filePath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(filePath);
            if (!File.Exists(filePath)) throw new FileNotFoundException("File was not found.", filePath);

            return File.ReadLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l));
        }

        public void CopyFile(string sourceFilePath, string targetFilePath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sourceFilePath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(targetFilePath);
            if (!File.Exists(sourceFilePath)) throw new FileNotFoundException("Source file was not found.", sourceFilePath);
            File.Copy(sourceFilePath, targetFilePath, overwrite: true);
        }

        public async Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines)
        {
            await File.WriteAllLinesAsync(filePath, lines, new UTF8Encoding(false)); //перезаписывает файл
        }

        public void DeleteFile(string filePath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(filePath);
            if (!File.Exists(filePath)) return;
            File.Delete(filePath);
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }
    }
}
