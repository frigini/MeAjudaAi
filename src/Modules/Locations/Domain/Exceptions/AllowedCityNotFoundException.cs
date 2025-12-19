using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma cidade permitida não é encontrada.
/// </summary>
public sealed class AllowedCityNotFoundException : NotFoundException
{
    public AllowedCityNotFoundException(Guid cityId)
        : base("Allowed city", cityId)
    {
    }
}
