using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Jobs;

public class DocumentVerificationJob : IDocumentVerificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IDocumentIntelligenceService _documentIntelligenceService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentVerificationJob> _logger;
    private readonly float _minimumConfidence;

    public DocumentVerificationJob(
        IUnitOfWork uow,
        IDocumentIntelligenceService documentIntelligenceService,
        IBlobStorageService blobStorageService,
        IConfiguration configuration,
        ILogger<DocumentVerificationJob> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _documentIntelligenceService = documentIntelligenceService ?? throw new ArgumentNullException(nameof(documentIntelligenceService));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _minimumConfidence = configuration?.GetValue<float>("Documents:Verification:MinimumConfidence", 0.7f) ?? 0.7f;
    }

    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting document processing for {DocumentId}", documentId);

        try
        {
            var repository = _uow.GetRepository<Document, DocumentId>();
            var document = await repository.TryFindAsync(new DocumentId(documentId), cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", documentId);
                return;
            }
            _logger.LogInformation("Document {DocumentId} found, current status: {Status}", documentId, document.Status);

            if (document.Status != EDocumentStatus.Uploaded &&
                document.Status != EDocumentStatus.PendingVerification &&
                document.Status != EDocumentStatus.Failed)
            {
                _logger.LogInformation(
                    "Documento {DocumentId} já foi processado (Status: {Status})",
                    documentId, document.Status);
                return;
            }

            if (document.Status == EDocumentStatus.Uploaded || document.Status == EDocumentStatus.Failed)
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

            var (downloadUrl, _) = await _blobStorageService.GenerateDownloadUrlAsync(
                document.FileUrl,
                cancellationToken);

            _logger.LogInformation("Executing OCR on document {DocumentId}", documentId);
            var ocrResult = await _documentIntelligenceService.AnalyzeDocumentAsync(
                downloadUrl,
                document.DocumentType.ToString(),
                cancellationToken);

            if (ocrResult.Success && ocrResult.Confidence >= _minimumConfidence)
            {
                _logger.LogInformation(
                    "OCR successful for document {DocumentId} (Confidence: {Confidence:P0})",
                    documentId,
                    ocrResult.Confidence);

                document.MarkAsVerified(ocrResult.ExtractedData);
            }
            else
            {
                _logger.LogWarning(
                    "OCR failed for document {DocumentId}: {Error}",
                    documentId,
                    ocrResult.ErrorMessage ?? "Low confidence");

                document.MarkAsRejected(
                    ocrResult.ErrorMessage ?? $"Low confidence: {ocrResult.Confidence:P0}");
            }

            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} processing completed with status {Status}",
                documentId,
                document.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocumentId}", documentId);

            var isTransient = IsTransientException(ex);

            if (isTransient)
            {
                _logger.LogWarning(
                    "Transient error processing document {DocumentId}: {Message}. Hangfire will retry.",
                    documentId,
                    ex.Message);
                throw;
            }

            // Note: document might be null if TryFindAsync fails, handle carefully
            // In case of non-transient error, document may have been fetched but not updated
            // Re-fetch if necessary or track status in memory.
            _logger.LogError(
                "Permanent error processing document {DocumentId}: {Message}. Marking as Failed.",
                documentId,
                ex.Message);
            
            // Re-fetch to ensure we have the tracked entity if it was fetched earlier
            var repository = _uow.GetRepository<Document, DocumentId>();
            var document = await repository.TryFindAsync(new DocumentId(documentId), cancellationToken);
            if (document != null)
            {
                document.MarkAsFailed($"Erro durante processamento: {ex.Message}");
                await _uow.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Detecta se uma exceção é transitória (rede, timeout, serviço indisponível)
    /// ou permanente (formato inválido, validação falhou).
    /// 
    /// Implementação robusta que verifica:
    /// - Tipos específicos de exceção do Azure (RequestFailedException com HTTP status transitórios)
    /// - Tipos comuns de exceções transitórias (HttpRequestException, TimeoutException)
    /// - Pattern matching em mensagens como fallback para casos não cobertos
    /// </summary>
    private static bool IsTransientException(Exception ex, int depth = 0)
    {
        const int MaxDepth = 10;
        if (depth > MaxDepth) return false;

        // 1. Exceções do Azure SDK - verificar HTTP status codes transitórios
        if (ex is Azure.RequestFailedException requestFailed)
        {
            var status = requestFailed.Status;
            // Status codes transitórios que devem ser retentados:
            // 408 Request Timeout
            // 429 Too Many Requests (rate limiting)
            // 500 Internal Server Error
            // 502 Bad Gateway
            // 503 Service Unavailable
            // 504 Gateway Timeout
            return status == 408 || status == 429 || status == 500 
                || status == 502 || status == 503 || status == 504;
        }

        // 2. Tipos de exceções transitórias comuns do .NET
        if (ex is HttpRequestException || ex is TimeoutException)
        {
            return true;
        }

        // Only treat OperationCanceledException as transient if not explicitly cancelled
        if (ex is OperationCanceledException oce && !oce.CancellationToken.IsCancellationRequested)
        {
            return true;
        }

        // 3. Verificação recursiva de InnerException com proteção contra loops infinitos
        if (ex.InnerException != null && IsTransientException(ex.InnerException, depth + 1))
        {
            return true;
        }

        // 4. Fallback: pattern matching em mensagens (menos confiável, mas cobre casos não mapeados)
        var message = ex.Message;
        return message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || message.Contains("network", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unavailable", StringComparison.OrdinalIgnoreCase)
            || message.Contains("connection", StringComparison.OrdinalIgnoreCase);
    }
}
