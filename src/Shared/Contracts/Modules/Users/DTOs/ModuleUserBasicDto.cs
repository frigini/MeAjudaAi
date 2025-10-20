namespace MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO básico de usuário para validações rápidas entre módulos
/// </summary>
public sealed record ModuleUserBasicDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive
);
