namespace MeAjudaAi.Contracts.Functional;

/// <summary>
/// Representa um erro com mensagem e código de status HTTP.
/// </summary>
/// <param name="Message">Mensagem descritiva do erro</param>
/// <param name="StatusCode">Código de status HTTP (padrão: 400)</param>
public record Error(string Message, int StatusCode = 400)
{
    /// <summary>
    /// Cria um erro Not Found (404).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <returns>Erro com StatusCode 404</returns>
    public static Error NotFound(string message) => new(message, 404);
    
    /// <summary>
    /// Cria um erro Bad Request (400).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <returns>Erro com StatusCode 400</returns>
    public static Error BadRequest(string message) => new(message, 400);
    
    /// <summary>
    /// Cria um erro Unauthorized (401).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <returns>Erro com StatusCode 401</returns>
    public static Error Unauthorized(string message) => new(message, 401);
    
    /// <summary>
    /// Cria um erro Forbidden (403).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <returns>Erro com StatusCode 403</returns>
    public static Error Forbidden(string message) => new(message, 403);
    
    /// <summary>
    /// Cria um erro Internal Server Error (500).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <returns>Erro com StatusCode 500</returns>
    public static Error Internal(string message) => new(message, 500);
}

