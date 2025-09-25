namespace MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;

/// <summary>
/// Request para buscar usuário por email entre módulos
/// </summary>
public sealed record GetModuleUserByEmailRequest(string Email);