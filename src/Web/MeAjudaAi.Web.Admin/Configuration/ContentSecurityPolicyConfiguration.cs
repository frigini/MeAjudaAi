namespace MeAjudaAi.Web.Admin.Configuration;

/// <summary>
/// Configuração de Content Security Policy para o Blazor WASM.
/// Define políticas de segurança para prevenir XSS, data injection e clickjacking.
/// </summary>
public static class ContentSecurityPolicyConfiguration
{
    /// <summary>
    /// Gera a política CSP para ambiente de desenvolvimento.
    /// Mais permissiva para permitir hot reload e debugging.
    /// </summary>
    public static string GetDevelopmentPolicy()
    {
        return string.Join("; ",
            "default-src 'self'",
            "script-src 'self' 'wasm-unsafe-eval' 'unsafe-eval' 'unsafe-inline'", // Necessário para Blazor WASM, Fluxor e scripts inline
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
            "font-src 'self' https://fonts.gstatic.com data:",
            "img-src 'self' data: https:",
            "connect-src 'self' https://localhost:* http://localhost:* ws://localhost:* wss://localhost:*", // Permitir qualquer porta localhost para Aspire
            "frame-src http://localhost:* https://localhost:*", // Permitir iframes do Keycloak
            "media-src 'none'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'none'"
        );
    }

    /// <summary>
    /// Gera a política CSP para ambiente de staging.
    /// Política intermediária para testes antes de produção.
    /// </summary>
    public static string GetStagingPolicy(string apiBaseUrl, string keycloakUrl)
    {
        return string.Join("; ",
            "default-src 'self'",
            "script-src 'self' 'wasm-unsafe-eval'",
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
            "font-src 'self' https://fonts.gstatic.com data:",
            "img-src 'self' data: https:",
            $"connect-src 'self' {apiBaseUrl} {keycloakUrl} wss://*.azurewebsites.net",
            $"frame-src {keycloakUrl}",
            "media-src 'none'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'none'",
            "upgrade-insecure-requests"
        );
    }

    /// <summary>
    /// Gera a política CSP para ambiente de produção.
    /// Política mais restritiva possível.
    /// </summary>
    public static string GetProductionPolicy(string apiBaseUrl, string keycloakUrl, string cdnUrl = "")
    {
        var connectSrc = $"'self' {apiBaseUrl} {keycloakUrl}";
        if (!string.IsNullOrWhiteSpace(cdnUrl))
        {
            connectSrc += $" {cdnUrl}";
        }

        return string.Join("; ",
            "default-src 'self'",
            "script-src 'self' 'wasm-unsafe-eval'",
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
            "font-src 'self' https://fonts.gstatic.com data:",
            "img-src 'self' data: https:",
            $"connect-src {connectSrc}",
            $"frame-src {keycloakUrl}",
            "media-src 'none'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'none'",
            "upgrade-insecure-requests",
            $"report-uri {apiBaseUrl}/api/csp-report", // Legacy report mechanism
            $"report-to {apiBaseUrl}/api/csp-report"   // Modern report mechanism
        );
    }

    /// <summary>
    /// Gera meta tag CSP para inserção no index.html.
    /// </summary>
    public static string GenerateMetaTag(string policy)
    {
        return $"<meta http-equiv=\"Content-Security-Policy\" content=\"{policy}\">";
    }
}
