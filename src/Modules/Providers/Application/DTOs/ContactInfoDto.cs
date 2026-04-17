using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para informações de contato.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ContactInfoDto(
    string Email,
    string? PhoneNumber,
    string? Website,
    IEnumerable<string>? AdditionalPhones = null
);
