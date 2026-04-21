using MeAjudaAi.Shared.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma cidade permitida não é encontrada.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class AllowedCityNotFoundException : NotFoundException
{
    public AllowedCityNotFoundException(Guid cityId)
        : base("Allowed city", cityId)
    {
    }
}
