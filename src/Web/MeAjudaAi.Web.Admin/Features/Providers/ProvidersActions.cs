using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Providers;

/// <summary>
/// Actions do Fluxor para a feature de Providers.
/// Representa eventos/comandos que modificam o estado.
/// </summary>
public static class ProvidersActions
{
    /// <summary>
    /// Action para carregar lista de providers
    /// </summary>
    public record LoadProvidersAction(int PageNumber = 1, int PageSize = 20);

    /// <summary>
    /// Action disparada quando providers são carregados com sucesso
    /// </summary>
    public record LoadProvidersSuccessAction(
        IReadOnlyList<ModuleProviderDto> Providers,
        int TotalCount,
        int CurrentPage,
        int PageSize);

    /// <summary>
    /// Action disparada quando falha ao carregar providers
    /// </summary>
    public record LoadProvidersFailureAction(string ErrorMessage);

    /// <summary>
    /// Action para ir para a próxima página
    /// </summary>
    public record NextPageAction;

    /// <summary>
    /// Action para ir para a página anterior
    /// </summary>
    public record PreviousPageAction;

    /// <summary>
    /// Action para ir para uma página específica
    /// </summary>
    public record GoToPageAction(int PageNumber);
}
