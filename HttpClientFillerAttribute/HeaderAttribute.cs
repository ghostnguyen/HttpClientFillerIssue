namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderAttribute : Attribute
{
    public HeaderAttribute(string header)
    {
        Header = header;
    }

    public string Header { get; }
}