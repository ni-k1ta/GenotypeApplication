using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;

namespace GenotypeApplication.MVVM.TreeView
{
    public class ProjectExplorerViewModel : ViewModelBase
    {
        private string? _projectPath;
        private string? _setName;
        private FileSystemWatcher? _watcher;

        private System.Threading.Timer? _debounceTimer;
        private const int DebounceDelayMs = 200;

        /// <summary>
        /// Корневые узлы дерева (всегда один элемент — папка проекта).
        /// </summary>
        public ObservableCollection<FileNodeViewModel> RootNodes { get; }

        /// <summary>
        /// Путь к папке проекта.
        /// </summary>
        public string? ProjectPath
        {
            get => _projectPath;
            set
            {
                if (SetField(ref _projectPath, value))
                    OnProjectOrSetChanged();
            }
        }
        /// <summary>
        /// Имя текущего Set-набора (null = набор не выбран).
        /// </summary>
        public string? SetName
        {
            get => _setName;
            set
            {
                if (SetField(ref _setName, value))
                    OnProjectOrSetChanged();
            }
        }

        public ProjectExplorerViewModel()
        {
            RootNodes = new ObservableCollection<FileNodeViewModel>();
        }

        /// <summary>
        /// Вызывается при любом изменении ProjectPath или SetName.
        /// Перестраивает дерево в нужном режиме.
        /// </summary>
        private async void OnProjectOrSetChanged()
        {
            StopWatcher();
            await RebuildAsync();
            StartWatcher();
        }
        /// <summary>
        /// Полная перестройка дерева (вызывается при событиях FileSystemWatcher).
        /// </summary>
        private async Task RebuildAsync()
        {
            var newRoot = await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(_projectPath) || !Directory.Exists(_projectPath))
                    return null;

                if (string.IsNullOrEmpty(_setName))
                {
                    var root = new FileNodeViewModel(_projectPath, isDirectory: true);
                    root.LoadChildren(expandAll: false);
                    root.IsExpanded = true;
                    return root;
                }
                else
                {
                    var root = new FileNodeViewModel(_projectPath, isDirectory: true);
                    root.IsExpanded = true;
                    var setPath = Path.Combine(_projectPath, _setName);
                    if (Directory.Exists(setPath))
                    {
                        var setNode = new FileNodeViewModel(setPath, isDirectory: true);
                        setNode.LoadChildren(expandAll: true);
                        setNode.IsExpanded = true;
                        root.Children.Add(setNode);
                    }
                    return root;
                }
            });

            RootNodes.Clear();
            if (newRoot != null)
                RootNodes.Add(newRoot);
        }

        #region FileSystemWatcher

        private void StartWatcher()
        {
            if (string.IsNullOrEmpty(_projectPath) || !Directory.Exists(_projectPath))
                return;

            _watcher = new FileSystemWatcher
            {
                Path = _projectPath,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileSystemChanged;
            _watcher.Deleted += OnFileSystemChanged;
            _watcher.Renamed += OnFileSystemChanged;
        }

        private void StopWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileSystemChanged;
                _watcher.Deleted -= OnFileSystemChanged;
                _watcher.Renamed -= OnFileSystemChanged;
                _watcher.Dispose();
                _watcher = null;
            }

            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new System.Threading.Timer(
                _ => System.Windows.Application.Current?.Dispatcher?.InvokeAsync(RebuildAsync),
                null,
                DebounceDelayMs,
                System.Threading.Timeout.Infinite
            );
        }

        #endregion
    }
}
