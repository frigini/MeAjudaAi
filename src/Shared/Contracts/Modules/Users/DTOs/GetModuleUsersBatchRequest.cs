using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;

/// <summary>
/// Request para buscar múltiplos usuários por IDs
/// </summary>
public sealed record GetModuleUsersBatchRequest(IReadOnlyList<Guid> UserIds) : Request;
