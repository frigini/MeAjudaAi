using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Documents.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands e Queries do módulo Documents.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia UploadDocumentRequest (Application) para UploadDocumentCommand.
    /// </summary>
    public static UploadDocumentCommand ToCommand(this UploadDocumentRequest request)
    {
        return new UploadDocumentCommand(
            request.ProviderId,
            request.DocumentType.ToString(),
            request.FileName,
            request.ContentType,
            request.FileSizeBytes);
    }

    /// <summary>
    /// Mapeia UploadDocumentRequest (Contract) para UploadDocumentCommand.
    /// </summary>
    public static UploadDocumentCommand ToCommand(this Contracts.Modules.Documents.DTOs.UploadDocumentRequest request)
    {
        return new UploadDocumentCommand(
            request.ProviderId,
            request.DocumentType,
            request.FileName,
            request.ContentType,
            request.FileSizeBytes);
    }

    /// <summary>
    /// Mapeia VerifyDocumentRequest para ApproveDocumentCommand quando IsVerified é true.
    /// </summary>
    public static ApproveDocumentCommand ToApproveCommand(this VerifyDocumentRequest request, Guid documentId)
    {
        return new ApproveDocumentCommand(documentId, request.VerificationNotes);
    }

    /// <summary>
    /// Mapeia VerifyDocumentRequest (Contract) para ApproveDocumentCommand.
    /// </summary>
    public static ApproveDocumentCommand ToApproveCommand(this Contracts.Modules.Documents.DTOs.VerifyDocumentRequest request, Guid documentId)
    {
        return new ApproveDocumentCommand(documentId, request.VerificationNotes);
    }

    /// <summary>
    /// Mapeia VerifyDocumentRequest para RejectDocumentCommand quando IsVerified é false.
    /// </summary>
    public static RejectDocumentCommand ToRejectCommand(this VerifyDocumentRequest request, Guid documentId)
    {
        var reason = request.VerificationNotes ?? "Documento rejeitado durante verificação";
        return new RejectDocumentCommand(documentId, reason);
    }

    /// <summary>
    /// Mapeia VerifyDocumentRequest (Contract) para RejectDocumentCommand.
    /// </summary>
    public static RejectDocumentCommand ToRejectCommand(this Contracts.Modules.Documents.DTOs.VerifyDocumentRequest request, Guid documentId)
    {
        var reason = request.VerificationNotes ?? "Documento rejeitado durante verificação";
        return new RejectDocumentCommand(documentId, reason);
    }

    /// <summary>
    /// Mapeia um Guid para RequestVerificationCommand.
    /// </summary>
    public static RequestVerificationCommand ToRequestVerificationCommand(this Guid documentId)
    {
        return new RequestVerificationCommand(documentId);
    }

    /// <summary>
    /// Mapeia um Guid para DeleteDocumentCommand.
    /// </summary>
    public static DeleteDocumentCommand ToDeleteCommand(this Guid documentId)
    {
        return new DeleteDocumentCommand(documentId);
    }

    /// <summary>
    /// Mapeia um Guid para GetDocumentStatusQuery.
    /// </summary>
    public static Application.Queries.GetDocumentStatusQuery ToQuery(this Guid documentId)
    {
        return new Application.Queries.GetDocumentStatusQuery(documentId);
    }

    /// <summary>
    /// Mapeia um Guid para GetProviderDocumentsQuery.
    /// </summary>
    public static Application.Queries.GetProviderDocumentsQuery ToDocumentsQuery(this Guid providerId)
    {
        return new Application.Queries.GetProviderDocumentsQuery(providerId);
    }
}
