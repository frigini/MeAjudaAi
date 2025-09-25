namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Extensões para HttpClient facilitar configuração de autenticação
/// </summary>
public static class HttpClientAuthExtensions
{
    /// <summary>
    /// Configura Authorization header para simular usuário autenticado
    /// </summary>
    public static HttpClient WithAuthorizationHeader(this HttpClient client, string token = "fake-token")
    {
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Remove Authorization header para simular usuário anônimo
    /// </summary>
    public static HttpClient WithoutAuthorizationHeader(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    /// <summary>
    /// Configura como admin (adiciona Authorization header)
    /// </summary>
    public static HttpClient AsAdmin(this HttpClient client)
    {
        return client.WithAuthorizationHeader("admin-token");
    }

    /// <summary>
    /// Configura como usuário normal (adiciona Authorization header)
    /// </summary>
    public static HttpClient AsUser(this HttpClient client)
    {
        return client.WithAuthorizationHeader("user-token");
    }

    /// <summary>
    /// Configura como usuário anônimo (remove Authorization header)
    /// </summary>
    public static HttpClient AsAnonymous(this HttpClient client)
    {
        return client.WithoutAuthorizationHeader();
    }
}