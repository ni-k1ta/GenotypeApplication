using GenotypeApplication.Models;
using System.Globalization;
using System.Reflection;

namespace GenotypeApplication.Services.Set
{
    public static class DefineParameterModelConverter
    {
        public static IEnumerable<string> GetFormatedLines(object model, string? prefix = null)
        {
            return model.GetType().GetProperties().Select(p => new
                {
                    Attribute = p.GetCustomAttribute<DefineParameterModelAttribute>(),
                    Value = p.GetValue(model)
                }).Where(x => x.Attribute != null).Select(x => 
                $"{ (string.IsNullOrWhiteSpace(prefix) ? (string.Empty) : (prefix + " ")) }" +
                $"{x.Attribute!.Name} {FormatValue(x.Value)}");
        }
        private static string FormatValue(object? value)
        {
            return value switch
            {
                bool b => b ? "1" : "0",
                double d => d.ToString("G", CultureInfo.InvariantCulture),
                float f => f.ToString("G", CultureInfo.InvariantCulture),
                decimal m => m.ToString("G", CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? string.Empty
            };
        }
    }
}
