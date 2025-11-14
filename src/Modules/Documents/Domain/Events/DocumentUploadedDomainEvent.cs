using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Documents.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um documento é enviado para verificação.
/// </summary>
/// <param name="AggregateId">Identificador único do documento</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="ProviderId">Identificador do prestador que enviou o documento</param>
/// <param name="DocumentType">Tipo do documento enviado</param>
/// <param name="FileUrl">URL do arquivo armazenado</param>
public record DocumentUploadedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    EDocumentType DocumentType,
    string FileUrl
) : DomainEvent(AggregateId, Version);
