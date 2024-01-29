namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public class HeadersAttribute : Attribute
{
    public HeadersAttribute(params string[] headers)
    {
        Headers = headers ?? Array.Empty<string>();
    }

    public string[] Headers { get; }
}