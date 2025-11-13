using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Documents.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um documento é rejeitado na verificação.
/// </summary>
/// <param name="AggregateId">Identificador único do documento</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="ProviderId">Identificador do prestador</param>
/// <param name="DocumentType">Tipo do documento rejeitado</param>
/// <param name="RejectionReason">Motivo da rejeição</param>
public record DocumentRejectedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    EDocumentType DocumentType,
    string RejectionReason
) : DomainEvent(AggregateId, Version);
