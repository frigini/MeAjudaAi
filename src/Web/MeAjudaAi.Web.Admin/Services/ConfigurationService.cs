using System.Net.Http.Json;
using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Serviço para buscar configuração do backend ao iniciar o aplicativo.
/// Permite configuração dinâmica sem expor informações sensíveis no wwwroot.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Obtém a configuração do cliente a partir do backend.
    /// </summary>
    Task<ClientConfiguration> GetClientConfigurationAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de configuração que busca do endpoint /api/configuration/client.
/// </summary>
public class ConfigurationService(HttpClient httpClient, ILogger<ConfigurationService> logger) : IConfigurationService
{
    private ClientConfiguration? _cachedConfiguration;

    public async Task<ClientConfiguration> GetClientConfigurationAsync(CancellationToken cancellationToken = default)
    {
        // Retorna do cache se já foi carregado
        if (_cachedConfiguration != null)
            return _cachedConfiguration;

        try
        {
            logger.LogInformation("Fetching client configuration from backend...");

            var response = await httpClient.GetAsync("/api/configuration/client", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Failed to fetch configuration from backend. Status: {response.StatusCode}. " +
                    $"Error: {errorContent}");
            }

            _cachedConfiguration = await response.Content.ReadFromJsonAsync<ClientConfiguration>(cancellationToken)
                ?? throw new InvalidOperationException("Configuration endpoint returned null");

            ValidateConfiguration(_cachedConfiguration);

            logger.LogInformation("Client configuration loaded successfully from backend");
            logger.LogDebug("API Base URL: {ApiBaseUrl}", _cachedConfiguration.ApiBaseUrl);
            logger.LogDebug("Keycloak Authority: {Authority}", _cachedConfiguration.Keycloak.Authority);

            return _cachedConfiguration;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error fetching configuration from backend");
            throw new InvalidOperationException(
                "Cannot connect to the backend API to fetch configuration. " +
                "Please ensure the API is running and accessible.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error fetching configuration");
            throw new InvalidOperationException(
                "Failed to load application configuration from backend.", ex);
        }
    }

    /// <summary>
    /// Valida a configuração recebida do backend.
    /// </summary>
    private void ValidateConfiguration(ClientConfiguration config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.ApiBaseUrl))
            errors.Add("ApiBaseUrl is missing");

        if (string.IsNullOrWhiteSpace(config.Keycloak.Authority))
            errors.Add("Keycloak Authority is missing");

        if (string.IsNullOrWhiteSpace(config.Keycloak.ClientId))
            errors.Add("Keycloak ClientId is missing");

        if (string.IsNullOrWhiteSpace(config.Keycloak.PostLogoutRedirectUri))
            errors.Add("Keycloak PostLogoutRedirectUri is missing");

        if (!Uri.TryCreate(config.ApiBaseUrl, UriKind.Absolute, out _))
            errors.Add("ApiBaseUrl is not a valid URI");

        if (!Uri.TryCreate(config.Keycloak.Authority, UriKind.Absolute, out _))
            errors.Add("Keycloak Authority is not a valid URI");

        if (errors.Any())
        {
            var errorMessage = "Configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }
}
