using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Modelo específico para erros de validação.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidationErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Inicializa uma nova instância de ValidationErrorResponse.
    /// </summary>
    public ValidationErrorResponse()
    {
        StatusCode = 400;
        Title = "Validation Error";
        Detail = ValidationMessages.Generic.InvalidData;
        ValidationErrors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Inicializa uma nova instância com erros de validação específicos.
    /// </summary>
    /// <param name="validationErrors">Dicionário de erros por campo</param>
    public ValidationErrorResponse(Dictionary<string, string[]>? validationErrors) : this()
    {
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }
}
