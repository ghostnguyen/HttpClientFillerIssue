namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class PutAttribute : Attribute, IHttpAttribute
{
    public PutAttribute()
    {
        Url = "";
    }

    public PutAttribute(string url)
    {
        Url = url;
    }

    public string HttpMethodCodeGen => "System.Net.Http.HttpMethod.Put";

    public string Url { get; }

    public bool CanHasBody => true;
}