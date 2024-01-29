namespace HttpClientFillerAttribute;

public interface IHttpAttribute
{
    public string HttpMethodCodeGen { get; }

    public string Url { get; }

    public bool CanHasBody { get; }
}