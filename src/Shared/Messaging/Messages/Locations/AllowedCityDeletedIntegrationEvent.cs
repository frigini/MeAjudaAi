using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Locations;

/// <summary>
/// Evento de integração disparado quando uma cidade é removida das permitidas.
/// </summary>
[ExcludeFromCodeCoverage]
public record AllowedCityDeletedIntegrationEvent(
    string Source,
    Guid CityId,
    string CityName,
    string StateSigla
) : IntegrationEvent(Source);
