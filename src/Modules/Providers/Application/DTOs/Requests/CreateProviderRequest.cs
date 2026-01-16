using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para criação de um novo prestador de serviços.
/// </summary>
public record CreateProviderRequest : Request
{
    /// <summary>
    /// Nome do prestador.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do prestador (Individual ou Company).
    /// </summary>
    public EProviderType Type { get; init; }

    /// <summary>
    /// Perfil de negócio do prestador.
    /// </summary>
    public BusinessProfileDto? BusinessProfile { get; init; }
}
