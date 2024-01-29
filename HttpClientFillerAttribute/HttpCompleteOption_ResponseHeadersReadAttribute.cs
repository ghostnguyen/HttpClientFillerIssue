namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class HttpCompleteOption_ResponseHeadersReadAttribute : Attribute
{
    public HttpCompleteOption_ResponseHeadersReadAttribute()
    {
    }
}