using GenotypeApplication.Constants;

namespace GenotypeApplication.Models
{
    public class LogModel
    {
        public DateTime Timestamp { get; }
        public string Source { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        public LogModel(string source, string message, LogLevel level)
        {
            Timestamp = DateTime.Now;
            Source = source;
            Level = level;
            Message = message;
        }

        // для записи в файл
        public string ToFileString
            => $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";

        // для UI — с источником
        public string ToDisplayString
            => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Source}] [{Level}] {Message}";
    }
}
