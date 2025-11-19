using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Documents.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando falha o processamento técnico de um documento.
/// Diferente de DocumentRejectedDomainEvent (rejeição de negócio), este evento
/// indica falha técnica (OCR timeout, serviço indisponível, erro de rede, etc.).
/// </summary>
/// <param name="AggregateId">Identificador único do documento</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="ProviderId">Identificador do prestador</param>
/// <param name="DocumentType">Tipo do documento que falhou</param>
/// <param name="FailureReason">Motivo da falha técnica</param>
public record DocumentFailedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    EDocumentType DocumentType,
    string FailureReason
) : DomainEvent(AggregateId, Version);
