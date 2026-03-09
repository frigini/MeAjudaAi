using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.AppHost.Services;

/// <summary>
/// Hosted Service within the AppHost that listens for the Keycloak resource to start,
/// then uses the Keycloak REST API to ensure that necessary OIDC clients (admin-portal, customer-web) exist.
/// </summary>
public class KeycloakBootstrapService(
    ResourceNotificationService resourceNotificationService,
    ILogger<KeycloakBootstrapService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("KeycloakBootstrapService is waiting for Keycloak to become healthy/running...");

        await foreach (var resourceEvent in resourceNotificationService.WatchAsync(stoppingToken))
        {
            if (resourceEvent.Resource.Name == "keycloak" &&
                resourceEvent.Snapshot.State?.Text == "Running")
            {
                // Give Keycloak a few seconds to fully start its internal HTTP server
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

                            await BootstrapKeycloakAsync(url, stoppingToken);
                            
                            // Break after successful bootstrap so we don't repeat this
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to bootstrap Keycloak clients.");
                }
            }
        }
    }

    private async Task BootstrapKeycloakAsync(string baseUrl, CancellationToken ct)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

        // 1. Get Admin Token
        var token = await GetAdminTokenAsync(httpClient, ct);
        if (string.IsNullOrEmpty(token))
        {
            logger.LogError("Could not obtain Keycloak admin token.");
            return;
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Ensure Clients
        await EnsureClientExistsAsync(httpClient, "admin-portal", "http://localhost:5030", ct);
        await EnsureClientExistsAsync(httpClient, "customer-web", "http://localhost:3000", ct);
        
        logger.LogInformation("Keycloak bootstrap completed successfully.");
    }

    private async Task<string?> GetAdminTokenAsync(HttpClient httpClient, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/realms/master/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", "admin"),      // Local dev config
                new KeyValuePair<string, string>("password", "admin123"),   // Local dev config
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
        // Check if exists
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

        // Create new client
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
