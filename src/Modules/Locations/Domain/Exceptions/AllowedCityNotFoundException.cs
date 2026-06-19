using MeAjudaAi.Shared.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma cidade permitida não é encontrada.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class AllowedCityNotFoundException(Guid cityId) : NotFoundException("Allowed city", cityId)
{
}
