namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderCollectionAttribute : Attribute
{
    public HeaderCollectionAttribute()
    {
    }
}