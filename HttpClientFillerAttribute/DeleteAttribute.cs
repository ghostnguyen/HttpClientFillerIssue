namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class DeleteAttribute : Attribute, IHttpAttribute
{
    public DeleteAttribute()
    {
        Url = "";
    }

    public DeleteAttribute(string url)
    {
        Url = url;
    }

    public string HttpMethodCodeGen => "System.Net.Http.HttpMethod.Delete";

    public string Url { get; }

    public bool CanHasBody => true;
}