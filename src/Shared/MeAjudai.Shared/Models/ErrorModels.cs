using System.ComponentModel.DataAnnotations;

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

/// <summary>
/// Modelo específico para erros de validação.
/// </summary>
/// <remarks>
/// Usado quando a validação de entrada falha, fornecendo detalhes
/// específicos sobre quais campos têm problemas.
/// </remarks>
public class ValidationErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância de ValidationErrorResponse.
    /// </summary>
    public ValidationErrorResponse()
    {
        StatusCode = 400;
        Title = "Validation Error";
        Detail = "Um ou mais campos de entrada contêm dados inválidos.";
    }

    /// <summary>
    /// Inicializa uma nova instância com erros de validação específicos.
    /// </summary>
    /// <param name="validationErrors">Dicionário de erros por campo</param>
    public ValidationErrorResponse(Dictionary<string, string[]> validationErrors) : this()
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Modelo para erros de autenticação/autorização.
/// </summary>
public class AuthenticationErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância para erro de autenticação.
    /// </summary>
    public AuthenticationErrorResponse()
    {
        StatusCode = 401;
        Title = "Unauthorized";
        Detail = "Token de autenticação ausente, inválido ou expirado.";
    }
}

/// <summary>
/// Modelo para erros de permissão/autorização.
/// </summary>
public class AuthorizationErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância para erro de autorização.
    /// </summary>
    public AuthorizationErrorResponse()
    {
        StatusCode = 403;
        Title = "Forbidden";
        Detail = "Você não possui permissão para acessar este recurso.";
    }
}

/// <summary>
/// Modelo para erros de recurso não encontrado.
/// </summary>
public class NotFoundErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância para erro de recurso não encontrado.
    /// </summary>
    public NotFoundErrorResponse()
    {
        StatusCode = 404;
        Title = "Not Found";
        Detail = "O recurso solicitado não foi encontrado.";
    }

    /// <summary>
    /// Inicializa uma nova instância com recurso específico.
    /// </summary>
    /// <param name="resourceType">Tipo do recurso não encontrado</param>
    /// <param name="resourceId">ID do recurso não encontrado</param>
    public NotFoundErrorResponse(string resourceType, string resourceId) : this()
    {
        Detail = $"{resourceType} com ID '{resourceId}' não foi encontrado.";
    }
}

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

/// <summary>
/// Modelo para erros internos do servidor.
/// </summary>
public class InternalServerErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância para erro interno.
    /// </summary>
    public InternalServerErrorResponse()
    {
        StatusCode = 500;
        Title = "Internal Server Error";
        Detail = "Ocorreu um erro interno no servidor. Tente novamente mais tarde.";
    }
}