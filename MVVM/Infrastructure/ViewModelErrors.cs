using GenotypeApplication.MVVM.Validation;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GenotypeApplication.MVVM.Infrastructure
{
    public class ViewModelErrors : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _propertyErrors = [];
        public bool HasErrors => _propertyErrors.Count != 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null || !_propertyErrors.TryGetValue(propertyName, out var errors))
            {
                return Enumerable.Empty<string>();
            }
            return errors;
        }
        public bool HasErrorsFor([CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;
            return _propertyErrors.TryGetValue(propertyName, out var errors) && errors.Count > 0;
        }
        public void AddError(string errorMessage, [CallerMemberName] string propertyName = "")
        {
            if (!_propertyErrors.ContainsKey(propertyName)) _propertyErrors.Add(propertyName, []);

            _propertyErrors[propertyName].Add(errorMessage);
            OnErrorsChanged(propertyName);
            OnPropertyChanged(nameof(HasErrors));
        }
        public void ClearErrors(string propertyName)
        {
            if (_propertyErrors.Remove(propertyName))
            {
                OnErrorsChanged(propertyName);
                OnPropertyChanged(nameof(HasErrors));
            }
        }
        private void OnErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ValidateProperty<T>(T value,
        Func<T, ValidationResult> validator,
        [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;

            ClearErrors(propertyName);

            var result = validator(value);
            if (!result.IsValid && result.ErrorMessage != null)
            {
                AddError(result.ErrorMessage, propertyName);
            }
        }
    }
}
