using System.Text.Json;

namespace GenotypeApplication.MVVM.Infrastructure
{
    public class ParametersChangesTracker<T> where T : class
    {
        private string _snapshot = string.Empty;

        public void TakeModelSnapshot(T model)
        {
            _snapshot = JsonSerializer.Serialize(model);
        }

        public bool HasChanges(T model)
        {
            if (_snapshot is null) return true;
            return _snapshot != JsonSerializer.Serialize(model);
        }
        public T? GetSnapshot()
        {
            if (string.IsNullOrWhiteSpace(_snapshot)) return null;
            var snapshot = JsonSerializer.Deserialize<T>(_snapshot);

            return snapshot ?? null;
        }
    }
}
