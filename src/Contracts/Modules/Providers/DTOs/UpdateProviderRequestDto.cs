namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// Request DTO para atualização de provider.
/// Usado pelo Admin Portal para modificar dados de providers existentes.
/// </summary>
public sealed record UpdateProviderRequestDto(
    string? Name = null,
    string? Phone = null,
    BusinessProfileUpdateDto? BusinessProfile = null);

/// <summary>
/// DTO para atualização de perfil de negócio.
/// </summary>
public sealed record BusinessProfileUpdateDto(
    string? LegalName = null,
    string? FantasyName = null,
    string? Description = null,
    ContactInfoUpdateDto? ContactInfo = null,
    PrimaryAddressUpdateDto? PrimaryAddress = null);

/// <summary>
/// DTO para atualização de informações de contato.
/// </summary>
public sealed record ContactInfoUpdateDto(
    string? Email = null,
    string? PhoneNumber = null,
    string? Website = null);

/// <summary>
/// DTO para atualização de endereço.
/// </summary>
public sealed record PrimaryAddressUpdateDto(
    string? Street = null,
    string? Number = null,
    string? Complement = null,
    string? Neighborhood = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null,
    string? Country = null);
