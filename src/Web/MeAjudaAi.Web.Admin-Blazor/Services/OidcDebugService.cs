using Microsoft.AspNetCore.Components;
using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Serviço para diagnosticar problemas com OIDC e Keycloak.
/// Fornece informações detalhadas sobre a configuração de autenticação.
/// </summary>
public class OidcDebugService
{
    private readonly ClientConfiguration _clientConfig;
    private readonly ILogger<OidcDebugService> _logger;
    private readonly NavigationManager _navigationManager;

    public OidcDebugService(
        ClientConfiguration clientConfig,
        ILogger<OidcDebugService> logger,
        NavigationManager navigationManager)
    {
        _clientConfig = clientConfig;
        _logger = logger;
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Verifica se o Keycloak está acessível e retorna informações de diagnóstico.
    /// </summary>
    public async Task<KeycloakHealthCheckResult> CheckKeycloakHealthAsync()
    {
        var result = new KeycloakHealthCheckResult
        {
            Authority = _clientConfig.Keycloak.Authority,
            ClientId = _clientConfig.Keycloak.ClientId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Construir URLs de diagnóstico
            var wellKnownUrl = $"{_clientConfig.Keycloak.Authority}/.well-known/openid-configuration";
            var tokenUrl = $"{_clientConfig.Keycloak.Authority}/protocol/openid-connect/token";
            var authUrl = $"{_clientConfig.Keycloak.Authority}/protocol/openid-connect/auth";

            _logger.LogInformation("?? Iniciando diagnóstico de Keycloak");
            _logger.LogInformation("   Authority: {Authority}", _clientConfig.Keycloak.Authority);
            _logger.LogInformation("   Client ID: {ClientId}", _clientConfig.Keycloak.ClientId);
            _logger.LogInformation("   Well-Known URL: {WellKnownUrl}", wellKnownUrl);

            // Tentar acessar o endpoint .well-known
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            try
            {
                var response = await client.GetAsync(wellKnownUrl);
                result.WellKnownEndpointStatus = $"{(int)response.StatusCode} {response.StatusCode}";
                result.IsWellKnownAccessible = response.IsSuccessStatusCode;

                _logger.LogInformation(
                    "   Well-Known Endpoint: {Status}",
                    result.WellKnownEndpointStatus);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    result.WellKnownContent = content;
                    _logger.LogDebug("   Well-Known Content: {Content}", content[..Math.Min(500, content.Length)]);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "   Well-Known Error: {Status} - {Content}",
                        response.StatusCode,
                        content[..Math.Min(500, content.Length)]);
                }
            }
            catch (HttpRequestException ex)
            {
                result.WellKnownEndpointStatus = $"Error: {ex.Message}";
                result.IsWellKnownAccessible = false;
                _logger.LogError(
                    ex,
                    "   ? Erro ao acessar Well-Known Endpoint: {Error}",
                    ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                result.WellKnownEndpointStatus = $"Timeout: {ex.Message}";
                result.IsWellKnownAccessible = false;
                _logger.LogError(
                    ex,
                    "   ?? Timeout ao acessar Well-Known Endpoint: {Error}",
                    ex.Message);
            }

            // Verificar URLs de configuração
            result.TokenEndpointUrl = tokenUrl;
            result.AuthorizationEndpointUrl = authUrl;
            result.PostLogoutRedirectUri = _clientConfig.Keycloak.PostLogoutRedirectUri;
            result.Scopes = _clientConfig.Keycloak.Scope?.Split(' ') ?? Array.Empty<string>();

            // Verificar navegador
            result.CurrentUrl = _navigationManager.Uri;
            result.AppOrigin = new Uri(_navigationManager.Uri).GetLeftPart(System.UriPartial.Authority);

            _logger.LogInformation("? Diagnóstico de Keycloak concluído");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erro durante diagnóstico de Keycloak");
            result.Errors.Add($"Erro geral: {ex.Message}");
            return result;
        }
    }
}

/// <summary>
/// Resultado da verificação de saúde do Keycloak.
/// </summary>
public class KeycloakHealthCheckResult
{
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsWellKnownAccessible { get; set; }
    public string WellKnownEndpointStatus { get; set; } = "Pending";
    public string? WellKnownContent { get; set; }
    public string TokenEndpointUrl { get; set; } = string.Empty;
    public string AuthorizationEndpointUrl { get; set; } = string.Empty;
    public string PostLogoutRedirectUri { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string CurrentUrl { get; set; } = string.Empty;
    public string AppOrigin { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public bool IsHealthy => IsWellKnownAccessible && Errors.Count == 0;

    public string GetDiagnosticSummary()
    {
        var lines = new List<string>
        {
            "???????????????????????????????????????????????????????????",
            "?? KEYCLOAK DIAGNOSTIC REPORT",
            "???????????????????????????????????????????????????????????",
            $"Timestamp: {Timestamp:O}",
            "",
            "Configuration:",
            $"  Authority: {Authority}",
            $"  Client ID: {ClientId}",
            $"  Scopes: {string.Join(", ", Scopes)}",
            $"  Post Logout Redirect: {PostLogoutRedirectUri}",
            "",
            "Endpoints:",
            $"  Well-Known: {WellKnownEndpointStatus}",
            $"  Token: {TokenEndpointUrl}",
            $"  Authorization: {AuthorizationEndpointUrl}",
            "",
            "Client Information:",
            $"  Current URL: {CurrentUrl}",
            $"  App Origin: {AppOrigin}",
            "",
            $"Status: {(IsHealthy ? "? Healthy" : "? Issues Detected")}",
        };

        if (Errors.Count > 0)
        {
            lines.Add("");
            lines.Add("Errors:");
            lines.AddRange(Errors.Select(e => $"  - {e}"));
        }

        lines.Add("???????????????????????????????????????????????????????????");

        return string.Join("\n", lines);
    }
}
