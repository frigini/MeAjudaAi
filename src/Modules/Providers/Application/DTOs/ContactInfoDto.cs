namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para informações de contato.
/// </summary>
public sealed record ContactInfoDto(
    string Email,
    string? PhoneNumber,
    string? Website
);
