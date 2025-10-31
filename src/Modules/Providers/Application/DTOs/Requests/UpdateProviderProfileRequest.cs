using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para atualização do perfil de um prestador de serviços.
/// </summary>
public record UpdateProviderProfileRequest : Request
{
    /// <summary>
    /// Nome do prestador.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Perfil de negócio atualizado do prestador.
    /// </summary>
    public BusinessProfileDto BusinessProfile { get; init; } = new(
        string.Empty, 
        null, 
        null, 
        new ContactInfoDto(string.Empty, null, null), 
        new AddressDto(string.Empty, string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
    );
}