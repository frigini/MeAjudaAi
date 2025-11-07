namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para endereço.
/// </summary>
public sealed record AddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country
);
