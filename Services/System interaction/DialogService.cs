using GenotypeApplication.Interfaces;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;

namespace GenotypeApplication.Services
{
    public class DialogService : IDialogService
    {
        public string SelectFolder(string initialDirectory)
        {
            using WinForms.FolderBrowserDialog dialog = new()
            {
                InitialDirectory = initialDirectory
            };

            return dialog.ShowDialog() == WinForms.DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }

        public string SelectFile(string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                Multiselect = false
            };

            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }
    }
}
