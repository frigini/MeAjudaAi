namespace MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;

/// <summary>
/// Request para verificar se usuário existe
/// </summary>
public sealed record CheckUserExistsRequest(Guid UserId);