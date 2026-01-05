using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using static MeAjudaAi.Web.Admin.Features.Providers.ProvidersActions;

namespace MeAjudaAi.Web.Admin.Features.Providers;

/// <summary>
/// Effects do Fluxor para a feature de Providers.
/// Lida com side effects (chamadas assíncronas à API).
/// </summary>
public class ProvidersEffects
{
    private readonly IProvidersApi _providersApi;

    public ProvidersEffects(IProvidersApi providersApi)
    {
        _providersApi = providersApi;
    }

    /// <summary>
    /// Effect para carregar providers da API quando LoadProvidersAction é disparada
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _providersApi.GetProvidersAsync(
                action.PageNumber, 
                action.PageSize);

            if (result.IsSuccess && result.Value is not null)
            {
                dispatcher.Dispatch(new LoadProvidersSuccessAction(
                    result.Value.Items,
                    result.Value.TotalItems,
                    result.Value.PageNumber,
                    result.Value.PageSize));
            }
            else
            {
                var errorMessage = result.Error?.Message ?? "Failed to load providers";
                dispatcher.Dispatch(new LoadProvidersFailureAction(errorMessage));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadProvidersFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Effect para recarregar providers quando a página muda
    /// </summary>
    [EffectMethod]
    public async Task HandleNextPageAction(NextPageAction action, IDispatcher dispatcher)
    {
        // O reducer já incrementou a página, agora recarregar os dados
        // Nota: isso será melhorado para usar o estado atual da página
        dispatcher.Dispatch(new LoadProvidersAction());
    }

    /// <summary>
    /// Effect para recarregar providers quando a página muda
    /// </summary>
    [EffectMethod]
    public async Task HandlePreviousPageAction(PreviousPageAction action, IDispatcher dispatcher)
    {
        // O reducer já decrementou a página, agora recarregar os dados
        dispatcher.Dispatch(new LoadProvidersAction());
    }

    /// <summary>
    /// Effect para recarregar providers quando vai para página específica
    /// </summary>
    [EffectMethod]
    public async Task HandleGoToPageAction(GoToPageAction action, IDispatcher dispatcher)
    {
        // O reducer já mudou a página, agora recarregar os dados
        dispatcher.Dispatch(new LoadProvidersAction(action.PageNumber));
    }
}
