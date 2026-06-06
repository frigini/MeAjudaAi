using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;

/// <summary>
/// Evento de integração disparado quando um serviço é desativado no catálogo global.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ServiceDeactivatedIntegrationEvent(
    string Source,
    Guid ServiceId
) : IntegrationEvent(Source);
