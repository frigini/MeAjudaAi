using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Documents.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um documento é verificado com sucesso.
/// </summary>
/// <param name="AggregateId">Identificador único do documento</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="ProviderId">Identificador do prestador</param>
/// <param name="DocumentType">Tipo do documento verificado</param>
/// <param name="OcrData">Dados extraídos por OCR (opcional)</param>
public record DocumentVerifiedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    EDocumentType DocumentType,
    string? OcrData
) : DomainEvent(AggregateId, Version);
