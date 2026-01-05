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

    public DashboardEffects(IProvidersApi providersApi)
    {
        _providersApi = providersApi;
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

            // Active Services (placeholder - futura implementação quando houver API de Services)
            var activeServices = 0;

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
