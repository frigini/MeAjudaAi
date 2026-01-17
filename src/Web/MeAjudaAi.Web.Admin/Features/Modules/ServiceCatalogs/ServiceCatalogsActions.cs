using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs;

/// <summary>
/// Actions para gerenciamento de catálogo de serviços
/// </summary>
public static class ServiceCatalogsActions
{
    // ========== CATEGORIES ==========

    /// <summary>
    /// Carrega todas as categorias
    /// </summary>
    public sealed record LoadCategoriesAction(bool ActiveOnly = true);

    /// <summary>
    /// Sucesso ao carregar categorias
    /// </summary>
    public sealed record LoadCategoriesSuccessAction(IReadOnlyList<ModuleServiceCategoryDto> Categories);

    /// <summary>
    /// Falha ao carregar categorias
    /// </summary>
    public sealed record LoadCategoriesFailureAction(string ErrorMessage);

    /// <summary>
    /// Adiciona categoria (pós-create)
    /// </summary>
    public sealed record AddCategoryAction(ModuleServiceCategoryDto Category);

    /// <summary>
    /// Remove categoria (pós-delete)
    /// </summary>
    public sealed record RemoveCategoryAction(Guid CategoryId);

    /// <summary>
    /// Atualiza categoria (pós-update)
    /// </summary>
    public sealed record UpdateCategoryAction(Guid CategoryId, string Name, string? Description, int DisplayOrder);

    // ========== SERVICES ==========

    /// <summary>
    /// Carrega todos os serviços
    /// </summary>
    public sealed record LoadServicesAction(bool ActiveOnly = true);

    /// <summary>
    /// Sucesso ao carregar serviços
    /// </summary>
    public sealed record LoadServicesSuccessAction(IReadOnlyList<ModuleServiceListDto> Services);

    /// <summary>
    /// Falha ao carregar serviços
    /// </summary>
    public sealed record LoadServicesFailureAction(string ErrorMessage);

    /// <summary>
    /// Adiciona serviço (pós-create)
    /// </summary>
    public sealed record AddServiceAction(ModuleServiceListDto Service);

    /// <summary>
    /// Remove serviço (pós-delete)
    /// </summary>
    public sealed record RemoveServiceAction(Guid ServiceId);

    /// <summary>
    /// Atualiza serviço (pós-update)
    /// </summary>
    public sealed record UpdateServiceAction(Guid ServiceId, string Name, string? Description);

    /// <summary>
    /// Atualiza status de ativação de categoria
    /// </summary>
    public sealed record UpdateCategoryActiveStatusAction(Guid CategoryId, bool IsActive);

    /// <summary>
    /// Atualiza status de ativação de serviço
    /// </summary>
    public sealed record UpdateServiceActiveStatusAction(Guid ServiceId, bool IsActive);

    /// <summary>
    /// Limpa erro atual
    /// </summary>
    public sealed record ClearErrorAction;

    // ========== CATEGORY DELETE OPERATIONS ==========

    /// <summary>
    /// Solicita exclusão de categoria
    /// </summary>
    public sealed record DeleteCategoryAction(Guid CategoryId);

    /// <summary>
    /// Sucesso ao excluir categoria
    /// </summary>
    public sealed record DeleteCategorySuccessAction(Guid CategoryId);

    /// <summary>
    /// Falha ao excluir categoria
    /// </summary>
    public sealed record DeleteCategoryFailureAction(Guid CategoryId, string ErrorMessage);

    // ========== CATEGORY TOGGLE OPERATIONS ==========

    /// <summary>
    /// Solicita alteração de status de ativação de categoria
    /// </summary>
    public sealed record ToggleCategoryActivationAction(Guid CategoryId, bool Activate);

    /// <summary>
    /// Sucesso ao alterar status de ativação
    /// </summary>
    public sealed record ToggleCategoryActivationSuccessAction(Guid CategoryId, bool IsActive);

    /// <summary>
    /// Falha ao alterar status de ativação
    /// </summary>
    public sealed record ToggleCategoryActivationFailureAction(Guid CategoryId, string ErrorMessage);

    // ========== SERVICE DELETE OPERATIONS ==========

    /// <summary>
    /// Solicita exclusão de serviço
    /// </summary>
    public sealed record DeleteServiceAction(Guid ServiceId);

    /// <summary>
    /// Sucesso ao excluir serviço
    /// </summary>
    public sealed record DeleteServiceSuccessAction(Guid ServiceId);

    /// <summary>
    /// Falha ao excluir serviço
    /// </summary>
    public sealed record DeleteServiceFailureAction(Guid ServiceId, string ErrorMessage);

    // ========== SERVICE TOGGLE OPERATIONS ==========

    /// <summary>
    /// Solicita alteração de status de ativação de serviço
    /// </summary>
    public sealed record ToggleServiceActivationAction(Guid ServiceId, bool Activate);

    /// <summary>
    /// Sucesso ao alterar status de ativação de serviço
    /// </summary>
    public sealed record ToggleServiceActivationSuccessAction(Guid ServiceId, bool IsActive);

    /// <summary>
    /// Falha ao alterar status de ativação de serviço
    /// </summary>
    public sealed record ToggleServiceActivationFailureAction(Guid ServiceId, string ErrorMessage);
}
