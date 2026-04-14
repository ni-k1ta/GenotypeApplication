using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;

namespace GenotypeApplication.MVVM.TreeView
{
    public class FileNodeViewModel : ViewModelBase
    {
        private bool _isExpanded;

        public string Name { get; }
        public string FullPath { get; }
        public bool IsDirectory { get; }
        public ObservableCollection<FileNodeViewModel> Children { get; }
        // Список расширений, которые нужно скрыть
        private static readonly HashSet<string> HiddenExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".aproj",
            ".cfg"
        };

        public bool IsExpanded
        {
            get => _isExpanded;
            set { SetField(ref _isExpanded, value); }
        }

        public FileNodeViewModel(string path, bool isDirectory)
        {
            FullPath = path;
            Name = Path.GetFileName(path);
            IsDirectory = isDirectory;
            Children = new ObservableCollection<FileNodeViewModel>();
        }

        /// <summary>
        /// Рекурсивно загружает содержимое папки.
        /// expandAll — раскрывать ли все подпапки сразу.
        /// </summary>
        public void LoadChildren(bool expandAll = false)
        {
            Children.Clear();

            if (!IsDirectory || !Directory.Exists(FullPath))
                return;

            try
            {
                // Папки — сначала, отсортированы по имени
                var dirs = Directory.GetDirectories(FullPath)
                    .OrderBy(d => Path.GetFileName(d))
                    .Select(d => CreateDirectoryNode(d, expandAll));

                // Файлы — после папок, отсортированы по имени
                var files = Directory.GetFiles(FullPath)
                    .Where(f => !HiddenExtensions.Contains(Path.GetExtension(f)))
                    .Where(f => !Path.GetFileName(f).StartsWith("log", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => Path.GetFileName(f))
                    .Select(f => new FileNodeViewModel(f, isDirectory: false));

                foreach (var node in dirs.Concat(files))
                    Children.Add(node);
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        private static FileNodeViewModel CreateDirectoryNode(string path, bool expandAll)
        {
            var node = new FileNodeViewModel(path, isDirectory: true);
            node.LoadChildren(expandAll);     // рекурсивная загрузка
            node.IsExpanded = expandAll;
            return node;
        }
    }
}
