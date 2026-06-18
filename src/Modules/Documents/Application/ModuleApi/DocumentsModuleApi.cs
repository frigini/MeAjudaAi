using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Documents;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Mappers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

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
/// <para>Os valores do atributo ModuleApi devem corresponder às propriedades ModuleMetadata.Name e ModuleMetadata.Version.
/// Esta consistência é garantida pela classe aninhada ModuleMetadata que centraliza as constantes.</para>
/// </remarks>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class DocumentsModuleApi(
    IQueryHandler<GetDocumentByIdQuery, DocumentDto?> getDocumentByIdHandler,
    IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>> getProviderDocumentsHandler,
    IDocumentQueries documentQueries,
    IServiceProvider serviceProvider,
    ILogger<DocumentsModuleApi> logger) : IDocumentsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = ModuleNames.Documents;
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Documents module availability");

            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService != null)
            {
                var healthReport = await healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains("documents") || check.Tags.Contains("database"),
                    cancellationToken);

                if (healthReport.Status == HealthStatus.Unhealthy)
                {
                    logger.LogWarning("Documents module unavailable due to health check failures: {FailedChecks}",
                        string.Join(", ", healthReport.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy).Select(e => e.Key)));
                    return false;
                }
            }

            var canExecuteBasicOperations = await CanExecuteBasicOperationsAsync(cancellationToken);
            if (!canExecuteBasicOperations)
            {
                logger.LogWarning("Documents module unavailable - basic operations test failed");
                return false;
            }

            logger.LogDebug("Documents module is available and healthy");
            return true;
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug("Documents module availability check canceled");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Documents module availability check failed with InvalidOperationException");
            return false;
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Documents module availability check timed out");
            return false;
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error checking Documents module availability");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error checking Documents module availability");
            return false;
        }
    }

    private async Task<bool> CanExecuteBasicOperationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await documentQueries.CanConnectAsync(cancellationToken);
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<Result<ModuleDocumentDto?>> GetDocumentByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDocumentByIdQuery(documentId);
            var document = await getDocumentByIdHandler.HandleAsync(query, cancellationToken);

            return document == null
                ? Result<ModuleDocumentDto?>.Success(null)
                : Result<ModuleDocumentDto?>.Success(document.ToModuleDto());
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error retrieving document {DocumentId}", documentId);
            return Result<ModuleDocumentDto?>.Failure("DOCUMENTS_GET_FAILED");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
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

            var moduleDtos = documents.Select(d => d.ToModuleDto()).ToList();
            return Result<IReadOnlyList<ModuleDocumentDto>>.Success(moduleDtos);
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error retrieving documents for provider {ProviderId}", providerId);
            return Result<IReadOnlyList<ModuleDocumentDto>>.Failure("DOCUMENTS_PROVIDER_GET_FAILED");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error retrieving documents for provider {ProviderId}", providerId);
            return Result<IReadOnlyList<ModuleDocumentDto>>.Failure("DOCUMENTS_PROVIDER_GET_FAILED");
        }
    }

    /// <summary>
    /// Obtém o status de um documento.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO de status do documento ou null se não encontrado</returns>
    /// <remarks>
    /// <para><strong>Semântica de UpdatedAt:</strong></para>
    /// <para>Usa VerifiedAt ?? UploadedAt, onde VerifiedAt representa o timestamp da última mudança
    /// de status (verificação ou rejeição). O modelo de domínio define VerifiedAt quando documentos
    /// são verificados OU rejeitados. Para documentos ainda em Uploaded/PendingVerification,
    /// usa UploadedAt como fallback.</para>
    /// <para><strong>Nota:</strong> RejectedAt NÃO é usado na cadeia de fallback porque o domínio
    /// já popula VerifiedAt para documentos rejeitados, tornando VerifiedAt o timestamp
    /// autoritativo para todos os estados terminais (Verified/Rejected).</para>
    /// </remarks>
    public async Task<Result<ModuleDocumentStatusDto?>> GetDocumentStatusAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDocumentByIdQuery(documentId);
            var document = await getDocumentByIdHandler.HandleAsync(query, cancellationToken);

            if (document == null)
            {
                return Result<ModuleDocumentStatusDto?>.Success(null);
            }

            var statusDto = new ModuleDocumentStatusDto(
                document.Id,
                document.Status.ToString(),
                document.VerifiedAt ?? document.UploadedAt
            );

            return Result<ModuleDocumentStatusDto?>.Success(statusDto);
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error retrieving document status {DocumentId}", documentId);
            return Result<ModuleDocumentStatusDto?>.Failure("DOCUMENTS_STATUS_GET_FAILED");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error retrieving document status {DocumentId}", documentId);
            return Result<ModuleDocumentStatusDto?>.Failure("DOCUMENTS_STATUS_GET_FAILED");
        }
    }

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

    public async Task<Result<bool>> HasVerifiedDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetProviderDocumentsResultAsync(providerId, cancellationToken);
        if (result.IsFailure)
            return Result<bool>.Failure(result.Error);

        var hasVerified = result.Value!.Any(d => d.Status == EDocumentStatus.Verified.ToDescription());
        return Result<bool>.Success(hasVerified);
    }

    public async Task<Result<bool>> HasRequiredDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetProviderDocumentsResultAsync(providerId, cancellationToken);
        if (result.IsFailure)
            return Result<bool>.Failure(result.Error);

        var verifiedDocs = result.Value!
            .Where(d => d.Status == EDocumentStatus.Verified.ToDescription())
            .Select(d => d.DocumentType)
            .ToHashSet();

        var hasIdentity = verifiedDocs.Contains(EDocumentType.IdentityDocument.ToDescription());
        var hasProofOfResidence = verifiedDocs.Contains(EDocumentType.ProofOfResidence.ToDescription());

        return Result<bool>.Success(hasIdentity && hasProofOfResidence);
    }

    public async Task<Result<DocumentStatusCountDto>> GetDocumentStatusCountAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetProviderDocumentsResultAsync(providerId, cancellationToken);
        if (result.IsFailure)
            return Result<DocumentStatusCountDto>.Failure(result.Error);

        var documents = result.Value;
        var countDto = new DocumentStatusCountDto(
            Total: documents.Count,
            Verified: documents.Count(d => d.Status == EDocumentStatus.Verified.ToString()),
            Pending: documents.Count(d => d.Status == EDocumentStatus.PendingVerification.ToString()),
            Rejected: documents.Count(d => d.Status == EDocumentStatus.Rejected.ToString()),
            Uploading: documents.Count(d => d.Status == EDocumentStatus.Uploaded.ToString()));

        return Result<DocumentStatusCountDto>.Success(countDto);
    }

    public async Task<Result<bool>> HasPendingDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetProviderDocumentsResultAsync(providerId, cancellationToken);
        if (result.IsFailure)
            return Result<bool>.Failure(result.Error);

        var hasPending = result.Value!.Any(d => d.Status == EDocumentStatus.PendingVerification.ToString());
        return Result<bool>.Success(hasPending);
    }

    public async Task<Result<bool>> HasRejectedDocumentsAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetProviderDocumentsResultAsync(providerId, cancellationToken);
        if (result.IsFailure)
            return Result<bool>.Failure(result.Error);

        var hasRejected = result.Value!.Any(d => d.Status == EDocumentStatus.Rejected.ToString());
        return Result<bool>.Success(hasRejected);
    }

    public async Task<Result<bool>> DeleteDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDocumentByIdQuery(documentId);
            var document = await getDocumentByIdHandler.HandleAsync(query, cancellationToken);

            if (document == null)
                return Result<bool>.Failure(Error.NotFound("Documento não encontrado"));

            var uow = serviceProvider.GetRequiredService<Shared.Database.Abstractions.IUnitOfWork>();
            var repository = uow.GetRepository<Modules.Documents.Domain.Entities.Document, Guid>();
            var entity = await repository.TryFindAsync(documentId, cancellationToken);

            if (entity is null)
                return Result<bool>.Failure(Error.NotFound("Documento não encontrado"));

            repository.Delete(entity);
            await uow.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error deleting document {DocumentId}", documentId);
            return Result<bool>.Failure("DOCUMENTS_DELETE_FAILED");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return Result<bool>.Failure("DOCUMENTS_DELETE_FAILED");
        }
    }
}
