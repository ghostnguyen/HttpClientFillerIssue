namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class PatchAttribute : Attribute, IHttpAttribute
{
    public PatchAttribute()
    {
        Url = "";
    }

    public PatchAttribute(string url)
    {
        Url = url;
    }

    public string HttpMethodCodeGen => "System.Net.Http.HttpMethod.Patch";

    public string Url { get; }

    public bool CanHasBody => true;
}