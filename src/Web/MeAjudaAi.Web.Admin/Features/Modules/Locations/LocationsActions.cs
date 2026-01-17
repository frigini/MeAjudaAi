using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Modules.Locations;

/// <summary>
/// Actions Fluxor para gerenciamento de cidades permitidas.
/// </summary>
public static class LocationsActions
{
    // --- Load Actions ---

    /// <summary>Inicia carregamento de todas as cidades permitidas</summary>
    public sealed record LoadAllowedCitiesAction(bool OnlyActive = true);

    /// <summary>Sucesso ao carregar cidades permitidas</summary>
    public sealed record LoadAllowedCitiesSuccessAction(IReadOnlyList<ModuleAllowedCityDto> Cities);

    /// <summary>Falha ao carregar cidades permitidas</summary>
    public sealed record LoadAllowedCitiesFailureAction(string ErrorMessage);

    // --- Create Actions ---

    /// <summary>Adiciona nova cidade à lista local (após criação no backend)</summary>
    public sealed record AddAllowedCityAction(ModuleAllowedCityDto City);

    // --- Update Actions ---

    /// <summary>Atualiza cidade na lista local (após update no backend)</summary>
    public sealed record UpdateAllowedCityAction(Guid CityId, ModuleAllowedCityDto UpdatedCity);

    // --- Delete Actions ---

    /// <summary>Remove cidade da lista local (após delete no backend)</summary>
    public sealed record RemoveAllowedCityAction(Guid CityId);

    // --- Toggle Active Actions ---

    /// <summary>Atualiza status IsActive de uma cidade</summary>
    public sealed record UpdateCityActiveStatusAction(Guid CityId, bool IsActive);

    // --- Error Actions ---

    /// <summary>Limpa mensagens de erro</summary>
    public sealed record ClearErrorAction;

    // ========== DELETE OPERATIONS ==========

    /// <summary>Solicita exclusão de cidade permitida</summary>
    public sealed record DeleteAllowedCityAction(Guid CityId);

    /// <summary>Sucesso ao excluir cidade</summary>
    public sealed record DeleteAllowedCitySuccessAction(Guid CityId);

    /// <summary>Falha ao excluir cidade</summary>
    public sealed record DeleteAllowedCityFailureAction(Guid CityId, string ErrorMessage);

    // ========== TOGGLE OPERATIONS ==========

    /// <summary>Solicita alteração de status de ativação de cidade</summary>
    public sealed record ToggleCityActivationAction(Guid CityId, bool Activate, ModuleAllowedCityDto City);

    /// <summary>Sucesso ao alterar status de ativação</summary>
    public sealed record ToggleCityActivationSuccessAction(Guid CityId, bool IsActive);

    /// <summary>Falha ao alterar status de ativação</summary>
    public sealed record ToggleCityActivationFailureAction(Guid CityId, string ErrorMessage);
}
