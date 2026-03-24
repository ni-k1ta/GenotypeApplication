using GenotypeApplication.Interfaces;
using System.IO;

namespace GenotypeApplication.Services
{
    public class DirectoryService : IDirectoryService
    {
        public void CopyDirectory(string sourceDirectoryPath, string targetDirectoryPath)
        {
            Directory.CreateDirectory(targetDirectoryPath);

            foreach (var file in Directory.GetFiles(sourceDirectoryPath))
            {
                string destFile = Path.Combine(targetDirectoryPath, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDirectoryPath))
            {
                string destDir = Path.Combine(targetDirectoryPath, Path.GetFileName(directory));
                CopyDirectory(directory, destDir);
            }
        }
        public void DeleteDirectory(string directoryPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(directoryPath);

            if (!IsDirectoryExist(directoryPath)) return;

            // Удаляем файлы
            foreach (var file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            // Удаляем папки снизу вверх
            foreach (var dir in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories)
                         .OrderByDescending(d => d.Length))
            {
                try
                {
                    Directory.Delete(dir, false);
                }
                catch
                {
                }
            }

            try
            {
                Directory.Delete(directoryPath, false);
            }
            catch
            {
            }
        }
        public bool IsDirectoryExist(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
    }
}
