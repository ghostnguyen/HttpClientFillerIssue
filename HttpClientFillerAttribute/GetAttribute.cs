namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class GetAttribute : Attribute, IHttpAttribute
{
    public GetAttribute()
    {
        Url = "";
    }

    public GetAttribute(string url)
    {
        Url = url;
    }

    public string HttpMethodCodeGen => "System.Net.Http.HttpMethod.Get";

    public string Url { get; }

    public bool CanHasBody => false;
}