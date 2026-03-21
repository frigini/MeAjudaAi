using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using Microsoft.Extensions.Logging;
using static MeAjudaAi.Web.Admin.Features.Dashboard.DashboardActions;

namespace MeAjudaAi.Web.Admin.Features.Dashboard;

/// <summary>
/// Effects do Fluxor para a feature de Dashboard.
/// Lida com side effects (chamadas assíncronas à API).
/// </summary>
public class DashboardEffects
{
    /// <summary>
    /// Status de verificação "Pending" deve corresponder ao valor de 
    /// MeAjudaAi.Modules.Providers.Domain.Enums.EVerificationStatus.Pending.
    /// IMPORTANTE: Se o enum do backend mudar, este valor deve ser atualizado.
    /// </summary>
    private const string PENDING_STATUS = "Pending";
    
    private readonly IProvidersApi _providersApi;
    private readonly IServiceCatalogsApi _serviceCatalogsApi;
    private readonly ILogger<DashboardEffects> _logger;

    public DashboardEffects(
        IProvidersApi providersApi, 
        IServiceCatalogsApi serviceCatalogsApi,
        ILogger<DashboardEffects> logger)
    {
        _providersApi = providersApi;
        _serviceCatalogsApi = serviceCatalogsApi;
        _logger = logger;
    }

    /// <summary>
    /// Effect para carregar estatísticas do dashboard quando LoadDashboardStatsAction é disparada
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadDashboardStatsAction(
        LoadDashboardStatsAction action, 
        IDispatcher dispatcher)
    {
        try
        {
            // Buscar primeira página com pageSize=1 apenas para obter TotalItems
            var allProvidersResult = await _providersApi.GetProvidersAsync(
                pageNumber: 1, 
                pageSize: 1);

            if (!allProvidersResult.IsSuccess || allProvidersResult.Value is null)
            {
                // Silencia erro 401 (race condition esperado durante inicialização)
                if (allProvidersResult.Error?.StatusCode == 401)
                {
                    dispatcher.Dispatch(new LoadDashboardStatsFailureAction("Autenticação necessária"));
                    return;
                }
                
                var errorMessage = allProvidersResult.Error?.Message 
                    ?? "Falha ao carregar estatísticas do dashboard";
                dispatcher.Dispatch(new LoadDashboardStatsFailureAction(errorMessage));
                return;
            }

            var totalProviders = allProvidersResult.Value.TotalItems;

            // Buscar providers com verificação pendente
            var pendingProvidersResult = await _providersApi
                .GetProvidersByVerificationStatusAsync(PENDING_STATUS);

            var pendingVerifications = 0;
            if (pendingProvidersResult.IsSuccess && pendingProvidersResult.Value is not null)
            {
                pendingVerifications = pendingProvidersResult.Value.Count;
            }
            else
            {
                _logger.LogWarning("Failed to load pending providers: {Error}", 
                    pendingProvidersResult.Error?.Message ?? "Unknown error");
            }

            // Buscar serviços ativos
            var servicesResult = await _serviceCatalogsApi.GetAllServicesAsync(activeOnly: true);
            var activeServices = 0;
            if (servicesResult.IsSuccess && servicesResult.Value is not null)
            {
                activeServices = servicesResult.Value.Count;
            }
            else
            {
                _logger.LogWarning("Failed to load active services: {Error}", 
                    servicesResult.Error?.Message ?? "Unknown error");
            }

            dispatcher.Dispatch(new LoadDashboardStatsSuccessAction(
                totalProviders,
                pendingVerifications,
                activeServices));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard statistics");
            dispatcher.Dispatch(new LoadDashboardStatsFailureAction(
                "Erro ao carregar estatísticas, tente novamente mais tarde"));
        }
    }
}
