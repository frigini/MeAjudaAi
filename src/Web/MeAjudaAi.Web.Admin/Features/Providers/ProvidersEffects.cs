using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Authorization;
using MeAjudaAi.Web.Admin.Services;
using static MeAjudaAi.Web.Admin.Features.Providers.ProvidersActions;

namespace MeAjudaAi.Web.Admin.Features.Providers;

/// <summary>
/// Effects do Fluxor para a feature de Providers.
/// Lida com side effects (chamadas assíncronas à API).
/// </summary>
public class ProvidersEffects
{
    private readonly IProvidersApi _providersApi;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ProvidersEffects> _logger;

    public ProvidersEffects(
        IProvidersApi providersApi,
        IPermissionService permissionService,
        ILogger<ProvidersEffects> logger)
    {
        _providersApi = providersApi;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Effect para carregar providers da API quando LoadProvidersAction é disparada
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
    {
        try
        {
            // Verify user has permission to view providers
            var hasPermission = await _permissionService.HasPermissionAsync(PolicyNames.ProviderManagerPolicy);
            if (!hasPermission)
            {
                _logger.LogWarning("User attempted to load providers without proper authorization");
                dispatcher.Dispatch(new LoadProvidersFailureAction("Acesso negado: você não tem permissão para visualizar provedores"));
                return;
            }

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
                var errorMessage = result.Error?.Message ?? "Falha ao carregar fornecedores";
                dispatcher.Dispatch(new LoadProvidersFailureAction(errorMessage));
            }
        }
        catch (Exception ex)
        {
            var userFriendlyMessage = $"Erro ao carregar fornecedores: {ex.Message}";
            dispatcher.Dispatch(new LoadProvidersFailureAction(userFriendlyMessage));
        }
    }

    /// <summary>
    /// Effect para recarregar providers quando a página muda
    /// </summary>
    [EffectMethod]
    public Task HandleNextPageAction(NextPageAction action, IDispatcher dispatcher)
    {
        // O reducer já incrementou a página, agora recarregar os dados
        // Nota: isso será melhorado para usar o estado atual da página
        dispatcher.Dispatch(new LoadProvidersAction());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Effect para recarregar providers quando a página muda
    /// </summary>
    [EffectMethod]
    public Task HandlePreviousPageAction(PreviousPageAction action, IDispatcher dispatcher)
    {
        // O reducer já decrementou a página, agora recarregar os dados
        dispatcher.Dispatch(new LoadProvidersAction());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Effect para recarregar providers quando vai para página específica
    /// </summary>
    [EffectMethod]
    public Task HandleGoToPageAction(GoToPageAction action, IDispatcher dispatcher)
    {
        // O reducer já mudou a página, agora recarregar os dados
        dispatcher.Dispatch(new LoadProvidersAction(action.PageNumber));
        return Task.CompletedTask;
    }
}
