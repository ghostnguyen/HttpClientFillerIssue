namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class NotMultipartAttribute : Attribute
{
    public NotMultipartAttribute()
    {
    }
}