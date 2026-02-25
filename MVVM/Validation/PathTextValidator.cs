using GenotypeApplication.Interfaces.MVVM;
using System.IO;

namespace GenotypeApplication.MVVM.Validation
{
    public class PathTextValidator : IValidator<string>
    {
        private const int MaxPathLength = 260;

        private readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        private readonly string[] ReservedNames =
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        public ValidationResult Validate(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return ValidationResult.Failure("Path can't be empty!");

            path = path.Trim();

            if (path.Length > MaxPathLength)
                return ValidationResult.Failure($"The path cannot be longer than {MaxPathLength} characters.");

            if (path.IndexOfAny(InvalidPathChars) >= 0)
                return ValidationResult.Failure("Path contains invalid characters.");

            if (ContainsInvalidCharacters(path))
                return ValidationResult.Failure("Path contains invalid characters: < > \" | ? *");

            if (!IsValidPathFormat(path))
                return ValidationResult.Failure("Incorrect path format.");

            if (ContainsReservedName(path))
                return ValidationResult.Failure("Path contains a reserved Windows name.");

            return ValidationResult.Success();
        }
        private bool ContainsInvalidCharacters(string path)
        {
            char[] additionalInvalid = { '<', '>', '"', '|', '?', '*' };

            var pathWithoutDrive = path;
            if (path.Length >= 2 && path[1] == ':' && char.IsLetter(path[0]))
            {
                pathWithoutDrive = path.Substring(2);
            }

            return pathWithoutDrive.IndexOfAny(additionalInvalid) >= 0;
        }
        private bool IsValidPathFormat(string path)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);

                if (fullPath == Path.DirectorySeparatorChar.ToString() ||
                    fullPath == Path.AltDirectorySeparatorChar.ToString())
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool ContainsReservedName(string path)
        {
            try
            {
                var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                foreach (var part in parts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;

                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(part).ToUpperInvariant();

                    if (ReservedNames.Contains(nameWithoutExtension))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
