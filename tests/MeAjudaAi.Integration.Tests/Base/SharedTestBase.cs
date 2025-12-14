using System.Net.Http.Headers;
using System.Text.Json;
using Bogus;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Base class compartilhada para testes de integração com máxima reutilização de recursos
/// </summary>
public abstract class SharedTestBase(SharedTestFixture sharedFixture) : IAsyncLifetime, IClassFixture<SharedTestFixture>
{
    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    public virtual async ValueTask InitializeAsync()
    {
        // Usa o fixture compartilhado que já está inicializado
        await sharedFixture.InitializeAsync();

        // Reutiliza o client HTTP do fixture
        ApiClient = sharedFixture.GetOrCreateHttpClient("apiservice");

        // Verificação rápida de saúde (opcional, só se necessário)
        if (!await sharedFixture.IsApiHealthyAsync())
        {
            await Task.Delay(1000); // Aguarda brevemente e tenta novamente
            if (!await sharedFixture.IsApiHealthyAsync())
            {
                throw new InvalidOperationException("API não está saudável no fixture compartilhado");
            }
        }
    }

    // Métodos helper otimizados
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, sharedFixture.JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PostAsync(requestUri, content, cancellationToken);
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, sharedFixture.JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PutAsync(requestUri, content, cancellationToken);
    }

    protected async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, sharedFixture.JsonOptions)!;
    }

    protected void SetAuthorizationHeader(string token)
    {
        ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthorizationHeader()
    {
        ApiClient.DefaultRequestHeaders.Authorization = null;
    }

    public virtual ValueTask DisposeAsync()
    {
        // Não dispose do ApiClient aqui - ele é compartilhado
        // Apenas limpar headers específicos do teste se necessário
        ClearAuthorizationHeader();
        return ValueTask.CompletedTask;
    }

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(Uri requestUri, T value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
