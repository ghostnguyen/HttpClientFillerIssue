namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Class)]
public class HttpClientAttribute : Attribute
{
    public HttpClientAttribute()
    {
    }
}