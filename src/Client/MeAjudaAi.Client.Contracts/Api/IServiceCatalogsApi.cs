using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IServiceCatalogsApi
{
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Categories}")]
    Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(
        [Query] bool activeOnly = true,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}")]
    Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(
        [Query] bool activeOnly = true,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}/{{serviceId}}")]
    Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}/category/{{categoryId}}")]
    Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(
        Guid categoryId,
        [Query] bool activeOnly = true,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Categories}")]
    Task<Result<ModuleServiceCategoryDto>> CreateCategoryAsync(
        [Body] CreateServiceCatalogCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Categories}/{{categoryId}}")]
    Task<Result<Unit>> UpdateCategoryAsync(
        Guid categoryId,
        [Body] UpdateServiceCatalogCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    [Delete($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Categories}/{{categoryId}}")]
    Task<Result<Unit>> DeleteCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Categories}/{{categoryId}}/activate")]
    Task<Result<Unit>> ActivateCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Categories}/{{categoryId}}/deactivate")]
    Task<Result<Unit>> DeactivateCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}")]
    Task<Result<ModuleServiceDto>> CreateServiceAsync(
        [Body] CreateServiceRequestDto request,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}/{{serviceId}}")]
    Task<Result<Unit>> UpdateServiceAsync(
        Guid serviceId,
        [Body] UpdateServiceRequestDto request,
        CancellationToken cancellationToken = default);

    [Delete($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}/{{serviceId}}")]
    Task<Result<Unit>> DeleteServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}/{{serviceId}}/activate")]
    Task<Result<Unit>> ActivateServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.ServiceCatalogs.Services}/{{serviceId}}/deactivate")]
    Task<Result<Unit>> DeactivateServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);
}