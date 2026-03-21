namespace MeAjudaAi.Web.Admin.Features.Dashboard;

/// <summary>
/// Actions do Fluxor para a feature de Dashboard.
/// Define todas as ações que podem ser disparadas para modificar o DashboardState.
/// </summary>
public static class DashboardActions
{
    /// <summary>
    /// Action para iniciar carregamento das estatísticas do dashboard
    /// </summary>
    public sealed record LoadDashboardStatsAction;

    /// <summary>
    /// Action disparada quando as estatísticas são carregadas com sucesso
    /// </summary>
    public sealed record LoadDashboardStatsSuccessAction(
        int TotalProviders,
        int PendingVerifications,
        int ActiveServices);

    /// <summary>
    /// Action disparada quando ocorre erro ao carregar estatísticas
    /// </summary>
    public sealed record LoadDashboardStatsFailureAction(string ErrorMessage);
}
