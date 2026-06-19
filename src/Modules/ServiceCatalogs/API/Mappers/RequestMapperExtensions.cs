using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands do módulo ServiceCatalogs.
/// </summary>
public static class RequestMapperExtensions
{
    // ========== Service Mappers ==========

    /// <summary>
    /// Mapeia CreateServiceRequest para CreateServiceCommand.
    /// </summary>
    public static CreateServiceCommand ToCommand(this CreateServiceRequest request)
    {
        return new CreateServiceCommand(
            request.CategoryId,
            request.Name,
            request.Description,
            request.DisplayOrder);
    }

    /// <summary>
    /// Mapeia UpdateServiceRequest para UpdateServiceCommand.
    /// </summary>
    public static UpdateServiceCommand ToCommand(this UpdateServiceRequest request, Guid id)
    {
        return new UpdateServiceCommand(
            id,
            request.Name,
            request.Description,
            request.DisplayOrder);
    }

    /// <summary>
    /// Mapeia um Guid para DeleteServiceCommand.
    /// </summary>
    public static DeleteServiceCommand ToDeleteCommand(this Guid id)
    {
        return new DeleteServiceCommand(id);
    }

    /// <summary>
    /// Mapeia um Guid para ActivateServiceCommand.
    /// </summary>
    public static ActivateServiceCommand ToActivateCommand(this Guid id)
    {
        return new ActivateServiceCommand(id);
    }

    /// <summary>
    /// Mapeia um Guid para DeactivateServiceCommand.
    /// </summary>
    public static DeactivateServiceCommand ToDeactivateCommand(this Guid id)
    {
        return new DeactivateServiceCommand(id);
    }

    /// <summary>
    /// Mapeia ChangeServiceCategoryRequest para ChangeServiceCategoryCommand.
    /// </summary>
    public static ChangeServiceCategoryCommand ToCommand(this ChangeServiceCategoryRequest request, Guid serviceId)
    {
        return new ChangeServiceCategoryCommand(
            serviceId,
            request.NewCategoryId);
    }

    // ========== ServiceCategory Mappers ==========

    /// <summary>
    /// Mapeia CreateServiceCategoryRequest (API) para CreateServiceCategoryCommand.
    /// </summary>
    public static CreateServiceCategoryCommand ToCommand(this Endpoints.ServiceCategory.CreateServiceCategoryRequest request)
    {
        return new CreateServiceCategoryCommand(
            request.Name,
            request.Description,
            request.DisplayOrder);
    }

    /// <summary>
    /// Mapeia UpdateServiceCategoryRequest (API) para UpdateServiceCategoryCommand.
    /// </summary>
    public static UpdateServiceCategoryCommand ToCommand(this Endpoints.ServiceCategory.UpdateServiceCategoryRequest request, Guid id)
    {
        return new UpdateServiceCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.DisplayOrder);
    }

    /// <summary>
    /// Mapeia um Guid para DeleteServiceCategoryCommand.
    /// </summary>
    public static DeleteServiceCategoryCommand ToDeleteCategoryCommand(this Guid id)
    {
        return new DeleteServiceCategoryCommand(id);
    }

    /// <summary>
    /// Mapeia um Guid para ActivateServiceCategoryCommand.
    /// </summary>
    public static ActivateServiceCategoryCommand ToActivateCategoryCommand(this Guid id)
    {
        return new ActivateServiceCategoryCommand(id);
    }

    /// <summary>
    /// Mapeia um Guid para DeactivateServiceCategoryCommand.
    /// </summary>
    public static DeactivateServiceCategoryCommand ToDeactivateCategoryCommand(this Guid id)
    {
        return new DeactivateServiceCategoryCommand(id);
    }
}
