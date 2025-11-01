using MeAjudaAi.Modules.Providers.Application.Contracts;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Services;

/// <summary>
/// Implementação da API pública do módulo Providers para outros módulos
/// </summary>
[ModuleApi("Providers", "1.0")]
public sealed class ProvidersModuleApi(
    IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>> getProviderByIdHandler,
    IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>> getProviderByUserIdHandler,
    IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>> getProviderByDocumentHandler,
    IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByIdsHandler,
    IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByCityHandler,
    IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByStateHandler,
    IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByTypeHandler,
    IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>> getProvidersByVerificationStatusHandler,
    IServiceProvider serviceProvider,
    ILogger<ProvidersModuleApi> logger) : IProvidersModuleApi
{
    public string ModuleName => "Providers";
    public string ApiVersion => "1.0";

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
            onFailure: error => Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderByDocumentQuery(document);
        var result = await getProviderByDocumentHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDto => Result<ModuleProviderDto?>.Success(providerDto == null ? null : MapToModuleDto(providerDto)),
            onFailure: error => Result<ModuleProviderDto?>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default)
    {
        var batchQuery = new GetProvidersByIdsQuery(providerIds.ToList());
        var result = await getProvidersByIdsHandler.HandleAsync(batchQuery, cancellationToken);

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
            onFailure: error => Result<ModuleProviderDto?>.Failure(error)
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
            onSuccess: provider => Result<bool>.Success(provider != null), // True if provider exists, false if null
            onFailure: error => Result<bool>.Failure(error) // Propagate any errors
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
        EProviderType type,
        CancellationToken cancellationToken = default)
    {
        // Mapear o enum compartilhado para o enum do domínio
        var domainType = (Modules.Providers.Domain.Enums.EProviderType)(int)type;
        var query = new GetProvidersByTypeQuery(domainType);
        var result = await getProvidersByTypeHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(
        EVerificationStatus status,
        CancellationToken cancellationToken = default)
    {
        // Mapear o enum compartilhado para o enum do domínio
        var domainStatus = (Modules.Providers.Domain.Enums.EVerificationStatus)(int)status;
        var query = new GetProvidersByVerificationStatusQuery(domainStatus);
        var result = await getProvidersByVerificationStatusHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: providerDtos => Result<IReadOnlyList<ModuleProviderBasicDto>>.Success(
                providerDtos.Select(MapToModuleBasicDto).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleProviderBasicDto>>.Failure(error)
        );
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
            // Takes the first document as there's no primary document concept yet
            // TODO: Consider implementing a primary document flag if needed
            Document = providerDto.Documents?.FirstOrDefault()?.Number ?? string.Empty,
            Phone = providerDto.BusinessProfile?.ContactInfo?.PhoneNumber,
            ProviderType = (EProviderType)(int)providerDto.Type,
            VerificationStatus = (EVerificationStatus)(int)providerDto.VerificationStatus,
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
            ProviderType = (EProviderType)(int)providerDto.Type,
            VerificationStatus = (EVerificationStatus)(int)providerDto.VerificationStatus,
            IsActive = !providerDto.IsDeleted
        };
    }
}
