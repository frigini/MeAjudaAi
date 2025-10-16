using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Shared.Models;

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
        Detail = ValidationMessages.Generic.InvalidData;
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