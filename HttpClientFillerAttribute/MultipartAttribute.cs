namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Method)]
public class MultipartAttribute : Attribute
{
    public MultipartAttribute()
    {
        BoundaryText = "----HttpClientFillerMultipartBoundary";
    }

    public MultipartAttribute(string boundaryText)
    {
        BoundaryText = boundaryText;
    }

    public string BoundaryText { get; }
}