using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Documents.Domain.Entities;

/// <summary>
/// Aggregate root representando um documento enviado por um provedor
/// </summary>
public sealed class Document : AggregateRoot<DocumentId>
{
    /// <summary>
    /// ID do provedor que enviou o documento
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// Tipo do documento
    /// </summary>
    public EDocumentType DocumentType { get; private set; }

    /// <summary>
    /// URL do arquivo no blob storage
    /// </summary>
    public string FileUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Nome original do arquivo
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// Status atual do documento
    /// </summary>
    public EDocumentStatus Status { get; private set; }

    /// <summary>
    /// Data de upload do documento
    /// </summary>
    public DateTime UploadedAt { get; private set; }

    /// <summary>
    /// Data da última verificação (se houver)
    /// </summary>
    public DateTime? VerifiedAt { get; private set; }

    /// <summary>
    /// Motivo de rejeição (se Status == Rejected)
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Dados extraídos por OCR (JSON serializado)
    /// </summary>
    public string? OcrData { get; private set; }

    /// <summary>
    /// Construtor privado para uso do Entity Framework.
    /// </summary>
    private Document() { }

    /// <summary>
    /// Cria um novo documento no sistema.
    /// </summary>
    /// <param name="providerId">ID do provedor que enviou o documento</param>
    /// <param name="documentType">Tipo do documento</param>
    /// <param name="fileName">Nome original do arquivo</param>
    /// <param name="fileUrl">URL do arquivo no blob storage</param>
    /// <remarks>
    /// Este construtor dispara automaticamente o evento DocumentUploadedDomainEvent.
    /// </remarks>
    private Document(
        Guid providerId,
        EDocumentType documentType,
        string fileName,
        string fileUrl)
        : base(DocumentId.New())
    {
        ProviderId = providerId;
        DocumentType = documentType;
        FileName = fileName;
        FileUrl = fileUrl;
        Status = EDocumentStatus.Uploaded;
        UploadedAt = DateTime.UtcNow;

        AddDomainEvent(new DocumentUploadedDomainEvent(
            Id,
            1,
            ProviderId,
            DocumentType,
            FileUrl));
    }

    /// <summary>
    /// Factory method para criar um novo documento.
    /// </summary>
    public static Document Create(
        Guid providerId,
        EDocumentType documentType,
        string fileName,
        string fileUrl)
    {
        return new Document(providerId, documentType, fileName, fileUrl);
    }

    public void MarkAsVerified(string? ocrData = null)
    {
        // Invariante: Só pode verificar documentos em estado PendingVerification
        if (Status != EDocumentStatus.PendingVerification)
        {
            throw new InvalidOperationException(
                $"Cannot verify document in status {Status}. Document must be in PendingVerification status.");
        }

        Status = EDocumentStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        OcrData = ocrData;
        MarkAsUpdated();

        AddDomainEvent(new DocumentVerifiedDomainEvent(
            Id,
            1,
            ProviderId,
            DocumentType,
            !string.IsNullOrEmpty(ocrData)));
    }

    public void MarkAsRejected(string reason)
    {
        // Invariante: Motivo de rejeição não pode ser vazio
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Rejection reason cannot be empty", nameof(reason));
        }

        // Invariante: Só pode rejeitar documentos em estado PendingVerification
        if (Status != EDocumentStatus.PendingVerification)
        {
            throw new InvalidOperationException(
                $"Cannot reject document in status {Status}. Document must be in PendingVerification status.");
        }

        Status = EDocumentStatus.Rejected;
        VerifiedAt = DateTime.UtcNow;
        RejectionReason = reason;
        MarkAsUpdated();

        AddDomainEvent(new DocumentRejectedDomainEvent(
            Id,
            1,
            ProviderId,
            DocumentType,
            reason));
    }

    public void MarkAsFailed(string reason)
    {
        // Invariante: Motivo de falha não pode ser vazio
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));
        }

        Status = EDocumentStatus.Failed;
        RejectionReason = reason;
        MarkAsUpdated();

        // Emitir evento de domínio para observabilidade de falhas técnicas
        AddDomainEvent(new DocumentFailedDomainEvent(
            Id,
            1,
            ProviderId,
            DocumentType,
            reason));
    }

    public void MarkAsPendingVerification()
    {
        // Allow transition from Uploaded or Failed (for retries)
        if (Status != EDocumentStatus.Uploaded && Status != EDocumentStatus.Failed)
            return;

        Status = EDocumentStatus.PendingVerification;

        // Clear previous failure reason when retrying
        if (RejectionReason != null)
        {
            RejectionReason = null;
        }

        MarkAsUpdated();
    }
}
