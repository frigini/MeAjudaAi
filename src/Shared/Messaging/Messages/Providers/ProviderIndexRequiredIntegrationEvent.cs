using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando um prestador precisa ser indexado.
/// </summary>
[ExcludeFromCodeCoverage]
[HighVolumeEvent(10)]
public record ProviderIndexRequiredIntegrationEvent(
    string Source,
    Guid ProviderId
) : IntegrationEvent(Source);
