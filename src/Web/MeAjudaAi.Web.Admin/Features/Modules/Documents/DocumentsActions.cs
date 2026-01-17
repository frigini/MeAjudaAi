using MeAjudaAi.Contracts.Modules.Documents.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Modules.Documents;

/// <summary>
/// Actions para gerenciamento de estado de documentos
/// </summary>
public static class DocumentsActions
{
    /// <summary>
    /// Carrega documentos de um provider específico
    /// </summary>
    public sealed record LoadDocumentsAction(Guid ProviderId);

    /// <summary>
    /// Sucesso ao carregar documentos
    /// </summary>
    public sealed record LoadDocumentsSuccessAction(IReadOnlyList<ModuleDocumentDto> Documents, Guid ProviderId);

    /// <summary>
    /// Falha ao carregar documentos
    /// </summary>
    public sealed record LoadDocumentsFailureAction(string ErrorMessage);

    /// <summary>
    /// Limpa erro atual
    /// </summary>
    public sealed record ClearErrorAction;

    /// <summary>
    /// Adiciona documento (pós-upload)
    /// </summary>
    public sealed record AddDocumentAction(ModuleDocumentDto Document);

    /// <summary>
    /// Remove documento (pós-delete)
    /// </summary>
    public sealed record RemoveDocumentAction(Guid DocumentId);

    /// <summary>
    /// Atualiza status de documento (pós-verify)
    /// </summary>
    public sealed record UpdateDocumentStatusAction(Guid DocumentId, string NewStatus);

    // Delete Document Actions
    /// <summary>
    /// Solicita exclusão de documento
    /// </summary>
    public sealed record DeleteDocumentAction(Guid ProviderId, Guid DocumentId);

    /// <summary>
    /// Sucesso ao excluir documento
    /// </summary>
    public sealed record DeleteDocumentSuccessAction(Guid DocumentId);

    /// <summary>
    /// Falha ao excluir documento
    /// </summary>
    public sealed record DeleteDocumentFailureAction(Guid DocumentId, string ErrorMessage);

    // Request Verification Actions
    /// <summary>
    /// Solicita verificação de documento
    /// </summary>
    public sealed record RequestVerificationAction(Guid ProviderId, Guid DocumentId);

    /// <summary>
    /// Sucesso ao solicitar verificação
    /// </summary>
    public sealed record RequestVerificationSuccessAction(Guid DocumentId);

    /// <summary>
    /// Falha ao solicitar verificação
    /// </summary>
    public sealed record RequestVerificationFailureAction(Guid DocumentId, string ErrorMessage);
}
