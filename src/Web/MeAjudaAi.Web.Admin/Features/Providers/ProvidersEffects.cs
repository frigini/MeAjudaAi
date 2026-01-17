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
    private readonly ErrorHandlingService _errorHandler;

    public ProvidersEffects(
        IProvidersApi providersApi,
        IPermissionService permissionService,
        ISnackbar snackbar,
        ILogger<ProvidersEffects> logger,
        ErrorHandlingService errorHandler)
    {
        _providersApi = providersApi;
        _permissionService = permissionService;
        _snackbar = snackbar;
        _logger = logger;
        _errorHandler = errorHandler;
    }

    /// <summary>
    /// Effect para carregar providers da API quando LoadProvidersAction é disparada
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
    {
        using var cts = new CancellationTokenSource();
        
        // Verifica permissões antes de fazer a chamada
        var hasPermission = await _permissionService.HasPermissionAsync(PolicyNames.ProviderManagerPolicy);
        if (!hasPermission)
        {
            var errorMessage = _errorHandler.GetUserFriendlyMessage(403, "Você não tem permissão para acessar provedores");
            _logger.LogWarning("User attempted to load providers without proper authorization");
            _snackbar.Add(errorMessage, Severity.Error);
            dispatcher.Dispatch(new LoadProvidersFailureAction(errorMessage));
            return;
        }

        // Use retry logic for transient failures (GET is safe to retry)
        // Polly handles retry at HttpClient level (3 attempts with exponential backoff)
        var result = await _errorHandler.ExecuteWithErrorHandlingAsync(
            ct => _providersApi.GetProvidersAsync(action.PageNumber, action.PageSize),
            "carregar provedores",
            cts.Token);

        if (result.IsSuccess)
        {
            dispatcher.Dispatch(new LoadProvidersSuccessAction(
                result.Value.Items,
                result.Value.TotalItems,
                result.Value.PageNumber,
                result.Value.PageSize));
            
            _logger.LogInformation(
                "Successfully loaded {Count} providers (page {Page}/{TotalPages})",
                result.Value.Items.Count,
                result.Value.PageNumber,
                result.Value.TotalPages);
        }
        else
        {
            var errorMessage = _errorHandler.HandleApiError(result, "carregar provedores");
            _snackbar.Add(errorMessage, Severity.Error);
            dispatcher.Dispatch(new LoadProvidersFailureAction(errorMessage));
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

    /// <summary>
    /// Effect para deletar provider
    /// </summary>
    [EffectMethod]
    public async Task HandleDeleteProviderAction(DeleteProviderAction action, IDispatcher dispatcher)
    {
        // Usa a extensão para tratar erros de API automaticamente
        var result = await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _providersApi.DeleteProviderAsync(action.ProviderId),
            operationName: "Deletar provedor",
            onSuccess: _ =>
            {
                _logger.LogInformation(
                    "Provider {ProviderId} deleted successfully",
                    action.ProviderId);
                
                dispatcher.Dispatch(new DeleteProviderSuccessAction(action.ProviderId));
                _snackbar.Add("Provedor excluído com sucesso!", Severity.Success);
                
                // Recarregar lista após delete
                dispatcher.Dispatch(new LoadProvidersAction());
            },
            onError: ex =>
            {
                _logger.LogError(ex, "Failed to delete provider {ProviderId}", action.ProviderId);
                dispatcher.Dispatch(new DeleteProviderFailureAction(
                    action.ProviderId,
                    ex.Message));
            });

        if (result is null)
        {
            dispatcher.Dispatch(new DeleteProviderFailureAction(
                action.ProviderId,
                "Falha ao deletar provedor"));
        }
    }
}
