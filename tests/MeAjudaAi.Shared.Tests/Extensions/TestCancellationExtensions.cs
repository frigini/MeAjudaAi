using System.Net.Http;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para facilitar o uso correto de CancellationToken nos testes
/// </summary>
public static class TestCancellationExtensions
{
    /// <summary>
    /// Obtém o CancellationToken do contexto de teste atual
    /// </summary>
    public static CancellationToken GetTestCancellationToken() => TestContext.Current.CancellationToken;
    
    /// <summary>
    /// Extensão para HttpClient.GetAsync com CancellationToken automático
    /// </summary>
    public static Task<HttpResponseMessage> GetTestAsync(this HttpClient client, string requestUri)
        => client.GetAsync(requestUri, GetTestCancellationToken());
    
    /// <summary>
    /// Extensão para HttpClient.PostAsync com CancellationToken automático
    /// </summary>
    public static Task<HttpResponseMessage> PostTestAsync(this HttpClient client, string requestUri, HttpContent content)
        => client.PostAsync(requestUri, content, GetTestCancellationToken());
    
    /// <summary>
    /// Extensão para HttpClient.PutAsync com CancellationToken automático
    /// </summary>
    public static Task<HttpResponseMessage> PutTestAsync(this HttpClient client, string requestUri, HttpContent content)
        => client.PutAsync(requestUri, content, GetTestCancellationToken());
    
    /// <summary>
    /// Extensão para HttpClient.DeleteAsync com CancellationToken automático
    /// </summary>
    public static Task<HttpResponseMessage> DeleteTestAsync(this HttpClient client, string requestUri)
        => client.DeleteAsync(requestUri, GetTestCancellationToken());
}