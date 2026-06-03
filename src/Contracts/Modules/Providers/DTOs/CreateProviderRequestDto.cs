namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// Request DTO para criação de provider.
/// Usado pelo Admin Portal para adicionar novos providers ao sistema.
/// </summary>
public sealed record CreateProviderRequestDto(
    Guid UserId,
    string Name,
    int Type,
    BusinessProfileDto BusinessProfile,
    string? Document = null);

/// <summary>
/// DTO para perfil de negócio do provider.
/// </summary>
public sealed record BusinessProfileDto(
    string LegalName,
    string? FantasyName,
    string? Description,
    ContactInfoDto ContactInfo,
    PrimaryAddressDto PrimaryAddress);

/// <summary>
/// DTO para informações de contato.
/// </summary>
public sealed record ContactInfoDto(
    string Email,
    string? PhoneNumber,
    string? Website);

/// <summary>
/// DTO para endereço primário.
/// </summary>
public sealed record PrimaryAddressDto(
    string Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country);
