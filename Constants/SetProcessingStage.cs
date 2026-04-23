using System.ComponentModel;
using System.Reflection;

namespace GenotypeApplication.Constants
{
    public enum SetProcessingStage
    {
        [Description("Structure")]
        Structure,

        [Description("Structure Harvester")]
        StructureHarvester,

        [Description("CLUMPP")]
        CLUMPP,

        [Description("Distruct")]
        Distruct
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
