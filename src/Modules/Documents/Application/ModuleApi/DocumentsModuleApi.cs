using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Documents;
using MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo Documents para comunicação entre módulos.
/// </summary>
[ModuleApi("Documents", "1.0")]
public sealed class DocumentsModuleApi(
    IQueryHandler<GetDocumentStatusQuery, DocumentDto?> getDocumentStatusHandler,
    IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>> getProviderDocumentsHandler,
    IServiceProvider serviceProvider,
    ILogger<DocumentsModuleApi> logger) : IDocumentsModuleApi
{
    public string ModuleName => "Documents";
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Documents module availability");

            // Verifica health checks registrados do sistema
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService != null)
            {
                var healthReport = await healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains("documents") || check.Tags.Contains("database"),
                    cancellationToken);

                if (healthReport.Status == HealthStatus.Unhealthy)
                {
                    logger.LogWarning("Documents module unavailable due to failed health checks: {FailedChecks}",
                        string.Join(", ", healthReport.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy).Select(e => e.Key)));
                    return false;
                }
            }

            // Testa funcionalidade básica
            var canExecuteBasicOperations = await CanExecuteBasicOperationsAsync(cancellationToken);
            if (!canExecuteBasicOperations)
            {
                logger.LogWarning("Documents module unavailable - basic operations test failed");
                return false;
            }

            logger.LogDebug("Documents module is available and healthy");
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Documents module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Documents module availability");
            return false;
        }
    }

    private async Task<bool> CanExecuteBasicOperationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Teste básico: tentar buscar por ID não existente
            var testId = Guid.NewGuid();
            var result = await GetDocumentByIdAsync(testId, cancellationToken);

            // Sucesso se retornar Success com null (documento não encontrado)
            return result.IsSuccess && result.Value == null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Basic operations test failed for Documents module");
            return false;
        }
    }

    public async Task<Result<ModuleDocumentDto?>> GetDocumentByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDocumentStatusQuery(documentId);
            var document = await getDocumentStatusHandler.HandleAsync(query, cancellationToken);

            return document == null
                ? Result<ModuleDocumentDto?>.Success(null)
                : Result<ModuleDocumentDto?>.Success(MapToModuleDto(document));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting document {DocumentId}", documentId);
            return Result<ModuleDocumentDto?>.Failure($"Failed to get document: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetProviderDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetProviderDocumentsQuery(providerId);
            var documents = await getProviderDocumentsHandler.HandleAsync(query, cancellationToken);

            var moduleDtos = documents.Select(MapToModuleDto).ToList();
            return Result<IReadOnlyList<ModuleDocumentDto>>.Success(moduleDtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting documents for provider {ProviderId}", providerId);
            return Result<IReadOnlyList<ModuleDocumentDto>>.Failure($"Failed to get provider documents: {ex.Message}");
        }
    }

    public async Task<Result<ModuleDocumentStatusDto?>> GetDocumentStatusAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDocumentStatusQuery(documentId);
            var document = await getDocumentStatusHandler.HandleAsync(query, cancellationToken);

            if (document == null)
            {
                return Result<ModuleDocumentStatusDto?>.Success(null);
            }

            var statusDto = new ModuleDocumentStatusDto
            {
                DocumentId = document.Id,
                Status = document.Status.ToString(),
                UpdatedAt = document.VerifiedAt ?? document.UploadedAt
            };

            return Result<ModuleDocumentStatusDto?>.Success(statusDto);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting document status {DocumentId}", documentId);
            return Result<ModuleDocumentStatusDto?>.Failure($"Failed to get document status: {ex.Message}");
        }
    }

    // Note: The following methods fetch all provider documents via GetProviderDocumentsAsync
    // and filter in-memory. For optimization, consider:
    // 1. Adding specialized queries for common checks (verified, pending, rejected)
    // 2. Implementing a caching layer if these methods are frequently called together
    // 3. Creating a helper method to reduce the repeated pattern of fetch-check-filter

    public async Task<Result<bool>> HasVerifiedDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsAsync(providerId, cancellationToken);
            
            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var hasVerified = documentsResult.Value!.Any(d => d.Status == EDocumentStatus.Verified.ToString());
            return Result<bool>.Success(hasVerified);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking verified documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure($"Failed to check verified documents: {ex.Message}");
        }
    }

    public async Task<Result<bool>> HasRequiredDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsAsync(providerId, cancellationToken);
            
            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var documents = documentsResult.Value!;

            // Documentos obrigatórios: IdentityDocument e ProofOfResidence
            var hasIdentity = documents.Any(d => d.DocumentType == EDocumentType.IdentityDocument.ToString());
            var hasProofOfResidence = documents.Any(d => d.DocumentType == EDocumentType.ProofOfResidence.ToString());

            var hasRequired = hasIdentity && hasProofOfResidence;
            return Result<bool>.Success(hasRequired);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking required documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure($"Failed to check required documents: {ex.Message}");
        }
    }

    public async Task<Result<DocumentStatusCountDto>> GetDocumentStatusCountAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsAsync(providerId, cancellationToken);
            
            if (documentsResult.IsFailure)
            {
                return Result<DocumentStatusCountDto>.Failure(documentsResult.Error);
            }

            var documents = documentsResult.Value!;

            var count = new DocumentStatusCountDto
            {
                Total = documents.Count,
                Pending = documents.Count(d => d.Status == EDocumentStatus.PendingVerification.ToString()),
                Verified = documents.Count(d => d.Status == EDocumentStatus.Verified.ToString()),
                Rejected = documents.Count(d => d.Status == EDocumentStatus.Rejected.ToString()),
                Uploading = documents.Count(d => d.Status == EDocumentStatus.Uploaded.ToString())
            };

            return Result<DocumentStatusCountDto>.Success(count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting document status count for provider {ProviderId}", providerId);
            return Result<DocumentStatusCountDto>.Failure($"Failed to get document status count: {ex.Message}");
        }
    }

    public async Task<Result<bool>> HasPendingDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsAsync(providerId, cancellationToken);
            
            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var hasPending = documentsResult.Value!.Any(d => d.Status == EDocumentStatus.PendingVerification.ToString());
            return Result<bool>.Success(hasPending);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking pending documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure($"Failed to check pending documents: {ex.Message}");
        }
    }

    public async Task<Result<bool>> HasRejectedDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsAsync(providerId, cancellationToken);
            
            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var hasRejected = documentsResult.Value!.Any(d => d.Status == EDocumentStatus.Rejected.ToString());
            return Result<bool>.Success(hasRejected);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking rejected documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure($"Failed to check rejected documents: {ex.Message}");
        }
    }

    /// <summary>
    /// Mapeia DocumentDto interno para ModuleDocumentDto público.
    /// </summary>
    private static ModuleDocumentDto MapToModuleDto(DocumentDto document)
    {
        return new ModuleDocumentDto
        {
            Id = document.Id,
            ProviderId = document.ProviderId,
            DocumentType = document.DocumentType.ToString(),
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            Status = document.Status.ToString(),
            UploadedAt = document.UploadedAt,
            VerifiedAt = document.VerifiedAt,
            RejectionReason = document.RejectionReason,
            OcrData = document.OcrData
        };
    }
}
