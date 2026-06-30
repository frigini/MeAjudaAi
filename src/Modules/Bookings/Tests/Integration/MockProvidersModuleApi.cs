using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class MockProvidersModuleApi : IProvidersModuleApi
{
    private readonly Dictionary<Guid, ModuleProviderDto> _providers = new();
    private readonly Dictionary<Guid, Guid> _userProviderMap = new();
    private readonly Dictionary<(Guid ProviderId, Guid ServiceId), bool> _providerServices = new();

    public string ModuleName => "Providers";
    public string ApiVersion => "1.0";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public void SeedProvider(Guid providerId, Guid userId, string name = "Test Provider")
    {
        _providers[providerId] = new ModuleProviderDto(
            providerId, name, "test-provider", $"{name.ToLower().Replace(" ", "-")}@test.com", "12345678900",
            "Individual", "Active", DateTime.UtcNow, DateTime.UtcNow, true, null, null);
        _userProviderMap[userId] = providerId;
    }

    public void SeedProviderService(Guid providerId, Guid serviceId)
    {
        _providerServices[(providerId, serviceId)] = true;
    }

    public Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        _providers.TryGetValue(providerId, out var dto);
        return Task.FromResult(Result<ModuleProviderDto?>.Success(dto));
    }

    public Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_userProviderMap.TryGetValue(userId, out var providerId) && _providers.TryGetValue(providerId, out var dto))
            return Task.FromResult(Result<ModuleProviderDto?>.Success(dto));
        return Task.FromResult(Result<ModuleProviderDto?>.Success(null));
    }

    public Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<ModuleProviderDto?>.Success(null));

    public Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(_providers.ContainsKey(providerId)));

    public Task<Result<bool>> UserIsProviderAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(_userProviderMap.ContainsKey(userId)));

    public Task<Result<bool>> DocumentExistsAsync(string document, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(false));

    public Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(Array.Empty<ModuleProviderBasicDto>()));

    public Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(Array.Empty<ModuleProviderBasicDto>()));

    public Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(string city, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(Array.Empty<ModuleProviderBasicDto>()));

    public Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(string state, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(Array.Empty<ModuleProviderBasicDto>()));

    public Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(string providerType, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(Array.Empty<ModuleProviderBasicDto>()));

    public Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(string verificationStatus, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(Array.Empty<ModuleProviderBasicDto>()));

    public Task<Result<ModuleProviderIndexingDto?>> GetProviderForIndexingAsync(Guid providerId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<ModuleProviderIndexingDto?>.Success(null));

    public Task<Result<IReadOnlyList<Guid>>> GetProvidersByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<Guid>>.Success(Array.Empty<Guid>()));

    public Task<Result<bool>> HasProvidersOfferingServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(false));

    public Task<Result<bool>> IsServiceOfferedByProviderAsync(Guid providerId, Guid serviceId, CancellationToken cancellationToken = default)
    {
        var result = _providerServices.ContainsKey((providerId, serviceId));
        return Task.FromResult(Result<bool>.Success(result));
    }
}
