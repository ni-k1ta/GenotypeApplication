namespace GenotypeApplication.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefineParameterModelAttribute : Attribute
    {
        public string Name { get; }

        public DefineParameterModelAttribute(string name)
        {
            Name = name;
        }
    }
}
