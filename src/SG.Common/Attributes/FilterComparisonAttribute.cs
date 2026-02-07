namespace SG.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterComparisonAttribute : Attribute
    {
        public ComparisonType ComparisonType { get; set; }

        public FilterComparisonAttribute(ComparisonType comparisonType)
        {
            ComparisonType = comparisonType;
        }
    }

    public enum ComparisonType
    {
        Equals,
        Contains,
        GreaterThan,
        LessThan
    }
}
