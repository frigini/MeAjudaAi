using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Documents;

/// <summary>
/// Evento de integração disparado quando um documento é rejeitado.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DocumentRejectedIntegrationEvent(
    string Source,
    Guid DocumentId,
    Guid ProviderId,
    string DocumentType,
    string Reason,
    DateTime RejectedAt
) : IntegrationEvent(Source);
