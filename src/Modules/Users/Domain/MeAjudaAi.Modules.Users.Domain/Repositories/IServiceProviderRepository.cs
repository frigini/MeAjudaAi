using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Enums;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;

namespace MeAjudaAi.Modules.Users.Domain.Repositories;

public interface IServiceProviderRepository
{
    Task<ServiceProvider?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<ServiceProvider?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceProvider>> GetByTierAsync(EServiceProviderTier tier, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceProvider>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceProvider>> GetVerifiedAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<ServiceProvider> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        EServiceProviderTier? tier = null,
        bool? isVerified = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(ServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    Task DeleteAsync(ServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);
    Task<int> CountByTierAsync(EServiceProviderTier tier, CancellationToken cancellationToken = default);
}