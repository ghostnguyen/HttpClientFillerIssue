using System.Net.Http.Json;
using System.Text.Json;

namespace HttpClientFillerCore
{
    public static class Helper
    {
        public static void AddHeader(HttpRequestMessage req,
            IEnumerable<KeyValuePair<string, string>> paramHeaders, IEnumerable<KeyValuePair<string, string>> methodHeaders)
        {
            var final = paramHeaders //First to overwrite
                .Concat(methodHeaders)
                .GroupBy(_ => _.Key.ToLower())
                .Select(_ => _.First())
                .Where(_ => !string.IsNullOrEmpty(_.Key) && !string.IsNullOrEmpty(_.Value));

            foreach (var he in final)
            {
                req.Headers.Add(he.Key, he.Value);
            }
        }

        public static void AddPart(MultipartFormDataContent multiPartContent, IEnumerable<(string, object)> keyValueItems, JsonSerializerOptions? options)
        {
            foreach (var item in keyValueItems)
            {
                AddPart(multiPartContent, item.Item1, item.Item2, options);
            }
        }

        public static void AddPart(MultipartFormDataContent multiPartContent, string parameterName, object itemValue, JsonSerializerOptions? options)
        {
            if (itemValue is HttpContent content)
            {
                multiPartContent.Add(content, parameterName, parameterName);
                return;
            }

            if (itemValue is MultipartItem multipartItem)
            {
                var httpContent = multipartItem.ToContent();
                multiPartContent.Add(
                    httpContent,
                    multipartItem.Name ?? parameterName,
                    string.IsNullOrEmpty(multipartItem.FileName) ? parameterName : multipartItem.FileName
                );
                return;
            }

            if (itemValue is Stream streamValue)
            {
                var streamContent = new StreamContent(streamValue);
                multiPartContent.Add(streamContent, parameterName, parameterName);
                return;
            }

            if (itemValue is string stringValue)
            {
                multiPartContent.Add(new StringContent(stringValue), parameterName, parameterName);
                return;
            }

            if (itemValue is FileInfo fileInfoValue)
            {
                var fileContent = new StreamContent(fileInfoValue.OpenRead());
                multiPartContent.Add(fileContent, parameterName, fileInfoValue.Name);
                return;
            }

            if (itemValue is byte[] byteArrayValue)
            {
                var fileContent = new ByteArrayContent(byteArrayValue);
                multiPartContent.Add(fileContent, parameterName, parameterName);
                return;
            }

            if (itemValue is System.Collections.IEnumerable objs)
            {
                foreach (var item in objs)
                {
                    AddPart(multiPartContent, parameterName, item, options);
                }
                return;
            }

            // Fallback to serializer
            multiPartContent.Add(
                    JsonContent.Create(itemValue, mediaType: null, options),
                    parameterName,
                    parameterName
                );
            return;
        }
    }
}