using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Jobs;

/// <summary>
/// Job para processar verificação de documentos individuais.
/// Este job é enfileirado quando um documento é enviado.
/// NOTA: Document.FileUrl é usado como blob name (chave) para operações de storage.
/// </summary>
public class DocumentVerificationJob : IDocumentVerificationService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentIntelligenceService _documentIntelligenceService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentVerificationJob> _logger;
    private readonly float _minimumConfidence;

    public DocumentVerificationJob(
        IDocumentRepository documentRepository,
        IDocumentIntelligenceService documentIntelligenceService,
        IBlobStorageService blobStorageService,
        IConfiguration configuration,
        ILogger<DocumentVerificationJob> logger)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _documentIntelligenceService = documentIntelligenceService ?? throw new ArgumentNullException(nameof(documentIntelligenceService));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configuração com fallback para valor padrão (após validações para evitar NullReferenceException em testes)
        _minimumConfidence = configuration?.GetValue<float>("Documents:Verification:MinimumConfidence", 0.7f) ?? 0.7f;
    }

    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting document processing for {DocumentId}", documentId);

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found", documentId);
            return;
        }

        // Só processa documentos que estão Uploaded, PendingVerification ou Failed (para retry)
        if (document.Status != EDocumentStatus.Uploaded &&
            document.Status != EDocumentStatus.PendingVerification &&
            document.Status != EDocumentStatus.Failed)
        {
            _logger.LogInformation(
                "Documento {DocumentId} já foi processado (Status: {Status})",
                documentId,
                document.Status);
            return;
        }

        try
        {
            // Marca como PendingVerification apenas se ainda não estiver
            if (document.Status == EDocumentStatus.Uploaded || document.Status == EDocumentStatus.Failed)
            {
                document.MarkAsPendingVerification();
            }

            // Verifica se arquivo existe no blob storage
            var exists = await _blobStorageService.ExistsAsync(document.FileUrl, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("File not found in blob storage: {BlobName}", document.FileUrl);
                document.MarkAsFailed("Arquivo não encontrado no blob storage");

                // Salva status final (Failed) em uma única operação
                await _documentRepository.UpdateAsync(document, cancellationToken);
                await _documentRepository.SaveChangesAsync(cancellationToken);
                return;
            }

            // Gera URL de download com SAS token
            var (downloadUrl, _) = await _blobStorageService.GenerateDownloadUrlAsync(
                document.FileUrl,
                cancellationToken);

            // Executa OCR no documento
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

            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} processing completed with status {Status}",
                documentId,
                document.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocumentId}", documentId);

            // Detectar erros transitórios (network, timeout, OCR indisponível) vs permanentes
            var isTransient = IsTransientException(ex);

            if (isTransient)
            {
                // Para erros transitórios, apenas rethrow sem marcar como Failed
                // para permitir que Hangfire tente novamente
                _logger.LogWarning(
                    "Transient error processing document {DocumentId}: {Message}. Hangfire will retry.",
                    documentId,
                    ex.Message);
                throw;
            }

            // Para erros permanentes, marcar como Failed para evitar retries desnecessários
            _logger.LogError(
                "Permanent error processing document {DocumentId}: {Message}. Marking as Failed.",
                documentId,
                ex.Message);
            document.MarkAsFailed($"Erro durante processamento: {ex.Message}");
            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            // Não rethrow para erros permanentes - job concluído com falha documentada
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
