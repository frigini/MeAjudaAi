using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Extensions;
using MudBlazor;

namespace MeAjudaAi.Web.Admin.Features.Documents;

/// <summary>
/// Effects para operações assíncronas de documentos.
/// Faz chamadas à API e dispatcha actions de sucesso/falha.
/// </summary>
public sealed class DocumentsEffects
{
    private readonly IDocumentsApi _documentsApi;
    private readonly ISnackbar _snackbar;

    public DocumentsEffects(IDocumentsApi documentsApi, ISnackbar snackbar)
    {
        _documentsApi = documentsApi;
        _snackbar = snackbar;
    }

    /// <summary>
    /// Effect para carregar documentos de um provider
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadDocumentsAction(DocumentsActions.LoadDocumentsAction action, IDispatcher dispatcher)
    {
        var result = await _documentsApi.GetDocumentsByProviderAsync(action.ProviderId);

        if (result.IsSuccess && result.Value != null)
        {
            dispatcher.Dispatch(new DocumentsActions.LoadDocumentsSuccessAction(result.Value, action.ProviderId));
        }
        else
        {
            var errorMessage = result.Error?.Message ?? "Erro ao carregar documentos";
            dispatcher.Dispatch(new DocumentsActions.LoadDocumentsFailureAction(errorMessage));
        }
    }

    /// <summary>
    /// Effect para excluir documento
    /// </summary>
    [EffectMethod]
    public async Task HandleDeleteDocumentAction(DocumentsActions.DeleteDocumentAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _documentsApi.DeleteDocumentAsync(action.ProviderId, action.DocumentId),
            operationName: "Excluir documento",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new DocumentsActions.DeleteDocumentSuccessAction(action.DocumentId));
                _snackbar.Add("Documento excluído com sucesso!", Severity.Success);
                dispatcher.Dispatch(new DocumentsActions.RemoveDocumentAction(action.DocumentId));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new DocumentsActions.DeleteDocumentFailureAction(action.DocumentId, ex.Message));
            });
    }

    /// <summary>
    /// Effect para solicitar verificação de documento
    /// </summary>
    [EffectMethod]
    public async Task HandleRequestVerificationAction(DocumentsActions.RequestVerificationAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _documentsApi.RequestDocumentVerificationAsync(action.ProviderId, action.DocumentId),
            operationName: "Solicitar verificação",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new DocumentsActions.RequestVerificationSuccessAction(action.DocumentId));
                _snackbar.Add("Verificação solicitada com sucesso!", Severity.Success);
                dispatcher.Dispatch(new DocumentsActions.UpdateDocumentStatusAction(action.DocumentId, Constants.DocumentStatus.ToDisplayName(Constants.DocumentStatus.PendingVerification)));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new DocumentsActions.RequestVerificationFailureAction(action.DocumentId, ex.Message));
            });
    }
}
