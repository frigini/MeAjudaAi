using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.AppHost.Services;

/// <summary>
/// Hosted Service interno ao AppHost que aguarda o recurso do Keycloak inicializar,
/// e então utiliza a REST API do Keycloak para garantir que os clientes OIDC necessários existam.
/// </summary>
public class KeycloakBootstrapService(
    ResourceNotificationService resourceNotificationService,
    ILogger<KeycloakBootstrapService> logger,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("KeycloakBootstrapService is waiting for Keycloak to become healthy/running...");

        try
        {
            await foreach (var resourceEvent in resourceNotificationService.WatchAsync(stoppingToken))
            {
                if (resourceEvent.Resource.Name == "keycloak" &&
                    resourceEvent.Snapshot.State?.Text == "Running")
                {
                    // Aguardar alguns segundos para o servidor HTTP interno iniciar completamente
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

                    try
                    {
                        if (resourceEvent.Resource is IResourceWithEndpoints resourceWithEndpoints)
                        {
                            var endpoint = resourceWithEndpoints.GetEndpoints().FirstOrDefault(e => e.EndpointName == "http" || e.EndpointName == "https");
                            if (endpoint != null && endpoint.IsAllocated)
                            {
                                var url = $"{endpoint.Scheme}://{endpoint.Host}:{endpoint.Port}";
                                logger.LogInformation("Keycloak is running at {Url}. Starting bootstrap process...", url);

                                var success = await BootstrapKeycloakAsync(url, stoppingToken);
                                
                                // Interrompe o loop somente após sucesso na configuração
                                if (success)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Failed to bootstrap Keycloak clients.");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignorar exceção de cancelamento durante o shutdown normal
            logger.LogInformation("KeycloakBootstrapService execution was cancelled.");
        }
    }

    private async Task<bool> BootstrapKeycloakAsync(string baseUrl, CancellationToken ct)
    {
        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            // 1. Obter Token Admin
            var token = await GetAdminTokenAsync(httpClient, ct);
            if (string.IsNullOrEmpty(token))
            {
                logger.LogError("Could not obtain Keycloak admin token.");
                return false;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Carregar URLs dos clientes da configuração com fallbacks padrão
            var adminUrl = configuration["AdminPortalUrl"] ?? "http://localhost:5030";
            var customerUrl = configuration["CustomerWebUrl"] ?? "http://localhost:3000";

            // 2. Garantir existência dos Clientes
            await EnsureClientExistsAsync(httpClient, "admin-portal", adminUrl, ct);
            await EnsureClientExistsAsync(httpClient, "customer-web", customerUrl, ct);
            
            logger.LogInformation("Keycloak bootstrap completed successfully.");
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            logger.LogWarning(ex, "Transient error connecting to Keycloak during bootstrap.");
            return false;
        }
    }

    private async Task<string?> GetAdminTokenAsync(HttpClient httpClient, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/realms/master/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", "admin"),      // Configuração dev local
                new KeyValuePair<string, string>("password", "admin123"),   // Configuração dev local
                new KeyValuePair<string, string>("grant_type", "password")
            ])
        };

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to get token: {Status}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        var json = JsonNode.Parse(content);
        return json?["access_token"]?.GetValue<string>();
    }

    private async Task EnsureClientExistsAsync(HttpClient httpClient, string clientId, string baseUrl, CancellationToken ct)
    {
        // Verificar se existe
        var getResponse = await httpClient.GetAsync($"/admin/realms/MeAjudaAi/clients?clientId={clientId}", ct);
        if (getResponse.IsSuccessStatusCode)
        {
            var content = await getResponse.Content.ReadAsStringAsync(ct);
            var clients = JsonNode.Parse(content) as JsonArray;

            if (clients != null && clients.Count > 0)
            {
                logger.LogInformation("Client {ClientId} already exists.", clientId);
                return;
            }
        }

        logger.LogInformation("Creating client {ClientId}...", clientId);

        // Criar novo cliente
        var newClient = new
        {
            clientId = clientId,
            enabled = true,
            publicClient = true,
            directAccessGrantsEnabled = true,
            standardFlowEnabled = true,
            implicitFlowEnabled = false,
            rootUrl = baseUrl,
            validRedirectUris = new[] { $"{baseUrl}/*" },
            webOrigins = new[] { "+" },
            protocol = "openid-connect"
        };

        var jsonPayload = JsonSerializer.Serialize(newClient);
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/realms/MeAjudaAi/clients")
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        var postResponse = await httpClient.SendAsync(postRequest, ct);
        if (postResponse.IsSuccessStatusCode)
        {
            logger.LogInformation("Client {ClientId} created successfully.", clientId);
        }
        else
        {
            logger.LogError("Failed to create client {ClientId}. Status: {Status}", clientId, postResponse.StatusCode);
        }
    }
}
