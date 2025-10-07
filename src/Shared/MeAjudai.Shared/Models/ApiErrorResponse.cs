namespace MeAjudaAi.Shared.Models;

/// <summary>
/// Modelo padrão para respostas de erro da API.
/// </summary>
/// <remarks>
/// Utilizado para documentação OpenAPI e padronização de respostas de erro.
/// Todos os endpoints que retornam erro devem seguir este formato.
/// </remarks>
public class ApiErrorResponse
{
    /// <summary>
    /// Código de status HTTP do erro.
    /// </summary>
    /// <example>400</example>
    public int StatusCode { get; set; }

    /// <summary>
    /// Título/tipo do erro.
    /// </summary>
    /// <example>Bad Request</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem detalhada do erro.
    /// </summary>
    /// <example>Os dados fornecidos são inválidos.</example>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único para rastreamento do erro.
    /// </summary>
    /// <example>abc123-def456-ghi789</example>
    public string? TraceId { get; set; }

    /// <summary>
    /// Timestamp de quando o erro ocorreu.
    /// </summary>
    /// <example>2024-01-15T14:30:00Z</example>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Detalhes específicos dos erros de validação (quando aplicável).
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}