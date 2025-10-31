namespace MeAjudaAi.Shared.Models;

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
