using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Jobs;

public class DocumentVerificationJob(
    [FromKeyedServices(ModuleKeys.Documents)] IUnitOfWork uow,
    IDocumentQueries documentQueries,
    IDocumentIntelligenceService documentIntelligenceService,
    IBlobStorageService blobStorageService,
    IConfiguration configuration,
    ILogger<DocumentVerificationJob> logger) : IDocumentVerificationService
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly IDocumentIntelligenceService _documentIntelligenceService = documentIntelligenceService ?? throw new ArgumentNullException(nameof(documentIntelligenceService));
    private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly ILogger<DocumentVerificationJob> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly float _minimumConfidence = configuration?.GetValue<float>("Documents:Verification:MinimumConfidence", 0.7f) ?? 0.7f;

    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting document processing for {DocumentId}", documentId);

        var document = await _documentQueries.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found", documentId);
            return;
        }

        if (document.Status is not EDocumentStatus.Uploaded and
            not EDocumentStatus.PendingVerification and
            not EDocumentStatus.Failed)
        {
            _logger.LogInformation("Document {DocumentId} has already been processed (Status: {Status})", documentId, document.Status);
            return;
        }

        try
        {
            if (document.Status is EDocumentStatus.Uploaded or EDocumentStatus.Failed)
            {
                document.MarkAsPendingVerification();
            }

            var exists = await _blobStorageService.ExistsAsync(document.FileUrl, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("File not found in blob storage: {BlobName}", document.FileUrl);
                document.MarkAsFailed("Arquivo não encontrado no blob storage");
                await _uow.SaveChangesAsync(cancellationToken);
                return;
            }

            var (downloadUrl, _) = await _blobStorageService.GenerateDownloadUrlAsync(document.FileUrl, cancellationToken);

            _logger.LogInformation("Executing OCR on document {DocumentId}", documentId);
            var ocrResult = await _documentIntelligenceService.AnalyzeDocumentAsync(downloadUrl, document.DocumentType.ToString(), cancellationToken);

            if (ocrResult.Success && ocrResult.Confidence >= _minimumConfidence)
            {
                _logger.LogInformation("OCR successful for document {DocumentId} (Confidence: {Confidence:P0})", documentId, ocrResult.Confidence);
                document.MarkAsVerified(ocrResult.ExtractedData);
            }
            else
            {
                _logger.LogWarning("OCR failed for document {DocumentId}: {Error}", documentId, ocrResult.ErrorMessage ?? "Low confidence");
                document.MarkAsRejected(ocrResult.ErrorMessage ?? $"Low confidence: {ocrResult.Confidence:P0}");
            }

            await _uow.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Document {DocumentId} processing completed with status {Status}", documentId, document.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocumentId}", documentId);
            if (IsTransientException(ex)) throw;

            document.MarkAsFailed($"Erro durante processamento: {ex.Message}");
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool IsTransientException(Exception ex, int depth = 0)
    {
        const int MaxDepth = 10;
        if (depth > MaxDepth) return false;
        if (ex is Azure.RequestFailedException rfe) return rfe.Status is 408 or 429 or >= 500;
        if (ex is HttpRequestException or TimeoutException) return true;
        if (ex is OperationCanceledException) return true;
        if (ex.InnerException != null && IsTransientException(ex.InnerException, depth + 1)) return true;
        return ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase);
    }
}
