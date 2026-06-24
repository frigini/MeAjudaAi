using MeAjudaAi.Contracts.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Services.Orchestration;

public interface IClientConfigurationService
{
    ClientConfiguration GetClientConfiguration();
}

public sealed class ClientConfigurationService : IClientConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ClientConfigurationService> _logger;

    public ClientConfigurationService(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<ClientConfigurationService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public ClientConfiguration GetClientConfiguration()
    {
        var apiBaseUrl = _configuration["ApiBaseUrl"]
            ?? _configuration["ASPNETCORE_URLS"]?.Split(';').FirstOrDefault()
            ?? "https://localhost:7001";

        apiBaseUrl = apiBaseUrl.TrimEnd('/');

        var keycloakAuthority = _configuration["Keycloak:Authority"]?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(keycloakAuthority))
        {
            var keycloakBaseUrl = _configuration["Keycloak:BaseUrl"];
            if (string.IsNullOrWhiteSpace(keycloakBaseUrl))
                throw new InvalidOperationException("Keycloak:BaseUrl ou Keycloak:Authority deve estar configurado");

            keycloakBaseUrl = keycloakBaseUrl.TrimEnd('/');

            var keycloakRealm = _configuration["Keycloak:Realm"];
            if (string.IsNullOrWhiteSpace(keycloakRealm))
                keycloakRealm = "meajudaai";

            keycloakRealm = keycloakRealm.Trim('/');

            keycloakAuthority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        }

        var keycloakClientId = _configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Keycloak:ClientId não configurado");

        var clientBaseUrl = _configuration["ClientBaseUrl"] ?? "http://localhost:5165";
        var postLogoutRedirectUri = $"{clientBaseUrl.TrimEnd('/')}/";

        var rawEnableFakeAuth = _configuration["FeatureFlags:EnableFakeAuth"]?.Trim();
        if (!bool.TryParse(rawEnableFakeAuth, out var enableFakeAuth) && !string.IsNullOrEmpty(rawEnableFakeAuth))
        {
            _logger.LogWarning("Invalid value for FeatureFlags:EnableFakeAuth: '{Value}'. Treating as false.", rawEnableFakeAuth);
        }

        return new ClientConfiguration
        {
            ApiBaseUrl = apiBaseUrl,
            Keycloak = new KeycloakConfiguration
            {
                Authority = keycloakAuthority,
                ClientId = keycloakClientId,
                ResponseType = _configuration["Keycloak:ResponseType"] ?? "code",
                Scope = _configuration["Keycloak:Scope"] ?? "openid profile email",
                PostLogoutRedirectUri = postLogoutRedirectUri
            },
            External = new ExternalResources
            {
                DocumentationUrl = _configuration["External:DocumentationUrl"],
                SupportUrl = _configuration["External:SupportUrl"]
            },
            Features = new FeatureFlags
            {
                EnableReduxDevTools = _environment.IsDevelopment(),
                EnableDebugMode = _environment.IsDevelopment(),
                EnableFakeAuth = enableFakeAuth
            }
        };
    }
}
