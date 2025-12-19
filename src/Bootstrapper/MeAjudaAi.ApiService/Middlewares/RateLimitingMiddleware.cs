using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware de Rate Limiting com suporte a usuários autenticados.
/// Implementa limitação de taxa de requisições com base em IP, usuário autenticado, role e endpoint.
/// </summary>
/// <remarks>
/// <para><b>Configuração</b>: Seção "AdvancedRateLimit" no appsettings.json</para>
/// <para><b>Limites Padrão</b>:</para>
/// <list type="bullet">
///   <item>Anônimos: 30 req/min, 300 req/hora, 1000 req/dia</item>
///   <item>Autenticados: 120 req/min, 2000 req/hora, 10000 req/dia</item>
///   <item>Por Role: Configurável via RoleLimits (ex: Admin com limites maiores)</item>
///   <item>Por Endpoint: Configurável via EndpointLimits (ex: /api/auth/* com limite menor)</item>
/// </list>
/// <para><b>Whitelist de IPs</b>: Configurável para bypass (ex: load balancers, health checks)</para>
/// <para><b>Resposta ao Exceder Limite</b>:</para>
/// <list type="bullet">
///   <item>Status Code: 429 Too Many Requests</item>
///   <item>Header Retry-After: tempo em segundos até liberação</item>
///   <item>Body JSON com mensagem de erro e detalhes</item>
/// </list>
/// <para><b>Thread-Safety</b>: Usa Interlocked.Increment para incremento atômico de contadores</para>
/// </remarks>
public class RateLimitingMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    IOptionsMonitor<RateLimitOptions> options,
    ILogger<RateLimitingMiddleware> logger)
{
    /// <summary>
    /// Cache de padrões Regex compilados para performance. Limitado a 1000 entradas para prevenir memory leaks.
    /// Em configurações normais, o número de padrões de endpoint é pequeno (&lt;100), mas esse limite
    /// previne crescimento descontrolado se padrões forem adicionados dinamicamente.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Regex> _patternCache = new();
    private const int MaxPatternCacheSize = 1000;

    /// <summary>
    /// Classe contador simples para rate limiting.
    /// <para>
    /// <b>Thread-safety:</b> O campo <see cref="Value"/> deve ser acessado ou modificado apenas usando operações thread-safe,
    /// como <see cref="System.Threading.Interlocked.Increment(ref int)"/>. Esta classe foi projetada para ser usada em um ambiente concorrente,
    /// e todas as modificações no <see cref="Value"/> devem ser realizadas atomicamente.
    /// </para>
    /// </summary>
    private sealed class Counter
    {
        public int Value;
        public DateTime ExpiresAt;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var currentOptions = options.CurrentValue;

        // Ignora rate limiting se explicitamente desabilitado
        if (!currentOptions.General.Enabled)
        {
            await next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context);
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

        // Verifica whitelist de IPs primeiro - ignora rate limiting se IP estiver na whitelist
        if (currentOptions.General.EnableIpWhitelist &&
            currentOptions.General.WhitelistedIps.Contains(clientIp))
        {
            await next(context);
            return;
        }

        // Garante janela mínima de 1 segundo por segurança
        var windowSeconds = Math.Max(1, currentOptions.General.WindowInSeconds);
        var effectiveWindow = TimeSpan.FromSeconds(windowSeconds);

        // Determina limite efetivo usando ordem de prioridade
        var limit = GetEffectiveLimit(context, currentOptions, isAuthenticated, effectiveWindow);

        // Chave por usuário (quando autenticado) e método para reduzir false sharing
        var userKey = isAuthenticated
            ? (context.User.FindFirst("sub")?.Value ?? context.User.Identity?.Name ?? clientIp)
            : clientIp;
        
        // Use route template when available to prevent memory pressure from dynamic path parameters
        var endpoint = context.GetEndpoint();
        var routeEndpoint = endpoint as RouteEndpoint;
        var pathKey = routeEndpoint?.RoutePattern.RawText ?? context.Request.Path.ToString();
        
        var key = $"rate_limit:{userKey}:{context.Request.Method}:{pathKey}";

        var counter = cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = effectiveWindow;
            return new Counter { ExpiresAt = DateTime.UtcNow + effectiveWindow };
        })!; // GetOrCreate nunca retorna null quando factory retorna um valor

        var current = Interlocked.Increment(ref counter.Value);

        if (current > limit)
        {
            logger.LogWarning("Rate limit exceeded for client {ClientIp} on path {Path}. Limit: {Limit}, Current count: {Count}, Window: {Window}s",
                clientIp, context.Request.Path, limit, current, windowSeconds);
            await HandleRateLimitExceeded(context, counter, currentOptions.General.ErrorMessage, (int)effectiveWindow.TotalSeconds);
            return;
        }

        // TTL definido na criação; sem necessidade de operação redundante de cache
        var warnThreshold = (int)Math.Ceiling(limit * 0.8);
        if (current >= warnThreshold) // aproximando do limite (80%)
        {
            logger.LogInformation("Client {ClientIp} approaching rate limit on path {Path}. Current: {Count}/{Limit}, Window: {Window}s",
                clientIp, context.Request.Path, current, limit, currentOptions.General.WindowInSeconds);
        }

        await next(context);
    }

    private int GetEffectiveLimit(HttpContext context, RateLimitOptions rateLimitOptions, bool isAuthenticated, TimeSpan window)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;

        // 1. Verifica limites específicos de endpoint primeiro com ordenação determinística
        // Ordena por: padrões mais longos primeiro (mais específicos), depois exatos antes de wildcards
        var matchingLimit = rateLimitOptions.EndpointLimits
            .OrderByDescending(e => e.Value.Pattern.Length)
            .ThenBy(e => e.Value.Pattern.Contains('*') ? 1 : 0)
            .FirstOrDefault(endpointLimit =>
                IsPathMatch(requestPath, endpointLimit.Value.Pattern) &&
                ((isAuthenticated && endpointLimit.Value.ApplyToAuthenticated) ||
                 (!isAuthenticated && endpointLimit.Value.ApplyToAnonymous)));

        if (matchingLimit.Value != null)
        {
            return ScaleToWindow(
                matchingLimit.Value.RequestsPerMinute,
                matchingLimit.Value.RequestsPerHour,
                0,
                window);
        }

        // 2. Verifica limites específicos de role (apenas para usuários autenticados)
        if (isAuthenticated)
        {
            var userRoles = context.User.FindAll("role")?.Select(c => c.Value) ??
                           context.User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Select(c => c.Value) ??
                           [];

            // Usa o limite mais permissivo (maior) entre todas as roles do usuário
            int? maxRoleLimit = null;
            foreach (var role in userRoles)
            {
                if (rateLimitOptions.RoleLimits.TryGetValue(role, out var roleLimit))
                {
                    var limit = ScaleToWindow(
                        roleLimit.RequestsPerMinute,
                        roleLimit.RequestsPerHour,
                        roleLimit.RequestsPerDay,
                        window);
                    
                    if (maxRoleLimit == null || limit > maxRoleLimit)
                        maxRoleLimit = limit;
                }
            }
            
            if (maxRoleLimit.HasValue)
                return maxRoleLimit.Value;
        }

        // 3. Usa limites padrão de autenticado/anônimo como fallback
        return isAuthenticated
            ? ScaleToWindow(rateLimitOptions.Authenticated.RequestsPerMinute, rateLimitOptions.Authenticated.RequestsPerHour, rateLimitOptions.Authenticated.RequestsPerDay, window)
            : ScaleToWindow(rateLimitOptions.Anonymous.RequestsPerMinute, rateLimitOptions.Anonymous.RequestsPerHour, rateLimitOptions.Anonymous.RequestsPerDay, window);
    }

    private static int ScaleToWindow(int perMinute, int perHour, int perDay, TimeSpan window)
    {
        var secs = Math.Max(1, (int)window.TotalSeconds);
        var candidates = new List<double>(3);
        if (perMinute > 0) candidates.Add(perMinute * secs / 60.0);
        if (perHour > 0) candidates.Add(perHour * secs / 3600.0);
        if (perDay > 0) candidates.Add(perDay * secs / 86400.0);
        var allowed = candidates.Count > 0 ? candidates.Min() : 0.0;
        return Math.Max(1, (int)Math.Floor(allowed));
    }

    private bool IsPathMatch(string requestPath, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return false;

        // Correspondência simples de wildcard - pode ser melhorado para padrões mais complexos
        if (pattern.Contains('*'))
        {
            // Prevenir memory leak: limitar cache a MaxPatternCacheSize entradas
            if (_patternCache.Count >= MaxPatternCacheSize && !_patternCache.ContainsKey(pattern))
            {
                // Log warning apenas uma vez quando o limite é atingido
                logger.LogWarning(
                    "Pattern cache size limit reached ({MaxSize}). Pattern '{Pattern}' will be compiled on-demand without caching.",
                    MaxPatternCacheSize,
                    pattern);
                
                // Compilar sem adicionar ao cache
                var escaped = Regex.Escape(pattern).Replace(@"\*", ".*");
                var regex = new Regex($"^{escaped}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return regex.IsMatch(requestPath);
            }

            var cachedRegex = _patternCache.GetOrAdd(pattern, p =>
            {
                var escaped = Regex.Escape(p).Replace(@"\*", ".*");
                return new Regex($"^{escaped}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            });
            return cachedRegex.IsMatch(requestPath);
        }

        return string.Equals(requestPath, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Use the IP already resolved by ForwardedHeadersMiddleware
        // which validates trusted proxies via KnownProxies/KnownNetworks.
        // This prevents malicious clients from spoofing whitelisted IPs or
        // rotating fake IPs to evade per-IP rate limits.
        // ForwardedHeadersMiddleware must be configured in the pipeline before
        // this middleware with appropriate KnownProxies/KnownNetworks settings.
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static async Task HandleRateLimitExceeded(HttpContext context, Counter counter, string errorMessage, int windowInSeconds)
    {
        // Calcula TTL restante da expiração do contador
        var retryAfterSeconds = Math.Max(0, (int)Math.Ceiling((counter.ExpiresAt - DateTime.UtcNow).TotalSeconds));

        context.Response.StatusCode = 429;
        context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Error = "RateLimitExceeded",
            Message = errorMessage,
            Details = new Dictionary<string, object>
            {
                ["retryAfterSeconds"] = retryAfterSeconds,
                ["windowInSeconds"] = windowInSeconds
            }
        };

        var json = JsonSerializer.Serialize(errorResponse, SerializationDefaults.Api);

        await context.Response.WriteAsync(json);
    }
}
