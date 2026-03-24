using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Project;
using System.IO;

namespace GenotypeApplication.Services.Application_configuration
{
    public class SetConfigurationService
    {
        private readonly string SET_CONFIGURATION_FILE_EXTENSION = AppConstants.CONFIGURATION_FILES_EXTENSION;

        private readonly IDirectoryService _directoryService = new DirectoryService();
        private readonly IFileService _fileService = new FileService();

        public void Create(string fullSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSetFolderPath);

            Directory.CreateDirectory(fullSetFolderPath);
        }

        public async Task SaveConfigFileAsync(string fullSetFolderPath, SetModel setModel)
        {
            string setConfigFileName = Path.ChangeExtension(setModel.Name, SET_CONFIGURATION_FILE_EXTENSION);

            string fullSetConfigFilePath = Path.Combine(fullSetFolderPath, setConfigFileName);

            await _fileService.WriteJsonAsync(setModel, fullSetConfigFilePath);
        }

        public void Rename(string fullSavedSetFolderPath, string fullNewSetFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSavedSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullNewSetFolderPath);

            Directory.Move(fullSavedSetFolderPath, fullNewSetFolderPath);
        }

        public async Task UpdateConfigFileAsync(string fullSavedSetFolderPath, string fullNewSetFolderPath, SetModel newSetModel)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullSavedSetFolderPath);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fullNewSetFolderPath);
            ArgumentNullException.ThrowIfNull(newSetModel);

            var fullSavedSetConfigFilePath = Path.Combine(
                fullNewSetFolderPath,
                Path.ChangeExtension(Path.GetFileName(fullSavedSetFolderPath), SET_CONFIGURATION_FILE_EXTENSION)
                );

            if (!File.Exists(fullSavedSetConfigFilePath)) throw new FileNotFoundException();

            var fullNewSetConfigFilePath = Path.Combine(
                fullNewSetFolderPath,
                 Path.ChangeExtension(Path.GetFileName(fullNewSetFolderPath), SET_CONFIGURATION_FILE_EXTENSION)
                 );

            if (File.Exists(fullNewSetConfigFilePath)) throw new InvalidOperationException("The configuration file with the new set name already exists.");

            File.Move(fullSavedSetConfigFilePath, fullNewSetConfigFilePath);

            await SaveConfigFileAsync(fullNewSetFolderPath, newSetModel);
        }

        public bool IsSetExist(string fullSetFolderPath)
        {
            return _directoryService.IsDirectoryExist(fullSetFolderPath);
        }
    }
}
