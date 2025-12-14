namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma cidade permitida não é encontrada.
/// </summary>
public sealed class AllowedCityNotFoundException(Guid cityId)
    : NotFoundException($"Cidade permitida com ID '{cityId}' não encontrada")
{
    public Guid CityId { get; } = cityId;
}
