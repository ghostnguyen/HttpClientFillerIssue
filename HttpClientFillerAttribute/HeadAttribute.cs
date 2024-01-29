namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class HeadAttribute : Attribute, IHttpAttribute
{
    public HeadAttribute()
    {
        Url = "";
    }

    public HeadAttribute(string url)
    {
        Url = url;
    }

    public string HttpMethodCodeGen => "System.Net.Http.HttpMethod.Head";

    public string Url { get; }

    public bool CanHasBody => false;
}