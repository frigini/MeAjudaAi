using MeAjudaAi.ApiService.Services.Orchestration.Interfaces;
using MeAjudaAi.Contracts.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Services.Orchestration;

/// <summary>
/// Serviço que extrai e compõe a configuração do cliente a partir das variáveis de ambiente e configuration files.
/// </summary>
public sealed class ClientConfigurationService(
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<ClientConfigurationService> logger) : IClientConfigurationService
{
    public ClientConfiguration GetClientConfiguration()
    {
        var apiBaseUrl = ExtractApiBaseUrl();
        var keycloakAuthority = ExtractKeycloakAuthority();
        var keycloakClientId = ExtractKeycloakClientId();
        var (_, postLogoutRedirectUri) = ExtractClientBaseUrl();
        var enableFakeAuth = ParseEnableFakeAuth();

        return new ClientConfiguration
        {
            ApiBaseUrl = apiBaseUrl,
            Keycloak = new KeycloakConfiguration
            {
                Authority = keycloakAuthority,
                ClientId = keycloakClientId,
                ResponseType = configuration["Keycloak:ResponseType"] ?? "code",
                Scope = configuration["Keycloak:Scope"] ?? "openid profile email",
                PostLogoutRedirectUri = postLogoutRedirectUri
            },
            External = new ExternalResources
            {
                DocumentationUrl = configuration["External:DocumentationUrl"],
                SupportUrl = configuration["External:SupportUrl"]
            },
            Features = new FeatureFlags
            {
                EnableReduxDevTools = environment.IsDevelopment(),
                EnableDebugMode = environment.IsDevelopment(),
                EnableFakeAuth = enableFakeAuth
            }
        };
    }

    private string ExtractApiBaseUrl()
    {
        var apiBaseUrl = configuration["ApiBaseUrl"]
            ?? configuration["ASPNETCORE_URLS"]?.Split(';').FirstOrDefault()
            ?? "https://localhost:7001";

        return apiBaseUrl.TrimEnd('/');
    }

    private string ExtractKeycloakAuthority()
    {
        var keycloakAuthority = configuration["Keycloak:Authority"]?.TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(keycloakAuthority))
            return keycloakAuthority;

        var keycloakBaseUrl = configuration["Keycloak:BaseUrl"];
        if (string.IsNullOrWhiteSpace(keycloakBaseUrl))
            throw new InvalidOperationException("Keycloak:BaseUrl or Keycloak:Authority must be configured");

        keycloakBaseUrl = keycloakBaseUrl.TrimEnd('/');

        var keycloakRealm = configuration["Keycloak:Realm"];
        if (string.IsNullOrWhiteSpace(keycloakRealm))
            keycloakRealm = "meajudaai";

        keycloakRealm = keycloakRealm.Trim('/');

        return $"{keycloakBaseUrl}/realms/{keycloakRealm}";
    }

    private string ExtractKeycloakClientId()
    {
        return configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Keycloak:ClientId is not configured");
    }

    private (string ClientBaseUrl, string PostLogoutRedirectUri) ExtractClientBaseUrl()
    {
        var clientBaseUrl = configuration["ClientBaseUrl"] ?? "http://localhost:5165";
        var postLogoutRedirectUri = $"{clientBaseUrl.TrimEnd('/')}/";
        return (clientBaseUrl, postLogoutRedirectUri);
    }

    private bool ParseEnableFakeAuth()
    {
        var rawEnableFakeAuth = configuration["FeatureFlags:EnableFakeAuth"]?.Trim();

        if (bool.TryParse(rawEnableFakeAuth, out var parsedValue))
            return environment.IsDevelopment() && parsedValue;

        if (!string.IsNullOrEmpty(rawEnableFakeAuth))
            logger.LogWarning("Invalid value for FeatureFlags:EnableFakeAuth: '{Value}'. Treating as false.", rawEnableFakeAuth);

        return false;
    }
}
