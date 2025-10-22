using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Shared.Models;

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
        Detail = ValidationMessages.Generic.InternalError;
    }
}
