using GenotypeApplication.MVVM.Validation;

namespace GenotypeApplication.Interfaces.MVVM
{
    public interface IValidator<in T>
    {
        ValidationResult Validate(T value);
    }
}
