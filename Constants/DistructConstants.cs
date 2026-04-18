namespace GenotypeApplication.Constants
{
    public static class DistructConstants
    {
        public const string DISTRUCT_EXECUTABLE_FILE_NAME = "distructWindows1.1.exe";
        public const string DISTRUCT_FOLDER_NAME = "Distruct";
        public const string DISTRUCT_RESULTS_FOLDER_NAME = "Results";
        public const string DISTRUCT_OPTIONAL_FOLDER_NAME = "Optional";
        public const string DISTRUCT_CLUST_PERM_FOLDER = "Color palette";

        [Flags]
        public enum OutputFormat
        {
            None = 0,
            Pdf = 1,
            Png = 2,
            Jpeg = 4,
            Bmp = 8
        }
    }
}
