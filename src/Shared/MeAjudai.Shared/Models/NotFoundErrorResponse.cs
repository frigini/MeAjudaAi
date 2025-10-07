namespace MeAjudaAi.Shared.Models;

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