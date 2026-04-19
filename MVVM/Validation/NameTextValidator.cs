using GenotypeApplication.Interfaces.MVVM;
using System.Text.RegularExpressions;

namespace GenotypeApplication.MVVM.Validation
{
    public class NameTextValidator : IValidator<string>
    {
        private readonly Regex AllowedCharsRegex = new(@"^[a-zA-Z0-9()#№\-_]*$", RegexOptions.Compiled);
        private const int MaxLength = 30;

        public ValidationResult Validate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ValidationResult.Failure("Name can't be empty!");
            }

            if (value.Length > MaxLength)
            {
                return ValidationResult.Failure($"Maximum length - {MaxLength} characters.");
            }

            if (!AllowedCharsRegex.IsMatch(value))
            {
                return ValidationResult.Failure("Allowed characters: letters, numbers, ( ) # № - _");
            }

            return ValidationResult.Success();
        }
    }
}
