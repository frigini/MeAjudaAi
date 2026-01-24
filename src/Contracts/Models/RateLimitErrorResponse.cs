namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Modelo para erros de rate limiting.
/// </summary>
public class RateLimitErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância para erro de rate limit.
    /// </summary>
    public RateLimitErrorResponse()
    {
        StatusCode = 429;
        Title = "Too Many Requests";
        Detail = "Muitas requisições realizadas. Tente novamente mais tarde.";
    }

    /// <summary>
    /// Tempo de espera recomendado em segundos.
    /// </summary>
    /// <example>60</example>
    public int? RetryAfterSeconds { get; set; }

    /// <summary>
    /// Limite de requests por minuto para o usuário.
    /// </summary>
    /// <example>200</example>
    public int? RequestLimit { get; set; }

    /// <summary>
    /// Requests restantes no período atual.
    /// </summary>
    /// <example>0</example>
    public int? RequestsRemaining { get; set; }
}
