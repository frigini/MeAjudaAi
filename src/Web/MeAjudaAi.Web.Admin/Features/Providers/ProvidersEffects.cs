using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Authorization;
using MeAjudaAi.Web.Admin.Extensions;
using MeAjudaAi.Web.Admin.Services;
using MudBlazor;
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
    private readonly ISnackbar _snackbar;
    private readonly ILogger<ProvidersEffects> _logger;

    public ProvidersEffects(
        IProvidersApi providersApi,
        IPermissionService permissionService,
        ISnackbar snackbar,
        ILogger<ProvidersEffects> logger)
    {
        _providersApi = providersApi;
        _permissionService = permissionService;
        _snackbar = snackbar;
        _logger = logger;
    }

    /// <summary>
    /// Effect para carregar providers da API quando LoadProvidersAction é disparada
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
    {
        // Verifica permissões antes de fazer a chamada
        var hasPermission = await _permissionService.HasPermissionAsync(PolicyNames.ProviderManagerPolicy);
        if (!hasPermission)
        {
            _logger.LogWarning("User attempted to load providers without proper authorization");
            _snackbar.Add("Acesso negado: você não tem permissão para visualizar provedores", Severity.Error);
            dispatcher.Dispatch(new LoadProvidersFailureAction("Acesso negado"));
            return;
        }

        // Usa a extensão para tratar erros de API automaticamente
        var result = await dispatcher.ExecuteApiCallAsync(
            apiCall: () => _providersApi.GetProvidersAsync(action.PageNumber, action.PageSize),
            snackbar: _snackbar,
            operationName: "Carregar provedores",
            onSuccess: pagedResult =>
            {
                dispatcher.Dispatch(new LoadProvidersSuccessAction(
                    pagedResult.Value.Items,
                    pagedResult.Value.TotalItems,
                    pagedResult.Value.PageNumber,
                    pagedResult.Value.PageSize));
                
                _logger.LogInformation(
                    "Successfully loaded {Count} providers (page {Page}/{TotalPages})",
                    pagedResult.Value.Items.Count,
                    pagedResult.Value.PageNumber,
                    pagedResult.Value.TotalPages);
            },
            onError: ex =>
            {
                _logger.LogError(ex, "Failed to load providers");
                dispatcher.Dispatch(new LoadProvidersFailureAction(ex.Message));
            });

        // Se o resultado foi nulo (erro), dispatch da action de falha já foi feito no onError
        if (result is null)
        {
            dispatcher.Dispatch(new LoadProvidersFailureAction("Falha ao carregar provedores"));
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
