using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class MockServiceCatalogsModuleApi : IServiceCatalogsModuleApi
{
    private readonly Dictionary<Guid, ModuleServiceDto> _services = new();
    private readonly Dictionary<Guid, ModuleServiceCategoryDto> _categories = new();

    public string ModuleName => "ServiceCatalogs";
    public string ApiVersion => "1.0";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public void SeedService(Guid serviceId, Guid categoryId, string name = "Test Service", bool isActive = true)
    {
        _services[serviceId] = new ModuleServiceDto(
            serviceId, null, categoryId, "Test Category", name, null, isActive);
    }

    public void SeedCategory(Guid categoryId, string name = "Test Category", bool isActive = true)
    {
        _categories[categoryId] = new ModuleServiceCategoryDto(
            categoryId, name, null, isActive, 1);
    }

    public Task<Result<ModuleServiceCategoryDto?>> GetServiceCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        _categories.TryGetValue(categoryId, out var dto);
        return Task.FromResult(Result<ModuleServiceCategoryDto?>.Success(dto));
    }

    public Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = _categories.Values.Where(c => !activeOnly || c.IsActive).ToList();
        return Task.FromResult(Result<IReadOnlyList<ModuleServiceCategoryDto>>.Success(result));
    }

    public Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        _services.TryGetValue(serviceId, out var dto);
        return Task.FromResult(Result<ModuleServiceDto?>.Success(dto));
    }

    public Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = _services.Values.Where(s => !activeOnly || s.IsActive)
            .Select(s => new ModuleServiceListDto(s.Id, s.CategoryId, s.Name, s.Description, 0, s.IsActive))
            .ToList();
        return Task.FromResult(Result<IReadOnlyList<ModuleServiceListDto>>.Success(result));
    }

    public Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(Guid categoryId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = _services.Values.Where(s => s.CategoryId == categoryId && (!activeOnly || s.IsActive)).ToList();
        return Task.FromResult(Result<IReadOnlyList<ModuleServiceDto>>.Success(result));
    }

    public Task<Result<bool>> IsServiceActiveAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var exists = _services.TryGetValue(serviceId, out var dto) && dto.IsActive;
        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(IReadOnlyCollection<Guid> serviceIds, CancellationToken cancellationToken = default)
    {
        var invalidIds = serviceIds.Where(id => !_services.ContainsKey(id) || !_services[id].IsActive).ToList();
        var isValid = invalidIds.Count == 0;
        return Task.FromResult(Result<ModuleServiceValidationResultDto>.Success(
            new ModuleServiceValidationResultDto(isValid, invalidIds, Array.Empty<Guid>())));
    }
}
