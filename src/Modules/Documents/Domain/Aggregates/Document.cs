using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Documents.Domain.Aggregates;

/// <summary>
/// Aggregate root representando um documento enviado por um provedor
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// ID do provedor que enviou o documento
    /// </summary>
    public Guid ProviderId { get; private set; }
    
    /// <summary>
    /// Tipo do documento
    /// </summary>
    public DocumentType DocumentType { get; private set; }
    
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
    public DocumentStatus Status { get; private set; }
    
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

    // EF Core
    private Document() { }

    private Document(
        Guid id,
        Guid providerId,
        DocumentType documentType,
        string fileName,
        string fileUrl)
    {
        Id = id;
        ProviderId = providerId;
        DocumentType = documentType;
        FileName = fileName;
        FileUrl = fileUrl;
        Status = DocumentStatus.Uploaded;
        UploadedAt = DateTime.UtcNow;
    }

    public static Document Create(
        Guid providerId,
        DocumentType documentType,
        string fileName,
        string fileUrl)
    {
        var document = new Document(
            Guid.NewGuid(),
            providerId,
            documentType,
            fileName,
            fileUrl);

        document.AddDomainEvent(new DocumentUploadedDomainEvent(
            document.Id,
            document.ProviderId,
            document.DocumentType,
            document.FileUrl));

        return document;
    }

    public void MarkAsVerified(string? ocrData = null)
    {
        if (Status == DocumentStatus.Verified)
            return;

        Status = DocumentStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        OcrData = ocrData;

        AddDomainEvent(new DocumentVerifiedDomainEvent(
            Id,
            ProviderId,
            DocumentType,
            ocrData));
    }

    public void MarkAsRejected(string reason)
    {
        if (Status == DocumentStatus.Rejected)
            return;

        Status = DocumentStatus.Rejected;
        VerifiedAt = DateTime.UtcNow;
        RejectionReason = reason;

        AddDomainEvent(new DocumentRejectedDomainEvent(
            Id,
            ProviderId,
            DocumentType,
            reason));
    }

    public void MarkAsFailed(string reason)
    {
        Status = DocumentStatus.Failed;
        RejectionReason = reason;
    }

    public void MarkAsPendingVerification()
    {
        if (Status != DocumentStatus.Uploaded)
            return;

        Status = DocumentStatus.PendingVerification;
    }
}
