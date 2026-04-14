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

        public static void PopulateModelFromLines(object model, IEnumerable<string> lines, string? prefix = null)
        {
            var properties = model.GetType().GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<DefineParameterModelAttribute>()
                })
                .Where(x => x.Attribute != null)
                .ToDictionary(x => x.Attribute!.Name, x => x.Property);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
                        continue;
                    trimmed = trimmed[prefix.Length..].TrimStart();
                }

                var spaceIndex = trimmed.IndexOf(' ');
                if (spaceIndex <= 0)
                    continue;

                var name = trimmed[..spaceIndex];
                var rawValue = trimmed[(spaceIndex + 1)..].Trim();

                if (!properties.TryGetValue(name, out var property))
                    continue;

                var converted = ParseValue(rawValue, property.PropertyType);
                property.SetValue(model, converted);
            }
        }

        private static object? ParseValue(string raw, Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(bool))
                return raw is "1" or "true" or "True";

            if (underlying == typeof(int))
                return int.Parse(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(double))
                return double.Parse(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(float))
                return float.Parse(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(decimal))
                return decimal.Parse(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(string))
                return raw;

            if (underlying.IsEnum)
                return Enum.Parse(underlying, raw);

            return Convert.ChangeType(raw, underlying, CultureInfo.InvariantCulture);
        }
    }
}
