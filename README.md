## HttpClientFiller - Refit that supports native ahead-of-time (AOT) compilation.

The HttpClientFiller usage is mostly identical from Refit. 

There are a few different to promote easier to use (less junk code) but highly customizable on your need.

**Unlike Refit** will mark for the different.

Requirements and Limitation:
- .NET 6 (C# 12) or above 
- No support XML Serialization, Newtonsoft.Json
- Only support System.Text.Json.JsonSerializer

Sample:

```csharp
public interface IGitHubApi
{
    [Get("/users/{user}")]
    Task<User> GetUser(string user);
}

[HttpClient]
public partial class GitHubApi : IGitHubApi
{
}
```

**Unlike Refit**
- You must declare addtion `partial class` and [HttpClient] attribute to generate explicit implementation (no reflection). 
- The method in interface must return `Task` or `Task<T>`, otherwise you have to manually provide implemetation.

```csharp
// Register to DI
builder.Services.AddHttpClient<IGitHubApi, GitHubApi>(c => c.BaseAddress = new Uri("https://api.github.com"));

//Usage
public AbcController(IGitHubApi gitHubApi)
{
    gitHubApi.GetUser("test");
}
```
**Unlike Refit**

- Since explicit implementation, it supports registering via HttpClientFactory by default. No need for additional helper method.

## Changelog

### [0.1.2] - 2024-01-25

#### Add
- Intercept HttpRequestMessage supports Async
- More Analyzer rules
- Fix bug


### [0.1.1] - 2024-01-22

#### Update 
- Readme.md

### [0.1.0] - 2024-01-21


# Table of Contents

* [Where does this work?](#where-does-this-work)
* [API Attributes](#api-attributes)
* [Dynamic Querystring Parameters](#dynamic-querystring-parameters)
* [Collections as Querystring parameters](#collections-as-querystring-parameters)
* [Unescape Querystring parameters](#unescape-querystring-parameters)
* [Body content](#body-content)
  * [Buffering and the Content-Length header](#buffering-and-the-content-length-header)
  * [HttpCompleteOption] (#http-complete-option)
  * [JSON content](#json-content)
  * [JSON source generator]
  * [XML Content](#xml-content)
  * [Form posts](#form-posts)
* [Setting request headers](#setting-request-headers)
  * [Static headers](#static-headers)
  * [Dynamic headers](#dynamic-headers)
  * [Bearer Authentication](#bearer-authentication)
  * [Reducing header boilerplate with DelegatingHandlers (Authorization headers worked example)](#reducing-header-boilerplate-with-delegatinghandlers-authorization-headers-worked-example)
  * [Redefining headers](#redefining-headers)
  * [Removing headers](#removing-headers)
* [Passing state into DelegatingHandlers](#passing-state-into-delegatinghandlers)
  * [Support for Polly and Polly.Context](#support-for-polly-and-pollycontext)
  * [Target Interface type](#target-interface-type)
* [Multipart uploads](#multipart-uploads)
* [Retrieving the response](#retrieving-the-response)
* [Using generic interfaces](#using-generic-interfaces)
* [Interface inheritance](#interface-inheritance)
  * [Headers inheritance](#headers-inheritance)
* [Default Interface Methods](#default-interface-methods)
* [Using HttpClientFactory](#using-httpclientfactory)
* [Providing a custom HttpClient](#providing-a-custom-httpclient)
* [Intercept HttpRequestMessage]
* [Handling exceptions](#handling-exceptions)
  * [Providing a custom ExceptionFactory](#providing-a-custom-exceptionfactory)
  * [ApiException deconstruction with Serilog](#apiexception-deconstruction-with-serilog)
* [Aspect Oriented Programming to your interface]
* [Issue Report]

### Where does this work?

- .NET 6 or above

### API Attributes

- Get, Post, Put, Delete, Patch and Head.
- The string param is used as relative URL.
- The string param will be generated as [string interpolation](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/string-interpolation) and [raw string literal](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/raw-string). It is more easier to use (less library custom Attributes) and maximum flexibility (write your own format)

Static url:

```csharp
[Get("/users/list")]
```

You can also specify query parameters in the URL:

```csharp
[Get("/users/list?sort=desc")]
```
Binding to param:

```csharp
[Get("/group/{groupId}/users")]
Task<List<User>> GroupList(int groupId);
```

**Unlike Refit**
- HttpClientFiller has no ~~`AliasAs`~~ attribute. 

This will make compile error since there is no `id` parameter in the method.

```csharp
[Get("/group/{id}/users")]
Task<List<User>> GroupList(int groupId);
```

Binding to object:

```csharp
[Get("/group/{request.groupId}/users/{request.userId}")]
Task<List<User>> GroupList(UserGroupRequest request);

class UserGroupRequest{
    int groupId { get;set; }
    int userId { get;set; }
}

```

**Unlike Refit**
- Binding must be explicit. There is no support for:
~~Parameters that are not specified as a URL substitution will automatically be used as query parameters.~~ 
- But you can write your own. (See Dynamic Querystring Parameters)

**Unlike Refit**
- No-need encoded using a double-asterisk (\*\*) for uri path.

```csharp
[Get("/search/{page}")]
Task<List<Page>> Search(string page);

Search("admin/products");
>>> "/search/admin/products"
```

**Note**

For [Head] attribute, the return type of method must be `Task<HttpResponseMessage>` because http verb head request should return no body response.

**CancellationToken**

Add the `CancellationToken ct` param to your method, it will be passed to HttpClient.

Only one `CancellationToken` param will be allowed.

```csharp
[Get("chapter/{id}")]
Task<ApiResponse<Chapter>> Chapter(int id, CancellationToken ct);
```



### Dynamic Querystring Parameters

**Unlike Refit**

- HttpClientFiller don't support for `Dynamic Querystring Parameters`. 

But you can create your own custom querystring method.

```csharp
public class MyQueryParams
{
    public string SortOrder { get; set; }
    public int Limit { get; set; }
    public KindOptions Kind { get; set; }

    public string ToQueryString(string prefix = "")
        => $"{prefix}order={SortOrder}&{prefix}Limit={Limit}&{prefix}Kind={Kind}}"
}

public enum KindOptions
{
    Foo,
    Bar
}


[Get("/group/{groupId}/users?{params.ToQueryString()}")]
Task<List<User>> GroupList(int groupId, MyQueryParams params);

[Get("""/group/{groupId}/users?{params.ToQueryString("search.")}""")]
Task<List<User>> GroupListWithAttribute(int groupId, MyQueryParams params);


params.SortOrder = "desc";
params.Limit = 10;
params.Kind = KindOptions.Bar;

GroupList(4, params)
>>> "/group/4/users?order=desc&Limit=10&Kind=1"

GroupListWithAttribute(4, params)
>>> "/group/4/users?search.order=desc&search.Limit=10&search.Kind=1"
```

### Collections as Querystring parameters

**Unlike Refit**
- HttpClientFiller has no ~~`Query`~~ attribute to specify format in which collections should be formatted in query string.

But you can create for your own or use some built-in methods for collection as query string.

```csharp

//In GlobalUsings.cs
global using HttpClientFiller.Extension;


[Get("/users/list?{ages.Multi()}")]
Task Search(int[] ages);

Search(new [] {10, 20, 30})
>>> "/users/list?ages=10&ages=20&ages=30"

[Get("/users/list?{ages.Csv()}")]
Task Search(int[] ages);

Search(new [] {10, 20, 30})
>>> "/users/list?ages=10%2C20%2C30"
```

### Unescape Querystring parameters

**Unlike Refit**
- HttpClientFiller has no ~~`QueryUriFormat`~~ attribute to specify if the query parameters should be url escaped.

But you can write your own such as:

```csharp

public partial class GitHubApi : IGitHubApi
{
    public string Encode(string data) => WebUtility.UrlEncode(data);
}

[Get("/query?q={Encode(q)}")]
Task Query(string q);

Query("Select+Id,Name+From+Account")
>>> "/query?q=Select+Id,Name+From+Account"
```

### Body content

One (and only one) of the parameters in your method can be used as the body, by using the
Body attribute:

```csharp
[Post("/users/new")]
Task CreateUser([Body] User user);
```

There are four possibilities for supplying the body data, depending on the
type of the parameter:

* If the type is `Stream`, the content will be streamed via `StreamContent`
* If the type is `string`, the string will be used directly as the content unless `[Body(BodySerializationMethod.Json)]` is set which will send it as a `StringContent`
* If the parameter has the attribute `[Body(BodySerializationMethod.UrlEncoded)]`,
  the content will be URL-encoded (see [form posts](#form-posts) below)
* For all other types, the object will be serialized using the `System.Text.Json.JsonSerializer`.

**Unlike Refit**
- HttpClientFiller has no custom setting (like RefitSettings). It uses HttpClient and you can config it directly.

* HttpClientFiller has no support for Xml serializer.


#### Buffering and the `Content-Length` header

Behind the scene is `HttpClient` send http request which is streaming without buffering by default.

**Unlike Refit**
- HttpClientFiller don't control streaming buffer behavior. But you can control it in the Stream object which is passed to method. 

#### HttpCompleteOption

By default, `HttpClient` sets `HttpCompleteOption` to `ResponseContentRead`.

To change it to `ResponseHeadersRead`, you can use `HttpCompleteOption_ResponseHeadersReadAttribute` to the method.

```csharp
[Post("/users/new")]
[HttpCompleteOption_ResponseHeadersRead]
Task CreateUser([Body] User user);
```

#### JSON content

**Unlike Refit**

- HttpClientFiller has no support for ~~`Newtonsoft.Json`~~.

It only support `System.Text.Json.JsonSerializer` to serialized/deserialized which is configurable through `System.Text.Json.JsonSerializerOptions`.

```csharp
builder.Services.AddHttpClient<IGitHubApi, GitHubApi>(client => 
{
    client.BaseAddress = new Uri("https://api.github.com");
    return new GitHubApi(client, new System.Text.Json.JsonSerializerOptions()
    {
        AllowTrailingCommas = true,                    
    });
});
```

Property serialization/deserialization can be customised using Json.NET's
JsonProperty attribute:

```csharp
public class Foo
{
    [JsonProperty(PropertyName="b")]
    public string Bar { get; set; }
}
```

##### JSON source generator

To apply the benefits of the new [JSON source generator](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0/) for System.Text.Json added in .NET 6

```csharp

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Chapter))]
internal partial class ChapterContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(WeatherForecast))]
internal partial class WeatherForecastContext : JsonSerializerContext
{
}

builder.Services.AddHttpClient<IGitHubApi, GitHubApi>(client => 
{
    client.BaseAddress = new Uri("https://api.github.com");
    return new GitHubApi(client, new System.Text.Json.JsonSerializerOptions()
    {
        AllowTrailingCommas = true,                    
        TypeInfoResolver = JsonTypeInfoResolver.Combine(ChapterContext.Default, WeatherForecastContext.Default)
    });
});
```

#### XML Content

No support

#### Form posts

For APIs that take form posts (i.e. serialized as `application/x-www-form-urlencoded`),
initialize the Body attribute with `BodySerializationMethod.UrlEncoded`.

The parameter can be an `IDictionary`:

```csharp
public interface IMeasurementProtocolApi
{
    [Post("/collect")]
    Task Collect([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
}

var data = new Dictionary<string, object> {
    {"v", 1},
    {"tid", "UA-1234-5"},
    {"cid", new Guid("d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c")},
    {"t", "event"},
};

// Serialized as: v=1&tid=UA-1234-5&cid=d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c&t=event
await api.Collect(data);
```

Or you can just pass any object and all _public, readable_ properties will
be serialized as form fields in the request. ~~This approach allows you to alias
property names using `[AliasAs("whatever")]` which can help if the API has
cryptic field names:~~

~~If you have a type that has `[JsonProperty(PropertyName)]` attributes setting property aliases, Refit will use those too (`[AliasAs]` will take precedence where you have both).
This means that the following type will serialize as `one=value1&two=value2`:~~

**Note**

Provide your own form-post data:

- If your object has public method like below, it will be used to generate form-post data rather than auto-gen.

```csharp
public IEnumerable<KeyValuePair<string, string>> GetFormPostData();
```

```csharp
public class ChapterRequest
{
    public int ChapterId { get; set; }

    public string Name { get; set; }

    public IEnumerable<KeyValuePair<string, string>> GetFormPostData()
        => new KeyValuePair<string, string>[] {
            new ("Cid", ChapterId.ToString()),
            new ("Name", Name)
        };
}

public interface IApiPostUrlEncoded
{
    [Post("chapterObj/{id}")]
    Task<long> PostChapterObj_GetFormPostData(int id, [Body(BodySerializationMethod.UrlEncoded)] ChapterRequest chapterRequest);
}
```

### Setting request headers

#### Static headers

You can set one or more static request headers for a request applying a `Headers`
attribute to the method:

```csharp
[Headers("User-Agent: Awesome Octocat App")]
[Get("/users/{user}")]
Task<User> GetUser(string user);
```

Static headers can also be added to _every request in the API_ by applying the
`Headers` attribute to the interface:

```csharp
[Headers("User-Agent: Awesome Octocat App")]
public interface IGitHubApi
{
    [Get("/users/{user}")]
    Task<User> GetUser(string user);

    [Post("/users/new")]
    Task CreateUser([Body] User user);
}
```

#### Dynamic headers

If the content of the header needs to be set at runtime, you can add a header
with a dynamic value to a request by applying a `Header` attribute to a parameter:

```csharp
[Get("/users/{user}")]
Task<User> GetUser(string user, [Header("Authorization")] string authorization);

// Will add the header "Authorization: token OAUTH-TOKEN" to the request
var user = await GetUser("octocat", "token OAUTH-TOKEN");
```

Adding an `Authorization` header is such a common use case that you can add an access token to a request by applying an `Authorize` attribute to a parameter and optionally specifying the scheme:

```csharp
[Get("/users/{user}")]
Task<User> GetUser(string user, [Authorize("Bearer")] string token);

// Will add the header "Authorization: Bearer OAUTH-TOKEN}" to the request
var user = await GetUser("octocat", "OAUTH-TOKEN");

//note: the scheme defaults to Bearer if none provided
```

If you need to set multiple headers at runtime, you can add a `IDictionary<string, string>`
and apply a `HeaderCollection` attribute to the parameter and it will inject the headers into the request:


```csharp

[Get("/users/{user}")]
Task<User> GetUser(string user, [HeaderCollection] IDictionary<string, string> headers);

var headers = new Dictionary<string, string> {{"Authorization","Bearer tokenGoesHere"}, {"X-Tenant-Id","123"}};
var user = await GetUser("octocat", headers);
```

#### Bearer Authentication

Most APIs need some sort of Authentication. The most common is OAuth Bearer authentication. A header is added to each request of the form: `Authorization: Bearer <token>`. 

1. Add `[Headers("Authorization: Bearer")]` to the interface or methods which need the token.

~~2. Set `AuthorizationHeaderValueGetter` in the `RefitSettings` instance. Refit will call your delegate each time it needs to obtain the token, so it's a good idea for your mechanism to cache the token value for some period within the token lifetime.~~

#### Reducing header boilerplate with DelegatingHandlers (Authorization headers worked example)

**Same as Refit**

Although we make provisions for adding dynamic headers at runtime directly in Refit,
most use-cases would likely benefit from registering a custom `DelegatingHandler` in order to inject the headers as part of the `HttpClient` middleware pipeline
thus removing the need to add lots of `[Header]` or `[HeaderCollection]` attributes.

In the example above we are leveraging a `[HeaderCollection]` parameter to inject an `Authorization` and `X-Tenant-Id` header.
This is quite a common scenario if you are integrating with a 3rd party that uses OAuth2. While it's ok for the occasional endpoint,
it would be quite cumbersome if we had to add that boilerplate to every method in our interface.

In this example we will assume our application is a multi-tenant application that is able to pull information about a tenant through
some interface `ITenantProvider` and has a data store `IAuthTokenStore` that can be used to retrieve an auth token to attach to the outbound request.

```csharp

 //Custom delegating handler for adding Auth headers to outbound requests
 class AuthHeaderHandler : DelegatingHandler
 {
     private readonly ITenantProvider tenantProvider;
     private readonly IAuthTokenStore authTokenStore;

    public AuthHeaderHandler(ITenantProvider tenantProvider, IAuthTokenStore authTokenStore)
    {
         this.tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
         this.authTokenStore = authTokenStore ?? throw new ArgumentNullException(nameof(authTokenStore));
         // InnerHandler must be left as null when using DI, but must be assigned a value when
         // using RestService.For<IMyApi>
         // InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await authTokenStore.GetToken();

        //potentially refresh token here if it has expired etc.

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantProvider.GetTenantId());

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

//Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ITenantProvider, TenantProvider>();
    services.AddTransient<IAuthTokenStore, AuthTokenStore>();
    services.AddTransient<AuthHeaderHandler>();

    //this will add our refit api implementation with an HttpClient
    //that is configured to add auth headers to all requests

    //note: AddRefitClient<T> requires a reference to Refit.HttpClientFactory
    //note: the order of delegating handlers is important and they run in the order they are added!

    builder.Services.AddHttpClient<IGitHubApi, GitHubApi>(client =>
    {
        client.BaseAddress = new Uri("https://api.github.com");
        return new GitHubApi(client, new System.Text.Json.JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(ChapterContext.Default, Chapter2Context.Default, WeatherForecastContext.Default)
        });
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();
        //you could add Polly here to handle HTTP 429 / HTTP 503 etc
}

//Your application code
public class SomeImportantBusinessLogic
{
    private ISomeThirdPartyApi thirdPartyApi;

    public SomeImportantBusinessLogic(ISomeThirdPartyApi thirdPartyApi)
    {
        this.thirdPartyApi = thirdPartyApi;
    }

    public async Task DoStuffWithUser(string username)
    {
        var user = await thirdPartyApi.GetUser(username);
        //do your thing
    }
}
```

If you aren't using dependency injection then you could achieve the same thing by doing something like this:

```csharp
var api = new GitHubApi((new HttpClient(new AuthHeaderHandler(tenantProvider, authTokenStore))
    {
        BaseAddress = new Uri("https://api.example.com")
    }
));

var user = await thirdPartyApi.GetUser(username);
//do your thing
```

#### Redefining headers

**Same as Refit**

Unlike Retrofit, where headers do not overwrite each other and are all added to
the request regardless of how many times the same header is defined, Refit takes
a similar approach to the approach ASP.NET MVC takes with action filters &mdash;
**redefining a header will replace it**, in the following order of precedence:

* `Headers` attribute on the interface _(lowest priority)_
* `Headers` attribute on the method
* `Header` attribute or `HeaderCollection` attribute on a method parameter _(highest priority)_

```csharp
[Headers("X-Emoji: :rocket:")]
public interface IGitHubApi
{
    [Get("/users/list")]
    Task<List> GetUsers();

    [Get("/users/{user}")]
    [Headers("X-Emoji: :smile_cat:")]
    Task<User> GetUser(string user);

    [Post("/users/new")]
    [Headers("X-Emoji: :metal:")]
    Task CreateUser([Body] User user, [Header("X-Emoji")] string emoji);
}

// X-Emoji: :rocket:
var users = await GetUsers();

// X-Emoji: :smile_cat:
var user = await GetUser("octocat");

// X-Emoji: :trollface:
await CreateUser(user, ":trollface:");
```

**Note:** This redefining behavior only applies to headers _with the same name_. Headers with different names are not replaced. The following code will result in all headers being included:

```csharp
[Headers("Header-A: 1")]
public interface ISomeApi
{
    [Headers("Header-B: 2")]
    [Post("/post")]
    Task PostTheThing([Header("Header-C")] int c);
}

// Header-A: 1
// Header-B: 2
// Header-C: 3
var user = await api.PostTheThing(3);
```

#### Removing headers

**Same as Refit**

Headers defined on an interface or method can be removed by redefining
a static header without a value (i.e. without `: <value>`) or passing `null` for
a dynamic header. _Empty strings will be included as empty headers._

```csharp
[Headers("X-Emoji: :rocket:")]
public interface IGitHubApi
{
    [Get("/users/list")]
    [Headers("X-Emoji")] // Remove the X-Emoji header
    Task<List> GetUsers();

    [Get("/users/{user}")]
    [Headers("X-Emoji:")] // Redefine the X-Emoji header as empty
    Task<User> GetUser(string user);

    [Post("/users/new")]
    Task CreateUser([Body] User user, [Header("X-Emoji")] string emoji);
}

// No X-Emoji header
var users = await GetUsers();

// X-Emoji:
var user = await GetUser("octocat");

// No X-Emoji header
await CreateUser(user, null);

// X-Emoji:
await CreateUser(user, "");
```

### Passing state into DelegatingHandlers

**Unlike Refit**
- HttpClientFiller don't support this feature.

Because in .NET 5 `HttpRequestMessage.Properties` has been marked `Obsolete`. We are at .NET 6 and above.


#### Support for Polly and Polly.Context

Polly is supported because `HttpClientFactory` and `HttpClient` is used as underneath.

#### Target Interface Type and method info

**Unlike Refit**

- HttpClientFiller don't support this feature.

Because in .NET 5 `HttpRequestMessage.Properties` has been marked `Obsolete`. We are at .NET 6 and above.

### Multipart uploads

**Same as Refit**

Methods decorated with `Multipart` attribute will be submitted with multipart content type.
At this time, multipart methods support the following parameter types:

 - string (parameter name will be used as name and string value as value)
 - byte array
 - Stream
 - FileInfo

Name of the field in the multipart data priority precedence:

* multipartItem.Name if specified and not null (optional); dynamic, allows naming form data part at execution time.
* ~~[AliasAs] attribute  (optional) that decorate the streamPart parameter in the method signature (see below); static, defined in code.~~
* MultipartItem parameter name (default) as defined in the method signature; static, defined in code.

A custom boundary can be specified with an optional string parameter to the `Multipart` attribute. If left empty, this defaults to `----MyGreatBoundary`.

To specify the file name and content type for byte array (`byte[]`), `Stream` and `FileInfo` parameters, use of a wrapper class is required.
The wrapper classes for these types are `ByteArrayPart`, `StreamPart` and `FileInfoPart`.

```csharp
public interface ISomeApi
{
    [Multipart]
    [Post("/users/{id}/photo")]
    Task UploadPhoto(int id, StreamPart stream);
}
```

To pass a Stream to this method, construct a StreamPart object like so:

```csharp
someApiInstance.UploadPhoto(id, new StreamPart(myPhotoStream, "photo.jpg", "image/jpeg"));
```

### Retrieving the response

**Unlike Refit**
- HttpClientFiller don't support ~~`IObservable`~~

Note that in HttpClientFiller, there is no option for a synchronous
network request - all requests must be async, either via `Task` or via `Task<T>`
~~`IObservable`~~. There is also no option to create an async method via a Callback
parameter unlike Retrofit, because we live in the async/await future.

Similarly to how body content changes via the parameter type, the return type
will determine the content returned.

Returning Task without a type parameter will discard the content and solely
tell you whether or not the call succeeded:

```csharp
[Post("/users/new")]
Task CreateUser([Body] User user);

// This will throw if the network call fails
await CreateUser(someUser);
```

If the type parameter is 'HttpResponseMessage' or 'string', the raw response
message or the content as a string will be returned respectively.

```csharp
// Returns the content as a string (i.e. the JSON data)
[Get("/users/{user}")]
Task<string> GetUser(string user);

// Returns the raw response, as an Task that can be used with the
// Reactive Extensions
[Get("/users/{user}")]
Task<HttpResponseMessage> GetUser(string user);
```

There is also a generic wrapper class called `ApiResponse<T>` that can be used as a return type. Using this class as a return type allows you to retrieve not just the content as an object `T`, but also any metadata associated with the request/response `HttpResponseMessage`.

This includes information such as response headers, the http status code and reason phrase (e.g. 404 Not Found), the response version, the original request message that was sent and in the case of an error, an `ApiException` object containing details of the error. Following are some examples of how you can retrieve the response metadata.


**Unlike Refit**
- In HttpClientFiller, to keep it simple, `ApiResponse<T>` just has two properties:

```csharp
namespace HttpClientFillerCore
{
    public sealed class ApiResponse<T>
    {
        public ApiResponse(T? content, HttpResponseMessage response)
        {
            Content = content;
            Response = response;
        }

        public T? Content { get; }
        public HttpResponseMessage Response { get; set; }
    }
}

```


```csharp
//Returns the content within a wrapper class containing metadata about the request/response
[Get("/users/{user}")]
Task<ApiResponse<User>> GetUser(string user);

//Calling the API
var response = await gitHubApi.GetUser("octocat");

//Getting the status code (returns a value from the System.Net.HttpStatusCode enumeration)
var httpStatus = response.Response.StatusCode;

//Determining if a success status code was received
if(response.Response.IsSuccessStatusCode)
{
    //YAY! Do the thing...
}

//Retrieving a well-known header value (e.g. "Server" header)
var serverHeaderValue = response.Response.Headers.Server != null ? response.Response.Headers.Server.ToString() : string.Empty;

//Retrieving a custom header value
var customHeaderValue = string.Join(',', response.Response.Headers.GetValues("A-Custom-Header"));

//Looping through all the headers
foreach(var header in response.Response.Headers)
{
    var headerName = header.Key;
    var headerValue = string.Join(',', header.Value);
}

//Finally, retrieving the content in the response body as a strongly-typed object
var user = response.Content;
```

### Using generic interfaces

**Same as Refit**

When using something like ASP.NET Web API, it's a fairly common pattern to have a whole stack of CRUD REST services. Refit now supports these, allowing you to define a single API interface with a generic type:

```csharp
public interface IReallyExcitingCrudApi<T, in TKey> where T : class
{
    [Post("")]
    Task<T> Create([Body] T payload);

    [Get("")]
    Task<List<T>> ReadAll();

    [Get("/{key}")]
    Task<T> ReadOne(TKey key);

    [Put("/{key}")]
    Task Update(TKey key, [Body]T payload);

    [Delete("/{key}")]
    Task Delete(TKey key);
}

[HttpClient]
public partial class ReallyExcitingCrudApiForUser : IReallyExcitingCrudApi<User, string>
{

}
```

Which can be used like this:

```csharp
// The "/users" part here is kind of important if you want it to work for more
// than one type (unless you have a different domain for each type)
var api = new ReallyExcitingCrudApiForUser((new HttpClient()
{
    BaseAddress = new Uri("http://api.example.com/users")
}
));
```

### Interface inheritance

When multiple services that need to be kept separate share a number of APIs, it is possible to leverage interface inheritance to avoid having to define the same Refit methods multiple times in different services:

```csharp
public interface IBaseService
{
    [Get("/resources")]
    Task<Resource> GetResource(string id);
}

public interface IDerivedServiceA : IBaseService
{
    [Delete("/resources")]
    Task DeleteResource(string id);
}

public interface IDerivedServiceB : IBaseService
{
    [Post("/resources")]
    Task<string> AddResource([Body] Resource resource);
}
```

In this example, the `IDerivedServiceA` interface will expose both the `GetResource` and `DeleteResource` APIs, while `IDerivedServiceB` will expose `GetResource` and `AddResource`.

**Notes:** HttpClientFiller don't support dupicated methods in interface inheritance.

This will cause compile error.

```csharp
public interface IBaseService
{
    [Get("/resources")]
    Task<Resource> GetResource(string id);
}

public interface IDerivedServiceA : IBaseService
{
    [Delete("/resources")]
    Task<Resource> GetResource(string id);
    
    [Delete("/resources")]
    Task DeleteResource(string id);
}

```

#### Headers inheritance

When using inheritance, existing header attributes will be passed along as well, and the inner-most ones will have precedence:

```csharp
[Headers("User-Agent: AAA")]
public interface IAmInterfaceA
{
    [Get("/get?result=Ping")]
    Task<string> Ping();
}

[Headers("User-Agent: BBB")]
public interface IAmInterfaceB : IAmInterfaceA
{
    [Get("/get?result=Pang")]
    [Headers("User-Agent: PANG")]
    Task<string> Pang();

    [Get("/get?result=Foo")]
    Task<string> Foo();
}
```

Here, `IAmInterfaceB.Pang()` will use `PANG` as its user agent, while `IAmInterfaceB.Foo` and `IAmInterfaceB.Ping` will use `BBB`.
Note that if `IAmInterfaceB` didn't have a header attribute, `Foo` would then use the `AAA` value inherited from `IAmInterfaceA`.

If an interface is inheriting more than one interface, the order of precedence is the same as the one in which the inherited interfaces are declared.

**Unlike Refit** 

- HttpClientFiller has two different resolution.

1. Inherited interfaces are declared which are **independent** then order of declaration is a precedence.

```csharp

[Headers("User-Agent: AAA")]
public interface IAmInterfaceA
{
    [Get("/get?result=Ping")]
    Task<string> PingA();
}

[Headers("User-Agent: DDD")]
public interface IAmInterfaceD
{
    [Get("/get?result=Ping")]
    Task<string> PingD();
}

public interface IAmInterfaceC : IAmInterfaceA, IAmInterfaceD
{
    [Get("/get?result=Foo")]
    Task<string> Foo();
}
```

Here `IAmInterfaceC.Foo` ("User-Agent: AAA") would use the header attribute inherited from `IAmInterfaceA`, if present, or the one inherited from `IAmInterfaceD`, and so on for all the declared interfaces.

2. Inherited interfaces are declared which are **also inherited** then most inherited has higher precedence.

```csharp

[Headers("User-Agent: AAA")]
public interface IAmInterfaceA
{
    [Get("/get?result=Ping")]
    Task<string> Ping();
}

[Headers("User-Agent: BBB")]
public interface IAmInterfaceB : IAmInterfaceA
{
    [Get("/get?result=Pang")]
    [Headers("User-Agent: PANG")]
    Task<string> Pang();

    [Get("/get?result=Foo")]
    Task<string> Foo();
}

public interface IAmInterfaceC : IAmInterfaceA, IAmInterfaceB
{
    [Get("/get?result=Foo")]
    Task<string> Foo();
}
```

Here `IAmInterfaceC.Foo` ("User-Agent: BBB") would use the header attribute inherited from `IAmInterfaceB`, if present, or the one inherited from `IAmInterfaceA`, and so on for all the declared interfaces.


### Default Interface Methods

Support by default.

### Using HttpClientFactory

Support by default

Since we are concreate implementation. No need for extra helper method.

### Providing a custom HttpClient

Support by default.

**Unlike Refit** 
- HttpClientFiller has no custom setting class like ~~`RefitSettings`~~, so you are free to provider your own custom `HttpClient`. All features are working as the same.

### Intercept HttpRequestMessage

We can modify the `HttpRequestMessage` before it sent.

Sync, async method (with/without CancellationToken) are also supported.

Add one of three methods below in your partial class.

```csharp
public interface IGitHubApi
{
    [Get("/users/{user}")]
    Task<User> GetUser(string user);
}

[HttpClient]
public partial class GitHubApi : IGitHubApi
{
    partial void UpdateHttpRequestMessage(HttpRequestMessage request){
        message.Headers.Add("Vary", "Sec-CH");
    }

    async Task UpdateHttpRequestMessageAsync(HttpRequestMessage message)
    {
        await Task.Yield();
        message.Headers.Add("Vary", "Sec-CH");
    }

    async Task UpdateHttpRequestMessageAsync(HttpRequestMessage message, CancellationToken ct)
    {
       await Task.Yield();
       message.Headers.Add("Vary", "Sec-CH");
    }
}
```

### Handling exceptions

**Unlike Refit**:
- HttpClientFiller support return type for `Task`, `Task<T>`, ~~`Task<IApiResponse>`, `Task<IApiResponse<T>>`~~, or `Task<ApiResponse<T>>`. 
- No custom for any type Exception.
- The exception will be throw as is from .NET libraries (HttpClient, System.Text.Json.JsonSerializer, etc..)

If your return is `Task<ApiResponse<T>>` then:
```csharp
var response = await _myRefitClient.GetSomeStuff();
if(response.IsSuccessStatusCode)
{
   //do your thing
}
else
{
   _logger.LogError(response.Respone);
}
```

Or `Try..catch..` as normal

```csharp
// ...
try
{
   var result = await awesomeApi.GetFooAsync("bar");
}
catch (HttpRequestException exception)
{
   //exception handling
}
// ...
```

#### Providing a custom `ExceptionFactory`

**Unlike Refit**
- HttpClientFiller has no custom ExceptionFactory.
- For more advance option, see `Aspect Oriented Programming to your interface`

#### `ApiException` deconstruction with Serilog

Serilog has no support for HttpClientFiller

#### Aspect Oriented Programming to your interface

By using the nuget package 

https://www.nuget.org/packages/InterfaceFillerCodeGen

you can add custom exception handler, logger or even token refesh to your interface. It's strong typed and also support .NET AOT.


```csharp


public interface IMiniBankApi
{
    [Get("/users/{userId}")]
    Task<User> GetUser(string userId, string token);

    [Post("/users/deposit/{userId}")]
    Task<User> Deposit(string userId, [Body] DepositDto depositDto, string token);

    [Post("/users/deposit/{userId}")]
    Task<User> Withdrawal(string userId, [Body] WithdrawalDto depositDto, string token);
}


[HttpClient]
public partial class MiniBankApi : IMiniBankApi
{
}

public partial class ExceptionHandlerMiniBankApi : IMiniBankApi
{
    [InterfaceFiller]
    private readonly IMiniBankApi _miniBankApi;

    public ExceptionHandlerMiniBankApi(IMiniBankApi miniBankApi)
    {
        _miniBankApi = miniBankApi;
    }

    [Wrapper]
    public async Task<T> HandlerException<T>(Func<Task<T>> nextFunc)
    {
        try
        {
            return await nextFunc();
        }
        catch (HttpRequestException ex)
        {
            //Log
        }
        catch (JsonException ex)
        {
            //Log
        }
        finally
        {
        }

        return default;
    }
}

public interface ITokenService
{
    Task<string> RefreshToken(string userId);
}

public class TokenService : ITokenService
{
    public Task<string> RefreshToken(string userId)
    {
        throw new NotImplementedException();
    }
}

public partial class TokenMiniBankApi : IMiniBankApi
{
    [InterfaceFiller]
    private readonly IMiniBankApi _miniBankApi;
    private readonly ITokenService _tokenService;

    public TokenMiniBankApi(IMiniBankApi miniBankApi, ITokenService tokenService)
    {
        _miniBankApi = miniBankApi;
        _tokenService = tokenService;
    }

    [Wrapper]
    public async Task<T> HandlerTokenExpired<T>(string userId, string token, Func<string, string, Task<T>> nextFunc)
    {
        try
        {
            return await nextFunc(userId, token);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Token is expired."))
        {
            var newToken = await _tokenService.RefreshToken(userId);
            return await nextFunc(userId, newToken);
        }
    }
}


```
Then register in DI

```csharp

builder.Services.AddHttpClient<IMiniBankApi, ExceptionHandlerMiniBankApi>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com");
    
    return new ExceptionHandlerMiniBankApi(
        new TokenMiniBankApi(
            new MiniBankApi(client), new TokenService()
        )
    );
});

```

## Issue Report

https://github.com/ghostnguyen/HttpClientFillerIssue/issues
