using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Locations;

/// <summary>
/// Evento de integração disparado quando uma cidade é permitida para operação.
/// </summary>
[ExcludeFromCodeCoverage]
public record AllowedCityCreatedIntegrationEvent(
    string Source,
    Guid CityId,
    string CityName,
    string StateSigla
) : IntegrationEvent(Source);
