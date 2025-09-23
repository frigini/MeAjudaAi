namespace MeAjudaAi.Shared.Models;

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