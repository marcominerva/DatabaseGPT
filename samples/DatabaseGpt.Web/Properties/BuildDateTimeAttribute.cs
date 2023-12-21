using System.Globalization;

namespace System.Reflection;

[AttributeUsage(AttributeTargets.Assembly)]
public class BuildDateTimeAttribute : Attribute
{
    public DateTime DateTime { get; }

    public BuildDateTimeAttribute(string value)
    {
        DateTime = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
