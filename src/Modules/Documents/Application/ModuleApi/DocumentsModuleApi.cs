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
/// <remarks>
/// <para><strong>Contrato de Health Check:</strong></para>
/// <para>IsAvailableAsync depende de health checks com tags "documents" ou "database".
/// Se as tags dos health checks mudarem, o relatório de disponibilidade do módulo pode ser afetado.</para>
/// <para><strong>Semântica de "Not Found":</strong></para>
/// <para>GetDocumentByIdAsync retorna Success(null) para documentos inexistentes em vez de tratar
/// "not found" como falha. A verificação de disponibilidade depende desta convenção.</para>
/// <para><strong>Metadados do Módulo:</strong></para>
/// <para>Os valores do atributo ModuleApi devem corresponder às constantes ModuleNameConst e ApiVersionConst.
/// Um teste unitário valida esta consistência para prevenir deriva de configuração.</para>
/// </remarks>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class DocumentsModuleApi(
    IQueryHandler<GetDocumentStatusQuery, DocumentDto?> getDocumentStatusHandler,
    IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>> getProviderDocumentsHandler,
    IServiceProvider serviceProvider,
    ILogger<DocumentsModuleApi> logger) : IDocumentsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "Documents";
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

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
            // GetDocumentByIdAsync faz o log de erros
            // NOTA: Isto acopla a disponibilidade ao GetDocumentByIdAsync retornando Success(null) para not-found.
            // Considere introduzir uma query de health check leve (SELECT 1) para desacoplar da semântica da API de negócio.
            // PERF: Se verificações de disponibilidade se tornarem hot-path, substitua por query dedicada leve para evitar
            // executar todo o pipeline de documentos em cada teste.
            var testId = Guid.NewGuid();
            var result = await GetDocumentByIdAsync(testId, cancellationToken);

            // Sucesso se retornar Success com null (documento não encontrado)
            return result.IsSuccess && result.Value == null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // GetDocumentByIdAsync já fez o log do erro
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
            return Result<ModuleDocumentDto?>.Failure("DOCUMENTS_GET_FAILED");
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
            return Result<IReadOnlyList<ModuleDocumentDto>>.Failure("DOCUMENTS_PROVIDER_GET_FAILED");
        }
    }

    /// <summary>
    /// Gets the status of a document.
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document status DTO or null if not found</returns>
    /// <remarks>
    /// <para><strong>UpdatedAt Semantics:</strong></para>
    /// <para>Uses VerifiedAt ?? UploadedAt where VerifiedAt represents the timestamp of the last
    /// status change (verification or rejection). The domain model sets VerifiedAt when documents
    /// are verified OR rejected. For documents still in Uploaded/PendingVerification status,
    /// falls back to UploadedAt.</para>
    /// <para><strong>Note:</strong> RejectedAt is NOT used in the fallback chain because the domain
    /// already populates VerifiedAt for rejected documents, making VerifiedAt the authoritative
    /// timestamp for all terminal states (Verified/Rejected).</para>
    /// </remarks>
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
            return Result<ModuleDocumentStatusDto?>.Failure("DOCUMENTS_STATUS_GET_FAILED");
        }
    }

    // PERFORMANCE NOTE: The following methods fetch all provider documents and filter in-memory.
    //
    // CURRENT DATA MODEL CONSTRAINT:
    // EDocumentType has exactly 4 types (IdentityDocument, ProofOfResidence, CriminalRecord, Other).
    // The ix_documents_provider_type index on (ProviderId, DocumentType) suggests the design allows
    // at most one document per type per provider, capping each provider at ~4 documents maximum.
    // Therefore, in-memory filtering is highly efficient and optimization is NOT currently needed.
    //
    // TODO: Implement specialized queries for document status checks ONLY IF:
    // - EDocumentType enum is extended with additional types (>10 types)
    // - Data model changes to allow multiple documents per type (removing one-per-type assumption)
    // - Performance metrics show this as a bottleneck despite the 4-document cap
    //
    // Potential optimizations (deferred until model changes):
    // - HasVerifiedDocumentsQuery, HasPendingDocumentsQuery, HasRejectedDocumentsQuery
    // - GetDocumentStatusCountQuery (GroupBy + Count in database)
    // - HasRequiredDocumentsQuery (complex filtering with All())

    /// <summary>
    /// Helper method to get provider documents and handle common error propagation.
    /// </summary>
    private async Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetProviderDocumentsResultAsync(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var result = await GetProviderDocumentsAsync(providerId, cancellationToken);
        return result.IsFailure
            ? Result<IReadOnlyList<ModuleDocumentDto>>.Failure(result.Error)
            : result;
    }

    /// <summary>
    /// Helper method to get consistent string representation of document status.
    /// Centralizes enum-to-string conversion to reduce chances of mismatches.
    /// </summary>
    private static string StatusString(EDocumentStatus status) => status.ToString();

    /// <summary>
    /// Helper method to get consistent string representation of document type.
    /// Centralizes enum-to-string conversion to reduce chances of mismatches.
    /// </summary>
    private static string TypeString(EDocumentType type) => type.ToString();

    public async Task<Result<bool>> HasVerifiedDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsResultAsync(providerId, cancellationToken);

            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var hasVerified = documentsResult.Value!.Any(d => d.Status == StatusString(EDocumentStatus.Verified));
            return Result<bool>.Success(hasVerified);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking verified documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure("DOCUMENTS_VERIFIED_CHECK_FAILED");
        }
    }

    /// <summary>
    /// Verifica se o provedor possui todos os documentos obrigatórios VERIFICADOS.
    /// </summary>
    /// <param name="providerId">ID do provedor</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True se possui IdentityDocument e ProofOfResidence verificados; False caso contrário</returns>
    /// <remarks>
    /// Documentos obrigatórios:
    /// - IdentityDocument (RG, CNH, etc.)
    /// - ProofOfResidence (Comprovante de residência)
    /// 
    /// Este método verifica PRESENÇA + STATUS VERIFICADO. Documentos apenas enviados
    /// (Uploaded/PendingVerification) ou rejeitados não satisfazem o requisito.
    /// </remarks>
    public async Task<Result<bool>> HasRequiredDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsResultAsync(providerId, cancellationToken);

            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var documents = documentsResult.Value!;

            // Documentos obrigatórios: IdentityDocument e ProofOfResidence (ambos devem estar VERIFICADOS)
            // Com apenas 4 tipos de documento por provedor, single-pass lookup é mais eficiente e legível
            var verifiedTypes = documents
                .Where(d => d.Status == StatusString(EDocumentStatus.Verified))
                .Select(d => d.DocumentType)
                .ToHashSet();

            var hasRequired = verifiedTypes.Contains(TypeString(EDocumentType.IdentityDocument)) &&
                            verifiedTypes.Contains(TypeString(EDocumentType.ProofOfResidence));

            return Result<bool>.Success(hasRequired);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking required documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure("DOCUMENTS_REQUIRED_CHECK_FAILED");
        }
    }

    public async Task<Result<DocumentStatusCountDto>> GetDocumentStatusCountAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsResultAsync(providerId, cancellationToken);

            if (documentsResult.IsFailure)
            {
                return Result<DocumentStatusCountDto>.Failure(documentsResult.Error);
            }

            var documents = documentsResult.Value!;

            // Single-pass GroupBy para eficiência (embora com ≤4 documentos a diferença seja mínima)
            var statusGroups = documents
                .GroupBy(d => d.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var count = new DocumentStatusCountDto
            {
                Total = documents.Count,
                Pending = statusGroups.GetValueOrDefault(StatusString(EDocumentStatus.PendingVerification)),
                Verified = statusGroups.GetValueOrDefault(StatusString(EDocumentStatus.Verified)),
                Rejected = statusGroups.GetValueOrDefault(StatusString(EDocumentStatus.Rejected)),
                Uploading = statusGroups.GetValueOrDefault(StatusString(EDocumentStatus.Uploaded))
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
            return Result<DocumentStatusCountDto>.Failure("DOCUMENTS_STATUS_COUNT_FAILED");
        }
    }

    public async Task<Result<bool>> HasPendingDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsResultAsync(providerId, cancellationToken);

            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var hasPending = documentsResult.Value!.Any(d => d.Status == StatusString(EDocumentStatus.PendingVerification));
            return Result<bool>.Success(hasPending);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking pending documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure("DOCUMENTS_PENDING_CHECK_FAILED");
        }
    }

    public async Task<Result<bool>> HasRejectedDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentsResult = await GetProviderDocumentsResultAsync(providerId, cancellationToken);

            if (documentsResult.IsFailure)
            {
                return Result<bool>.Failure(documentsResult.Error);
            }

            var hasRejected = documentsResult.Value!.Any(d => d.Status == StatusString(EDocumentStatus.Rejected));
            return Result<bool>.Success(hasRejected);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking rejected documents for provider {ProviderId}", providerId);
            return Result<bool>.Failure("DOCUMENTS_REJECTED_CHECK_FAILED");
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
            DocumentType = TypeString(document.DocumentType),
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            Status = StatusString(document.Status),
            UploadedAt = document.UploadedAt,
            VerifiedAt = document.VerifiedAt,
            RejectionReason = document.RejectionReason,
            OcrData = document.OcrData
        };
    }
}
