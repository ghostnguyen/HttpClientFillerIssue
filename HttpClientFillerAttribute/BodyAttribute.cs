namespace HttpClientFillerAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class BodyAttribute : Attribute
{
    public BodyAttribute()
    {
        SerializationMethod = BodySerializationMethod.Auto;
    }

    public BodyAttribute(BodySerializationMethod serializationMethod)
    {
        SerializationMethod = serializationMethod;
    }

    public BodySerializationMethod SerializationMethod { get; }
}