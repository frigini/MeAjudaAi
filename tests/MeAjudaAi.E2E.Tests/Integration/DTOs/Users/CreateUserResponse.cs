namespace MeAjudaAi.E2E.Tests.Integration.DTOs.Users;

/// <summary>
/// Modelo de resposta para criação de usuário em testes E2E.
/// </summary>
public record CreateUserResponse
{
    /// <summary>
    /// Obtém ou inicializa o ID do usuário criado.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Obtém ou inicializa a mensagem de resposta.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
