using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using static MeAjudaAi.Web.Admin.Features.Dashboard.DashboardActions;

namespace MeAjudaAi.Web.Admin.Features.Dashboard;

/// <summary>
/// Effects do Fluxor para a feature de Dashboard.
/// Lida com side effects (chamadas assíncronas à API).
/// </summary>
public class DashboardEffects
{
    private readonly IProvidersApi _providersApi;
    private readonly IServiceCatalogsApi _serviceCatalogsApi;

    public DashboardEffects(IProvidersApi providersApi, IServiceCatalogsApi serviceCatalogsApi)
    {
        _providersApi = providersApi;
        _serviceCatalogsApi = serviceCatalogsApi;
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
                var errorMessage = allProvidersResult.Error?.Message 
                    ?? "Falha ao carregar estatísticas do dashboard";
                dispatcher.Dispatch(new LoadDashboardStatsFailureAction(errorMessage));
                return;
            }

            var totalProviders = allProvidersResult.Value.TotalItems;

            // Buscar providers com verificação pendente
            var pendingProvidersResult = await _providersApi
                .GetProvidersByVerificationStatusAsync("Pending");

            var pendingVerifications = pendingProvidersResult.IsSuccess && pendingProvidersResult.Value is not null
                ? pendingProvidersResult.Value.Count
                : 0;

            // Buscar serviços ativos
            var servicesResult = await _serviceCatalogsApi.GetAllServicesAsync(activeOnly: true);
            var activeServices = servicesResult.IsSuccess && servicesResult.Value is not null
                ? servicesResult.Value.Count
                : 0;

            dispatcher.Dispatch(new LoadDashboardStatsSuccessAction(
                totalProviders,
                pendingVerifications,
                activeServices));
        }
        catch (Exception ex)
        {
            var userFriendlyMessage = $"Erro ao carregar estatísticas: {ex.Message}";
            dispatcher.Dispatch(new LoadDashboardStatsFailureAction(userFriendlyMessage));
        }
    }
}
