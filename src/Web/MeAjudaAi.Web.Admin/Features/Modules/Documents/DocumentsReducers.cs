using Fluxor;

namespace MeAjudaAi.Web.Admin.Features.Modules.Documents;

/// <summary>
/// Reducers para estado de documentos.
/// Implementam transformações imutáveis do estado baseadas em actions.
/// </summary>
public static class DocumentsReducers
{
    /// <summary>
    /// Reducer para início de carregamento
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceLoadDocumentsAction(DocumentsState state, DocumentsActions.LoadDocumentsAction action)
        => state with
        {
            IsLoading = true,
            ErrorMessage = null,
            SelectedProviderId = action.ProviderId
        };

    /// <summary>
    /// Reducer para sucesso no carregamento
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceLoadDocumentsSuccessAction(DocumentsState state, DocumentsActions.LoadDocumentsSuccessAction action)
        => state with
        {
            Documents = action.Documents,
            IsLoading = false,
            ErrorMessage = null,
            SelectedProviderId = action.ProviderId
        };

    /// <summary>
    /// Reducer para falha no carregamento
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceLoadDocumentsFailureAction(DocumentsState state, DocumentsActions.LoadDocumentsFailureAction action)
        => state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };

    /// <summary>
    /// Reducer para limpar erro
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceClearErrorAction(DocumentsState state, DocumentsActions.ClearErrorAction _)
        => state with { ErrorMessage = null };

    /// <summary>
    /// Reducer para adicionar documento
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceAddDocumentAction(DocumentsState state, DocumentsActions.AddDocumentAction action)
        => state with
        {
            Documents = state.Documents.Append(action.Document).ToList()
        };

    /// <summary>
    /// Reducer para remover documento
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceRemoveDocumentAction(DocumentsState state, DocumentsActions.RemoveDocumentAction action)
        => state with
        {
            Documents = state.Documents.Where(d => d.Id != action.DocumentId).ToList()
        };

    /// <summary>
    /// Reducer para atualizar status de documento
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceUpdateDocumentStatusAction(DocumentsState state, DocumentsActions.UpdateDocumentStatusAction action)
    {
        var updatedDocs = state.Documents.Select(d =>
            d.Id == action.DocumentId
                ? d with { Status = action.NewStatus }
                : d
        ).ToList();

        return state with { Documents = updatedDocs };
    }

    // Delete Document Reducers
    /// <summary>
    /// Reducer para início de exclusão
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceDeleteDocumentAction(DocumentsState state, DocumentsActions.DeleteDocumentAction action)
        => state with
        {
            IsDeleting = true,
            DeletingDocumentId = action.DocumentId,
            ErrorMessage = null
        };

    /// <summary>
    /// Reducer para sucesso na exclusão
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceDeleteDocumentSuccessAction(DocumentsState state, DocumentsActions.DeleteDocumentSuccessAction action)
        => state with
        {
            IsDeleting = false,
            DeletingDocumentId = null,
            ErrorMessage = null
        };

    /// <summary>
    /// Reducer para falha na exclusão
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceDeleteDocumentFailureAction(DocumentsState state, DocumentsActions.DeleteDocumentFailureAction action)
        => state with
        {
            IsDeleting = false,
            DeletingDocumentId = null,
            ErrorMessage = action.ErrorMessage
        };

    // Request Verification Reducers
    /// <summary>
    /// Reducer para início de solicitação de verificação
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceRequestVerificationAction(DocumentsState state, DocumentsActions.RequestVerificationAction action)
        => state with
        {
            IsRequestingVerification = true,
            VerifyingDocumentId = action.DocumentId,
            ErrorMessage = null
        };

    /// <summary>
    /// Reducer para sucesso na solicitação de verificação
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceRequestVerificationSuccessAction(DocumentsState state, DocumentsActions.RequestVerificationSuccessAction action)
        => state with
        {
            IsRequestingVerification = false,
            VerifyingDocumentId = null,
            ErrorMessage = null
        };

    /// <summary>
    /// Reducer para falha na solicitação de verificação
    /// </summary>
    [ReducerMethod]
    public static DocumentsState ReduceRequestVerificationFailureAction(DocumentsState state, DocumentsActions.RequestVerificationFailureAction action)
        => state with
        {
            IsRequestingVerification = false,
            VerifyingDocumentId = null,
            ErrorMessage = action.ErrorMessage
        };
}
