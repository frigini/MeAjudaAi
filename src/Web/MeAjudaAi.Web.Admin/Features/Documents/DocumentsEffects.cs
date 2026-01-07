using Fluxor;
using MeAjudaAi.Client.Contracts.Api;

namespace MeAjudaAi.Web.Admin.Features.Documents;

/// <summary>
/// Effects para operações assíncronas de documentos.
/// Faz chamadas à API e dispatcha actions de sucesso/falha.
/// </summary>
public sealed class DocumentsEffects
{
    private readonly IDocumentsApi _documentsApi;

    public DocumentsEffects(IDocumentsApi documentsApi)
    {
        _documentsApi = documentsApi;
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
}
