namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class AuthorizeAttribute : Attribute
{
    public AuthorizeAttribute()
    {
        Scheme = "Bearer";
    }

    public AuthorizeAttribute(string scheme = "Bearer")
    {
        Scheme = string.IsNullOrEmpty(scheme.Trim()) ? "Bearer" : scheme;
    }

    public string Scheme { get; }
}