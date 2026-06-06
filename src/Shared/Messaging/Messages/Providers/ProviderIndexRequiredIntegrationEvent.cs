using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando um prestador precisa ser indexado ou reindexado no módulo de busca.
/// </summary>
[ExcludeFromCodeCoverage]
public record ProviderIndexRequiredIntegrationEvent(
    string Source,
    Guid ProviderId
) : IntegrationEvent(Source);
