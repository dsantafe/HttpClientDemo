namespace HttpClientDemo.Services
{
    public class HttpService(IHttpClientFactory httpClientFactory, ILogger<HttpService> logger)
    {
        public async Task<string> SendAsync(
            string clientName,
            HttpMethod method,
            string url,
            object? body = null,
            string? bearerToken = null,
            (string user, string pass)? basicAuth = null,
            string contentType = "json")
        {
            var client = httpClientFactory.CreateClient(clientName);
            using var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(bearerToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            if (basicAuth.HasValue)
            {
                var bytes = Encoding.UTF8.GetBytes($"{basicAuth.Value.user}:{basicAuth.Value.pass}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
            }

            if (body != null && method != HttpMethod.Get && method != HttpMethod.Delete)
            {
                string serialized;
                string mediaType;

                switch (contentType.ToLower())
                {
                    case "xml":
                        var serializer = new XmlSerializer(body.GetType());
                        using (var stringWriter = new StringWriter())
                        {
                            serializer.Serialize(stringWriter, body);
                            serialized = stringWriter.ToString();
                        }
                        mediaType = "application/xml";
                        break;

                    case "form":
                        if (body is Dictionary<string, string> dict)
                        {
                            request.Content = new FormUrlEncodedContent(dict);
                            serialized = string.Join("&", dict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                            mediaType = "application/x-www-form-urlencoded";
                        }
                        else
                        {
                            throw new InvalidOperationException("Form content requires Dictionary<string,string>");
                        }
                        break;

                    default:
                        serialized = JsonSerializer.Serialize(body);
                        mediaType = "application/json";
                        break;
                }

                request.Content ??= new StringContent(serialized, Encoding.UTF8, mediaType);
            }

            logger.LogInformation("➡️ Sending {Method} request to {Url} with client '{ClientName}'", method, url, clientName);

            try
            {
                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                logger.LogInformation("Success {Method} {Url} - Status {StatusCode}", method, url, response.StatusCode);

                return content;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {Method} {Url}", method, url);
                throw;
            }
        }

        public async Task<T?> SendAsync<T>(
            string clientName,
            HttpMethod method,
            string url,
            string? body = null,
            string? bearerToken = null,
            (string user, string pass)? basicAuth = null,
            string contentType = "application/json",
            string responseFormat = "json")
        {
            var responseString = await SendAsync(clientName, method, url, body, bearerToken, basicAuth, contentType);

            if (responseFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<T>(responseString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else if (responseFormat.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StringReader(responseString);
                return (T?)serializer.Deserialize(reader);
            }

            throw new NotSupportedException($"Response format '{responseFormat}' not supported.");
        }
    }
}
