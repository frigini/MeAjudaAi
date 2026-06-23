using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo Providers para outros módulos
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class ProvidersModuleApi(
    IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>> getProviderByIdHandler,
    IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>> getProviderByUserIdHandler,
    IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>> getProviderByDocumentHandler,
    IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByIdsHandler,
    ILocationsModuleApi locationApi,
    IProviderQueries providerQueries,
    ILogger<ProvidersModuleApi> logger) : IProvidersModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = ModuleNames.Providers;
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await providerQueries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByIdQuery(providerId);
        var result = await getProviderByIdHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : providerDto.ToContract()),
            onFailure: error => error.StatusCode == 404 ? Result<ModuleProviderDto?>.Success(null) : Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByDocumentQuery(document);
        var result = await getProviderByDocumentHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : providerDto.ToContract()),
            onFailure: error => error.StatusCode == 404 ? Result<ModuleProviderDto?>.Success(null) : Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default)
    {
        var idList = providerIds.ToList();
        var result = await getProvidersByIdsHandler.HandleAsync(new GetProvidersByIdsQuery(idList), cancellationToken);

        if (result.IsFailure) return Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(result.Error);

        if (result.Value == null) return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(new List<ModuleProviderBasicDto>());

        return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(result.Value.ToBasicContract());
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByUserIdQuery(userId);
        var result = await getProviderByUserIdHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : providerDto.ToContract()),
            onFailure: error => error.StatusCode == 404 ? Result<ModuleProviderDto?>.Success(null) : Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(
        IEnumerable<Guid> providerIds,
        CancellationToken cancellationToken = default)
    {
        var batchQuery = new GetProvidersByIdsQuery(providerIds.ToList());
        var result = await getProvidersByIdsHandler.HandleAsync(batchQuery, cancellationToken);

        if (result.IsFailure) return Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(result.Error);

        if (result.Value == null) return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(new List<ModuleProviderBasicDto>());

        return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(result.Value.ToBasicContract());
    }

    public async Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await providerQueries.ExistsAsync(new ProviderId(providerId), cancellationToken);
    }

    public async Task<Result<bool>> UserIsProviderAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await providerQueries.ExistsByUserIdAsync(userId, cancellationToken);
    }

    public async Task<Result<bool>> DocumentExistsAsync(string document, CancellationToken cancellationToken = default)
    {
        var provider = await providerQueries.GetByDocumentAsync(document, cancellationToken);
        return Result<bool>.Success(provider != null);
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        var providers = await providerQueries.GetByCityAsync(city, cancellationToken);
        var dtos = new List<ModuleProviderBasicDto>();
        foreach (var p in providers)
        {
             dtos.Add(new ModuleProviderBasicDto(
                Id: p.Id.Value,
                Name: p.Name,
                Slug: p.Slug,
                Email: p.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
                ProviderType: p.Type.ToDescription(),
                VerificationStatus: p.VerificationStatus.ToDescription(),
                IsActive: p.IsActive));
        }
        return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(
        string state,
        CancellationToken cancellationToken = default)
    {
        var providers = await providerQueries.GetByStateAsync(state, cancellationToken);
        var dtos = new List<ModuleProviderBasicDto>();
        foreach (var p in providers)
        {
             dtos.Add(new ModuleProviderBasicDto(
                Id: p.Id.Value,
                Name: p.Name,
                Slug: p.Slug,
                Email: p.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
                ProviderType: p.Type.ToDescription(),
                VerificationStatus: p.VerificationStatus.ToDescription(),
                IsActive: p.IsActive));
        }
        return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(
        string providerType,
        CancellationToken cancellationToken = default)
    {
        var enumResult = providerType.ToEnum<EProviderType>();
        if (enumResult.IsFailure) return Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(enumResult.Error);

        var providers = await providerQueries.GetByTypeAsync(enumResult.Value, cancellationToken);
        var dtos = new List<ModuleProviderBasicDto>();
        foreach (var p in providers)
        {
             dtos.Add(new ModuleProviderBasicDto(
                Id: p.Id.Value,
                Name: p.Name,
                Slug: p.Slug,
                Email: p.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
                ProviderType: p.Type.ToDescription(),
                VerificationStatus: p.VerificationStatus.ToDescription(),
                IsActive: p.IsActive));
        }
        return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(
        string verificationStatus,
        CancellationToken cancellationToken = default)
    {
        var enumResult = verificationStatus.ToEnum<EVerificationStatus>();
        if (enumResult.IsFailure) return Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(enumResult.Error);

        var providers = await providerQueries.GetByVerificationStatusAsync(enumResult.Value, cancellationToken);
        var dtos = new List<ModuleProviderBasicDto>();
        foreach (var p in providers)
        {
             dtos.Add(new ModuleProviderBasicDto(
                Id: p.Id.Value,
                Name: p.Name,
                Slug: p.Slug,
                Email: p.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
                ProviderType: p.Type.ToDescription(),
                VerificationStatus: p.VerificationStatus.ToDescription(),
                IsActive: p.IsActive));
        }
        return Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(dtos);
    }

    public async Task<Result<ModuleProviderIndexingDto?>> GetProviderForIndexingAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting provider indexing data for provider {ProviderId}", providerId);

        var providerEntity = await providerQueries.GetByIdAsync(new ProviderId(providerId), cancellationToken);

        if (providerEntity == null) return Result<ModuleProviderIndexingDto?>.Success(null);

        var address = providerEntity.BusinessProfile.PrimaryAddress;
        var fullAddress = $"{address.Street}, {address.Number}, {address.Neighborhood}, {address.City}/{address.State}, {address.ZipCode}";

        var coordinatesResult = await locationApi.GetCoordinatesFromAddressAsync(fullAddress, cancellationToken);
        if (coordinatesResult.IsFailure) return Result<ModuleProviderIndexingDto?>.Failure(coordinatesResult.Error);

        var coordinates = coordinatesResult.Value;
        var indexingDto = new ModuleProviderIndexingDto(
            ProviderId: providerEntity.Id.Value,
            Name: providerEntity.Name,
            Slug: providerEntity.Slug,
            Latitude: coordinates.Latitude,
            Longitude: coordinates.Longitude,
            ServiceIds: providerEntity.GetServiceIds(),
            AverageRating: 0, 
            TotalReviews: 0, 
            SubscriptionTier: ESubscriptionTier.Free,
            IsActive: providerEntity.VerificationStatus == EVerificationStatus.Verified && !providerEntity.IsDeleted,
            Description: providerEntity.BusinessProfile.Description,
            City: address.City,
            State: address.State);

        return Result<ModuleProviderIndexingDto?>.Success(indexingDto);
    }

    public async Task<Result<IReadOnlyList<Guid>>> GetProvidersByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var providers = await providerQueries.GetByServiceIdAsync(serviceId, cancellationToken);
        return Result<IReadOnlyList<Guid>>.Success(providers.Select(p => p.Id.Value).ToList());
    }

    public async Task<Result<bool>> HasProvidersOfferingServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return Result<bool>.Success(await providerQueries.HasProvidersWithServiceAsync(serviceId, cancellationToken));
    }

    public async Task<Result<bool>> IsServiceOfferedByProviderAsync(Guid providerId, Guid serviceId, CancellationToken cancellationToken = default)
    {
        var provider = await providerQueries.GetByIdAsync(new ProviderId(providerId), cancellationToken);
        return Result<bool>.Success(provider?.OffersService(serviceId) ?? false);
    }
}
