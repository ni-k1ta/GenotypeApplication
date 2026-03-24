using System.IO;

namespace GenotypeApplication.Constants
{
    public static class PathConstants
    {
        public static readonly string PROJECT_DEFAULT_PATH = Path.Combine(AppContext.BaseDirectory, "Projects");
        public static readonly string RECENT_PROJECTS_FILE_DEFAULT_PATH = Path.Combine(AppContext.BaseDirectory, "recent_projects.json");
        public static readonly string DEFAULT_DOCUMENTS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static readonly string EXTERNAL_PROGRAMS_DEFAULT_FOLDER_PATH = Path.Combine(AppContext.BaseDirectory, "External programs");
    }
}
