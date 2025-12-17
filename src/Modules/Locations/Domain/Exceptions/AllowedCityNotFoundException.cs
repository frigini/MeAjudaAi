using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma cidade permitida não é encontrada.
/// </summary>
public sealed class AllowedCityNotFoundException : DomainException
{
    public Guid CityId { get; }

    public AllowedCityNotFoundException(Guid cityId)
        : base($"Cidade permitida com ID '{cityId}' não encontrada")
    {
        CityId = cityId;
    }
}
