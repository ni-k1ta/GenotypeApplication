using GenotypeApplication.Constants;
using GenotypeApplication.Models;
using GenotypeApplication.MVVM.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;

namespace GenotypeApplication.Services.Application_configuration.Logger
{
    public class LoggerService
    {
        private readonly object _fileLock = new();

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
                Interval = TimeSpan.FromMilliseconds(100) // flush 10 раз в секунду
            };
            _flushTimer.Tick += FlushToUI;
            _flushTimer.Start();
        }

        public ProgramLogger CreateLogger(SetProcessingStage stage)
        {
            return new ProgramLogger(this, stage);
        }

        internal void Log(SetProcessingStage stage, string message, LogLevel level)
        {
            var entry = new LogModel(stage.ToString(), message, level);

            lock (_pendingLock)
            {
                _pendingEntries.Add(entry);
            }

            Task.Run(() => WriteToFile(stage.ToString(), entry));
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

            // скролл один раз после всей пачки
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
                    Entries.Add(new LogModel("LogService", $"Ошибка записи лога: {ex.Message}", LogLevel.Error)));
            }
        }

        public void ClearDisplay()
        {
            UIDispatcherHelper.RunOnUI(() => Entries.Clear());
        }
    }
}
