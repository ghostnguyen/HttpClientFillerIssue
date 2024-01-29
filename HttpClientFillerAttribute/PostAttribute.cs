namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class PostAttribute : Attribute, IHttpAttribute
{
    public PostAttribute()
    {
        Url = "";
    }

    public PostAttribute(string url)
    {
        Url = url;
    }

    public string HttpMethodCodeGen => "System.Net.Http.HttpMethod.Post";

    public string Url { get; }

    public bool CanHasBody => true;
}