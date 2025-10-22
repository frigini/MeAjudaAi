using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;

/// <summary>
/// Request para buscar usuário por ID entre módulos
/// </summary>
public sealed record GetModuleUserRequest(Guid UserIdToGet) : Request;
