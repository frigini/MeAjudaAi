using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para perfil empresarial.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record BusinessProfileDto(
    string LegalName,
    string? FantasyName,
    string? Description,
    ContactInfoDto ContactInfo,
    AddressDto? PrimaryAddress,
    bool ShowAddressToClient = false
);
