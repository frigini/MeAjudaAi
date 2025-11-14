using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Documents.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um documento é verificado com sucesso.
/// NOTA: OcrData foi removido para evitar exposição de PII em logs, eventos externos e persistência.
/// Os dados OCR completos devem ser acessados apenas via queries diretas ao agregado.
/// </summary>
/// <param name="AggregateId">Identificador único do documento</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="ProviderId">Identificador do prestador</param>
/// <param name="DocumentType">Tipo do documento verificado</param>
/// <param name="HasOcrData">Indica se o documento possui dados OCR extraídos</param>
public record DocumentVerifiedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    EDocumentType DocumentType,
    bool HasOcrData
) : DomainEvent(AggregateId, Version);
