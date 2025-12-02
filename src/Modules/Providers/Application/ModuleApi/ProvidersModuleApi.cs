using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Locations;
using MeAjudaAi.Shared.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
    IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByCityHandler,
    IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByStateHandler,
    IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByTypeHandler,
    IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByVerificationStatusHandler,
    ILocationsModuleApi locationApi,
    IProviderRepository providerRepository,
    IServiceProvider serviceProvider,
    ILogger<ProvidersModuleApi> logger) : IProvidersModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "Providers";
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Providers module availability");

            // Verifica health checks registrados do sistema
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService != null)
            {
                var healthReport = await healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains("providers") || check.Tags.Contains("database"),
                    cancellationToken);

                // Se algum health check crítico falhou, o módulo não está disponível
                if (healthReport.Status == HealthStatus.Unhealthy)
                {
                    logger.LogWarning("Providers module unavailable due to failed health checks: {FailedChecks}",
                        string.Join(", ", healthReport.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy).Select(e => e.Key)));
                    return false;
                }
            }

            // Testa funcionalidade básica - verifica se os handlers essenciais estão disponíveis
            var canExecuteBasicOperations = await CanExecuteBasicOperationsAsync(cancellationToken);
            if (!canExecuteBasicOperations)
            {
                logger.LogWarning("Providers module unavailable - basic operations test failed");
                return false;
            }

            logger.LogDebug("Providers module is available and healthy");
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Providers module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Providers module availability");
            return false;
        }
    }

    /// <summary>
    /// Testa se as operações básicas do módulo estão funcionando
    /// </summary>
    private async Task<bool> CanExecuteBasicOperationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Teste básico: tentar buscar por ID não existente (deve retornar NotFound ou Success com null)
            var testId = Guid.NewGuid();
            var result = await GetProviderByIdAsync(testId, cancellationToken);

            // Sucesso se: 1) Success com null, 2) Failure com NotFound
            if (result.IsSuccess && result.Value == null)
                return true;

            if (!result.IsSuccess && result.Error.StatusCode == 404)
                return true;

            return false;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Basic operations test failed for Providers module");
            return false;
        }
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByIdQuery(providerId);
        var result = await getProviderByIdHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : MapToModuleDto(providerDto)),
            onFailure: error => error.StatusCode == 404 ? Result<ModuleProviderDto?>.Success(null) : Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByDocumentQuery(document);
        var result = await getProviderByDocumentHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : MapToModuleDto(providerDto)),
            onFailure: error => error.StatusCode == 404 ? Result<ModuleProviderDto?>.Success(null) : Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting basic provider info for {Count} provider IDs", providerIds.Count());

        var result = await getProvidersByIdsHandler.HandleAsync(new GetProvidersByIdsQuery(providerIds.ToList()), cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByUserIdQuery(userId);
        var result = await getProviderByUserIdHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : MapToModuleDto(providerDto)),
            onFailure: error => error.StatusCode == 404 ? Result<ModuleProviderDto?>.Success(null) : Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(
        IEnumerable<Guid> providerIds,
        CancellationToken cancellationToken = default)
    {
        var batchQuery = new GetProvidersByIdsQuery(providerIds.ToList());
        var result = await getProvidersByIdsHandler.HandleAsync(batchQuery, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var result = await GetProviderByIdAsync(providerId, cancellationToken);
        return result.Match(
            onSuccess: provider => Result<bool>.Success(provider != null),
            onFailure: error => Result<bool>.Failure(error)
        );
    }

    public async Task<Result<bool>> UserIsProviderAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await GetProviderByUserIdAsync(userId, cancellationToken);
        return result.Match(
            onSuccess: provider => Result<bool>.Success(provider != null), // Verdadeiro se o provider existir, falso se for null
            onFailure: error => Result<bool>.Failure(error) // Propaga quaisquer erros
        );
    }

    public async Task<Result<bool>> DocumentExistsAsync(string document, CancellationToken cancellationToken = default)
    {
        var result = await GetProviderByDocumentAsync(document, cancellationToken);
        return result.Match(
            onSuccess: provider => Result<bool>.Success(provider != null),
            onFailure: error => Result<bool>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProvidersByCityQuery(city);
        var result = await getProvidersByCityHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(
        string state,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProvidersByStateQuery(state);
        var result = await getProvidersByStateHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(
        string providerType,
        CancellationToken cancellationToken = default)
    {
        var enumResult = providerType.ToEnum<EProviderType>();
        if (enumResult.IsFailure)
        {
            return Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(enumResult.Error);
        }

        var query = new GetProvidersByTypeQuery(enumResult.Value);
        var result = await getProvidersByTypeHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(
        string verificationStatus,
        CancellationToken cancellationToken = default)
    {
        var enumResult = verificationStatus.ToEnum<EVerificationStatus>();
        if (enumResult.IsFailure)
        {
            return Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(enumResult.Error);
        }

        var query = new GetProvidersByVerificationStatusQuery(enumResult.Value);
        var result = await getProvidersByVerificationStatusHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    /// <summary>
    /// Gets provider data prepared for search indexing, including coordinates obtained via geocoding.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provider indexing DTO with geocoded location, or null if provider not found, or failure if geocoding fails.</returns>
    public async Task<Result<ModuleProviderIndexingDto?>> GetProviderForIndexingAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting provider indexing data for provider {ProviderId}", providerId);

        // 1. Buscar entidade Provider diretamente do repositório (inclui Services)
        var providerEntity = await providerRepository.GetByIdAsync(new ProviderId(providerId), cancellationToken);

        if (providerEntity == null)
        {
            logger.LogDebug("Provider {ProviderId} not found for indexing", providerId);
            return Result<ModuleProviderIndexingDto?>.Success(null);
        }

        // 2. Obter coordenadas do endereço primário via ILocationsModuleApi
        var address = providerEntity.BusinessProfile.PrimaryAddress;
        var fullAddress = $"{address.Street}, {address.Number}, {address.Neighborhood}, {address.City}/{address.State}, {address.ZipCode}";

        var coordinatesResult = await locationApi.GetCoordinatesFromAddressAsync(fullAddress, cancellationToken);

        if (coordinatesResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to get coordinates for provider {ProviderId} address '{Address}': {Error}",
                providerId, fullAddress, coordinatesResult.Error.Message);

            // Sem coordenadas não podemos indexar (SearchableProvider exige Location)
            return Result<ModuleProviderIndexingDto?>.Failure(coordinatesResult.Error);
        }

        var coordinates = coordinatesResult.Value;

        // 3. Obter ServiceIds do provider (da coleção Services)
        var serviceIds = providerEntity.GetServiceIds();

        // 4. TODO: Buscar rating e reviews do provider (quando implementarmos Reviews)
        // Por enquanto, valores padrão
        decimal averageRating = 0;
        int totalReviews = 0;

        // 5. TODO: Mapear subscription tier do provider
        // Por enquanto, Free como padrão
        var subscriptionTier = ESubscriptionTier.Free;

        // 6. Criar DTO de indexação
        var indexingDto = new ModuleProviderIndexingDto
        {
            ProviderId = providerEntity.Id.Value,
            Name = providerEntity.Name,
            Description = providerEntity.BusinessProfile.Description,
            Latitude = coordinates.Latitude,
            Longitude = coordinates.Longitude,
            ServiceIds = serviceIds,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            SubscriptionTier = subscriptionTier,
            City = address.City,
            State = address.State,
            IsActive = providerEntity.VerificationStatus == EVerificationStatus.Verified && !providerEntity.IsDeleted
        };

        logger.LogInformation(
            "Successfully prepared indexing data for provider {ProviderId} at ({Lat}, {Lon})",
            providerId, coordinates.Latitude, coordinates.Longitude);

        return Result<ModuleProviderIndexingDto?>.Success(indexingDto);
    }

    /// <summary>
    /// Mapeia ProviderDto interno para ModuleProviderDto público
    /// </summary>
    private static ModuleProviderDto MapToModuleDto(ProviderDto providerDto)
    {
        return new ModuleProviderDto
        {
            Id = providerDto.Id,
            Name = providerDto.Name,
            Email = providerDto.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
            Document = GetMainDocument(providerDto)?.Number ?? string.Empty,
            Phone = providerDto.BusinessProfile?.ContactInfo?.PhoneNumber,
            ProviderType = providerDto.Type.ToString(),
            VerificationStatus = providerDto.VerificationStatus.ToString(),
            CreatedAt = providerDto.CreatedAt,
            UpdatedAt = providerDto.UpdatedAt ?? providerDto.CreatedAt,
            IsActive = !providerDto.IsDeleted
        };
    }

    /// <summary>
    /// Mapeia ProviderDto interno para ModuleProviderBasicDto público
    /// </summary>
    private static ModuleProviderBasicDto MapToModuleBasicDto(ProviderDto providerDto)
    {
        return new ModuleProviderBasicDto
        {
            Id = providerDto.Id,
            Name = providerDto.Name,
            Email = providerDto.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
            ProviderType = providerDto.Type.ToString(),
            VerificationStatus = providerDto.VerificationStatus.ToString(),
            IsActive = !providerDto.IsDeleted
        };
    }

    /// <summary>
    /// Obtém o documento primário do provider
    /// </summary>
    /// <param name="providerDto">DTO do provider</param>
    /// <returns>O documento primário ou null se não houver</returns>
    private static DocumentDto? GetPrimaryDocument(ProviderDto providerDto)
    {
        return providerDto.Documents?.FirstOrDefault(d => d.IsPrimary);
    }

    /// <summary>
    /// Obtém o documento principal (primário ou primeiro disponível)
    /// </summary>
    /// <param name="providerDto">DTO do provider</param>
    /// <returns>O documento principal ou null se não houver documentos</returns>
    private static DocumentDto? GetMainDocument(ProviderDto providerDto)
    {
        return GetPrimaryDocument(providerDto) ?? providerDto.Documents?.FirstOrDefault();
    }
}
