using GenotypeApplication.Constants;
using GenotypeApplication.Models;
using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace GenotypeApplication.Services.Application_configuration.Logger
{
    public class LoggerService
    {
        private readonly object _fileLock = new();
        private readonly ConcurrentDictionary<string, StreamWriter> _unitWriters = new();
        private readonly object _pendingLock = new();
        private readonly List<LogModel> _pendingEntries = new();
        private readonly DispatcherTimer _flushTimer;

        public ObservableCollection<LogModel> Entries { get; } = new();

        public string? ProjectPath { get; set; }
        public string? SetName { get; set; }

        public LoggerService()
        {
            _flushTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _flushTimer.Tick += FlushToUI;
            _flushTimer.Start();
        }

        public ProgramLogger CreateLogger(SetProcessingStage stage, string? unitKey = null)
        {
            return new ProgramLogger(this, stage, unitKey);
        }

        internal void Log(SetProcessingStage stage, string message, LogLevel level, string? unitKey)
        {
            var source = stage.GetDescription();
            var entry = new LogModel(source, message, level);

            lock (_pendingLock)
            {
                _pendingEntries.Add(entry);
            }
            if (unitKey != null)
            {
                if (string.IsNullOrEmpty(ProjectPath) || string.IsNullOrEmpty(SetName))
                    return;

                var writer = GetOrCreateUnitWriter(stage, unitKey);
                lock (writer)
                {
                    writer.WriteLine(entry.ToFileString);
                }
            }
            else
            {
                Task.Run(() => WriteToFile(source, entry));   // общие — как было
            }
        }

        private string GetUnitTempDir(SetProcessingStage stage)
        {
            // временные файлы кладём рядом с финальным логом, в подпапку
            var dir = Path.Combine(ProjectPath!, SetName!, stage.GetDescription(), ".units");
            Directory.CreateDirectory(dir);
            return dir;
        }
        private static string SanitizeKey(string key)
            => string.Concat(key.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

        private StreamWriter GetOrCreateUnitWriter(SetProcessingStage stage, string unitKey)
        {
            return _unitWriters.GetOrAdd(unitKey, key =>
            {
                var path = Path.Combine(GetUnitTempDir(stage), SanitizeKey(key) + ".log");
                var sw = new StreamWriter(path, append: true) { AutoFlush = true };
                return sw;
            });
        }

        public void AssembleUnitLogs(SetProcessingStage stage, IEnumerable<string> orderedUnitKeys)
        {
            if (string.IsNullOrEmpty(ProjectPath) || string.IsNullOrEmpty(SetName))
                return;

            var stageName = stage.GetDescription();
            var stageDir = Path.Combine(ProjectPath, SetName, stageName);
            var unitsDir = Path.Combine(stageDir, ".units");
            Directory.CreateDirectory(stageDir);

            var finalPath = Path.Combine(stageDir, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

            lock (_fileLock)
            {
                // 1. закрываем все открытые writer'ы, чтобы файлы освободились
                foreach (var key in _unitWriters.Keys.ToList())
                {
                    if (_unitWriters.TryRemove(key, out var w))
                    {
                        try { w.Dispose(); } catch { /* лог-в-лог не пишем, чтобы не зациклиться */ }
                    }
                }

                // 2. склеиваем в финальный файл в нужном порядке
                using (var output = new StreamWriter(finalPath, append: true))
                {
                    foreach (var key in orderedUnitKeys)
                    {
                        var unitFile = Path.Combine(unitsDir, SanitizeKey(key) + ".log");
                        if (!File.Exists(unitFile)) continue;

                        output.WriteLine($"===== {key} =====");
                        using (var input = new StreamReader(unitFile))
                        {
                            string? line;
                            while ((line = input.ReadLine()) != null)
                                output.WriteLine(line);
                        }
                        output.WriteLine();
                    }
                }

                // 3. чистим временные файлы
                try
                {
                    if (Directory.Exists(unitsDir))
                        Directory.Delete(unitsDir, recursive: true);
                }
                catch (Exception ex)
                {
                    UIDispatcherHelper.RunOnUI(() =>
                        Entries.Add(new LogModel("LogService", $"Failed to clean unit logs: {ex.Message}", LogLevel.Warning)));
                }
            }
        }

        public void RecoverOrphanedUnits(SetProcessingStage stage)
        {
            if (string.IsNullOrEmpty(ProjectPath) || string.IsNullOrEmpty(SetName))
                return;

            var unitsDir = Path.Combine(ProjectPath, SetName, stage.GetDescription(), ".units");
            if (!Directory.Exists(unitsDir)) return;

            var orphanFiles = Directory.GetFiles(unitsDir, "*.log");
            if (orphanFiles.Length == 0) return;

            // подбираем что есть — даже без orderedKeys, просто по имени файла
            var keys = orphanFiles
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToList();

            AssembleUnitLogs(stage, keys!);
        }
        private void FlushToUI(object? sender, EventArgs e)
        {
            List<LogModel> toAdd;

            lock (_pendingLock)
            {
                if (_pendingEntries.Count == 0) return;
                toAdd = new List<LogModel>(_pendingEntries);
                _pendingEntries.Clear();
            }

            foreach (var entry in toAdd)
                Entries.Add(entry);

            LogScrollRequested?.Invoke();
        }

        public event Action? LogScrollRequested;

        private void WriteToFile(string programName, LogModel entry)
        {
            if (string.IsNullOrEmpty(ProjectPath) || string.IsNullOrEmpty(SetName))
                return;

            try
            {
                var dir = Path.Combine(ProjectPath, SetName, programName);
                Directory.CreateDirectory(dir);

                var fileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
                var filePath = Path.Combine(dir, fileName);

                lock (_fileLock)
                {
                    File.AppendAllText(filePath, entry.ToFileString + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                UIDispatcherHelper.RunOnUI(() =>
                    Entries.Add(new LogModel("LogService", $"Log write error: {ex.Message}", LogLevel.Error)));
            }
        }

        public void ClearDisplay()
        {
            UIDispatcherHelper.RunOnUI(() => Entries.Clear());
        }
    }
}
