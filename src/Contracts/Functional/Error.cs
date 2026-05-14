namespace MeAjudaAi.Contracts.Functional;

/// <summary>
/// Representa um erro com mensagem e código de status HTTP.
/// </summary>
/// <param name="Message">Mensagem descritiva do erro</param>
/// <param name="StatusCode">Código de status HTTP (padrão: 400)</param>
/// <param name="Code">Código estável do erro para identificação programática</param>
public record Error(string Message, int StatusCode = 400, string? Code = null)
{
    /// <summary>
    /// Cria um erro Not Found (404).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 404</returns>
    public static Error NotFound(string message, string? code = null) => new(message, 404, code);
    
    /// <summary>
    /// Cria um erro Bad Request (400).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 400</returns>
    public static Error BadRequest(string message, string? code = null) => new(message, 400, code);
    
    /// <summary>
    /// Cria um erro Unauthorized (401).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 401</returns>
    public static Error Unauthorized(string message, string? code = null) => new(message, 401, code);
    
    /// <summary>
    /// Cria um erro Forbidden (403).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 403</returns>
    public static Error Forbidden(string message, string? code = null) => new(message, 403, code);
    
    /// <summary>
    /// Cria um erro Internal Server Error (500).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 500</returns>
    public static Error Internal(string message, string? code = null) => new(message, 500, code);

    /// <summary>
    /// Cria um erro Conflict (409).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 409</returns>
    public static Error Conflict(string message, string? code = null) => new(message, 409, code);

    /// <summary>
    /// Cria um erro Unprocessable Entity (422).
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="code">Código opcional do erro</param>
    /// <returns>Erro com StatusCode 422</returns>
    public static Error Unprocessable(string message, string? code = null) => new(message, 422, code);
}

