using HttpClientFillerAttribute;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

HttpClient client = new();

client.BaseAddress = new Uri("https://catfact.ninja/");

var opt = new System.Text.Json.JsonSerializerOptions()
{
    AllowTrailingCommas = true,
    TypeInfoResolver = JsonTypeInfoResolver.Combine(ItemContext.Default)
};

var api = new Restfulapi(client, opt);

var item = await api.Get();

Console.WriteLine(item.fact);

//dotnet publish -r win-x64 -c Release

public interface IRestfulapi
{
    [Get("/fact")]
    Task<Item> Get();
}

[HttpClient]
public partial class Restfulapi : IRestfulapi
{
}

[JsonSerializable(typeof(Item))]
internal partial class ItemContext : JsonSerializerContext
{
}

public class Item
{
    public string fact { get; set; }
    public int length { get; set; }
}