using GenotypeApplication.Interfaces.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.MVVM.Validation
{
    public class KRangeValidator : IValidator<(int kStart, int kEnd, int startLimited, int endLimited)>
    {
        public ValidationResult Validate((int kStart, int kEnd, int startLimited, int endLimited) value)
        {
            if (value.kStart < 0 || value.kEnd < 0)
            {
                return ValidationResult.Failure("K must be greater than 0!");
            }

            if (value.kStart > value.kEnd)
            {
                return ValidationResult.Failure("Start K value must be less than or equal to end K!");
            }

            if (value.kStart < value.startLimited)
            {
                return ValidationResult.Failure($"Start K value must be greater than or equal to {value.startLimited}!");
            }

            if (value.kEnd > value.endLimited)
            {
                return ValidationResult.Failure($"End K value must be less than or equal to {value.endLimited}!");
            }

            return ValidationResult.Success();
        }
    }
}
