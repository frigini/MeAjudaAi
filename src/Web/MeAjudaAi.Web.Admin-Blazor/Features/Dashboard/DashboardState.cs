using Fluxor;

namespace MeAjudaAi.Web.Admin.Features.Dashboard;

/// <summary>
/// Estado do Fluxor para a feature de Dashboard.
/// Armazena KPIs e estatísticas do sistema.
/// </summary>
[FeatureState]
public sealed record DashboardState
{
    /// <summary>
    /// Total de providers cadastrados no sistema
    /// </summary>
    public int TotalProviders { get; init; }

    /// <summary>
    /// Total de providers com verificação pendente
    /// </summary>
    public int PendingVerifications { get; init; }

    /// <summary>
    /// Total de serviços ativos (futura implementação)
    /// </summary>
    public int ActiveServices { get; init; }

    /// <summary>
    /// Indica se os dados estão sendo carregados
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Mensagem de erro, se houver
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Construtor padrão com valores iniciais
    /// </summary>
    public DashboardState()
    {
        TotalProviders = 0;
        PendingVerifications = 0;
        ActiveServices = 0;
        IsLoading = false;
        ErrorMessage = null;
    }
}
