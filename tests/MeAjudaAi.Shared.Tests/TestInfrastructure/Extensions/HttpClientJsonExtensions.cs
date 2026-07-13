using System.Net.Http.Headers;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Extensions;

/// <summary>
/// Extension methods for HTTP JSON operations in tests.
/// Extracted from E2E.TestContainerFixture for reuse across test projects.
/// </summary>
public static class HttpClientJsonExtensions
{
    private static JsonSerializerOptions JsonOptions => Shared.Serialization.SerializationDefaults.Api;

    public static async Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        return await client.PostAsync(requestUri, stringContent);
    }

    public static async Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        return await client.PutAsync(requestUri, stringContent);
    }

    public static async Task<HttpResponseMessage> PatchJsonAsync<T>(this HttpClient client, string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        return await client.PatchAsync(requestUri, stringContent);
    }

    public static async Task<T?> ReadJsonAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
            return default;

        var json = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        var type = typeof(T);
        bool isWrapperType = type.IsGenericType && (type.GetGenericTypeDefinition().Name == "Result`1" || type.GetGenericTypeDefinition().Name == "Response`1");

        if (!isWrapperType && json.ValueKind == JsonValueKind.Object && json.TryGetProperty("data", out var data) && data.ValueKind != JsonValueKind.Null)
            return JsonSerializer.Deserialize<T>(data.GetRawText(), JsonOptions);

        if (!isWrapperType && json.ValueKind == JsonValueKind.Object && json.TryGetProperty("value", out var value) && json.TryGetProperty("isSuccess", out var isSuccess) && isSuccess.ValueKind == JsonValueKind.True)
            return JsonSerializer.Deserialize<T>(value.GetRawText(), JsonOptions);

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Extracts the data object from a JSON response, supporting { "value": {...} }, { "data": {...} } or direct object formats.
    /// </summary>
    public static JsonElement GetResponseData(this JsonElement response)
    {
        if (response.TryGetProperty("value", out var value))
            return value;
        if (response.TryGetProperty("data", out var data))
            return data;
        return response;
    }
}
