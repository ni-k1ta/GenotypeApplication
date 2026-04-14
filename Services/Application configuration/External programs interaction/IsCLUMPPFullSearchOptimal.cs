namespace GenotypeApplication.Services.Application_configuration.External_programs_interaction
{
    public static class IsCLUMPPFullSearchOptimal
    {
        private static long Factorial(int n)
        {
            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        public static bool Calculate(int kMax, int R, long threshold = 100_000_000)
        {
            long kFactorial = Factorial(kMax);

            long configurations = 1;
            for (int i = 0; i < R - 1; i++)
            {
                configurations *= kFactorial;
                if (configurations > threshold)
                    return false;
            }

            return true;
        }
    }
}
