namespace MeAjudaAi.Shared.Exceptions;

/// <summary>
/// Exceção lançada quando uma requisição está bem formada mas não pode ser processada
/// devido a erros de validação semântica/regras de negócio.
/// </summary>
/// <remarks>
/// HTTP 422 Unprocessable Entity - A requisição estava bem formada mas não pôde ser processada
/// devido a erros semânticos (ex: categoria não existe, transição de estado inválida).
/// 
/// Use esta exceção para validações de regras de negócio que ocorrem APÓS a validação
/// de formato/estrutura básica da requisição.
/// 
/// Diferença de ValidationException (400 Bad Request):
/// - 400: Erros de formato/estrutura (campo obrigatório faltando, tipo errado, JSON inválido)
/// - 422: Erros semânticos/regras de negócio (categoria não existe, transição inválida)
/// </remarks>
public class UnprocessableEntityException : Exception
{
    /// <summary>
    /// Nome da entidade relacionada ao erro (opcional).
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Detalhes adicionais sobre o erro (opcional).
    /// </summary>
    public Dictionary<string, object?>? Details { get; }

    public UnprocessableEntityException(string message) : base(message)
    {
    }

    public UnprocessableEntityException(string message, string entityName) : base(message)
    {
        EntityName = entityName;
    }

    public UnprocessableEntityException(string message, string entityName, Dictionary<string, object?> details)
        : base(message)
    {
        EntityName = entityName;
        Details = details;
    }

    public UnprocessableEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
