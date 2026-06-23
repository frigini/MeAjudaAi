using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Contracts.Functional;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IProvidersApi
{
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}")]
    Task<Result<PagedResult<ModuleProviderDto>>> GetProvidersAsync(
        [Query] int pageNumber = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.GetById}")]
    Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}/type/{{providerType}}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(
        string providerType,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.GetByVerificationStatus}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(
        [AliasAs("status")] string verificationStatus,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.GetByCity}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(
        string city,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.GetByState}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(
        string state,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}/document/{{document}}")]
    Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(
        string document,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}")]
    Task<Result<ModuleProviderDto>> CreateProviderAsync(
        [Body] CreateProviderRequestDto request,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.GetById}")]
    Task<Result<Unit>> UpdateProviderAsync(
        Guid id,
        [Body] UpdateProviderRequestDto request,
        CancellationToken cancellationToken = default);

    [Delete($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.GetById}")]
    Task<Result<Unit>> DeleteProviderAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Providers.Base}{ApiEndpoints.Providers.UpdateVerificationStatus}")]
    Task<Result<Unit>> UpdateVerificationStatusAsync(
        Guid id,
        [Body] UpdateVerificationStatusRequestDto request,
        CancellationToken cancellationToken = default);
}