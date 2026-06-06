using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Locations;

/// <summary>
/// Evento de integração disparado quando uma cidade permitida é atualizada.
/// </summary>
[ExcludeFromCodeCoverage]
public record AllowedCityUpdatedIntegrationEvent(
    string Source,
    Guid CityId,
    string CityName,
    string StateSigla
) : IntegrationEvent(Source);
