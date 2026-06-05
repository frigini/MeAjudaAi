using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;

/// <summary>
/// Evento de integração disparado quando um serviço tem seu nome atualizado no catálogo global.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ServiceNameUpdatedIntegrationEvent(
    string Source,
    Guid ServiceId,
    string NewName
) : IntegrationEvent(Source);
