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