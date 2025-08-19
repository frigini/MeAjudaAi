using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Services;

public interface IServiceProviderService
{
    Task<Result<ServiceProviderDto>> CreateServiceProviderAsync(Guid userId, string companyName, string? taxId = null, CancellationToken cancellationToken = default);
    Task<Result<ServiceProviderDto>> GetServiceProviderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ServiceProviderDto>> GetServiceProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ServiceProviderDto>> UpdateServiceProviderAsync(Guid id, string companyName, string? description, string? taxId = null, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateTierAsync(Guid id, string tier, string changedBy, CancellationToken cancellationToken = default);
    Task<Result<bool>> VerifyServiceProviderAsync(Guid id, string verifiedBy, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateSubscriptionAsync(Guid id, string subscriptionId, string status, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    
    Task<Result<PagedResponse<IEnumerable<ServiceProviderDto>>>> GetServiceProvidersAsync(
        int pageNumber = 1, 
        int pageSize = 10, 
        string? searchTerm = null, 
        string? tier = null, 
        bool? isVerified = null,
        CancellationToken cancellationToken = default);
}