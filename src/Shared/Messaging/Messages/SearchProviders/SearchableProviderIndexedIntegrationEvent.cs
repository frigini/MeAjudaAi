using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.SearchProviders;

/// <summary>
/// Evento de integração disparado quando um prestador é indexado para busca.
/// </summary>
[ExcludeFromCodeCoverage]
public record SearchableProviderIndexedIntegrationEvent(
    string Source,
    Guid ProviderId,
    string Name,
    string Description,
    List<string> Services
) : IntegrationEvent(Source);
