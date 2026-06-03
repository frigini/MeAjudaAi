namespace MeAjudaAi.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO simplificado de usuário para comunicação entre módulos
/// </summary>
public sealed record ModuleUserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string FullName
);

