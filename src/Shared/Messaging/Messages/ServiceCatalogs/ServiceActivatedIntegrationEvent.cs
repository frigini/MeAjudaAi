using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;

/// <summary>
/// Evento de integração disparado quando um serviço é ativado no catálogo global.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ServiceActivatedIntegrationEvent(
    string Source,
    Guid ServiceId,
    string Name
) : IntegrationEvent(Source);
