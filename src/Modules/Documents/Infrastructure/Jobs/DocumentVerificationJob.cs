using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Jobs;

/// <summary>
/// Job para processar verificação de documentos individuais.
/// Este job é enfileirado quando um documento é enviado.
/// NOTA: Document.FileUrl é usado como blob name (chave) para operações de storage.
/// </summary>
public class DocumentVerificationJob(
    IDocumentRepository documentRepository,
    IDocumentIntelligenceService documentIntelligenceService,
    IBlobStorageService blobStorageService,
    ILogger<DocumentVerificationJob> logger) : IDocumentVerificationService
{
    private readonly IDocumentRepository _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    private readonly IDocumentIntelligenceService _documentIntelligenceService = documentIntelligenceService ?? throw new ArgumentNullException(nameof(documentIntelligenceService));
    private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly ILogger<DocumentVerificationJob> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // TODO: Tornar configurável via appsettings.json quando necessário
    // Para MVP, mantendo valor fixo documentado
    private const float MinimumConfidence = 0.7f;

    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processamento do documento {DocumentId}", documentId);

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Documento {DocumentId} não encontrado", documentId);
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
            if (document.Status == EDocumentStatus.Uploaded)
            {
                document.MarkAsPendingVerification();
            }

            // Verifica se arquivo existe no blob storage
            var exists = await _blobStorageService.ExistsAsync(document.FileUrl, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Arquivo não encontrado no blob storage: {BlobName}", document.FileUrl);
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
            _logger.LogInformation("Executando OCR no documento {DocumentId}", documentId);
            var ocrResult = await _documentIntelligenceService.AnalyzeDocumentAsync(
                downloadUrl,
                document.DocumentType.ToString(),
                cancellationToken);

            if (ocrResult.Success && ocrResult.Confidence >= MinimumConfidence)
            {
                _logger.LogInformation(
                    "OCR bem-sucedido para documento {DocumentId} (Confiança: {Confidence:P0})",
                    documentId,
                    ocrResult.Confidence);

                document.MarkAsVerified(ocrResult.ExtractedData);
            }
            else
            {
                _logger.LogWarning(
                    "OCR falhou para documento {DocumentId}: {Error}",
                    documentId,
                    ocrResult.ErrorMessage ?? "Confiança baixa");

                document.MarkAsRejected(
                    ocrResult.ErrorMessage ?? $"Confiança baixa: {ocrResult.Confidence:P0}");
            }

            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Processamento do documento {DocumentId} concluído com status {Status}",
                documentId,
                document.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar documento {DocumentId}", documentId);

            // Detectar erros transitórios (network, timeout, OCR indisponível) vs permanentes
            var isTransient = IsTransientException(ex);

            if (isTransient)
            {
                // Para erros transitórios, apenas rethrow sem marcar como Failed
                // para permitir que Hangfire tente novamente
                _logger.LogWarning(
                    "Erro transitório ao processar documento {DocumentId}: {Message}. Hangfire tentará novamente.",
                    documentId,
                    ex.Message);
                throw;
            }

            // Para erros permanentes, marcar como Failed para evitar retries desnecessários
            _logger.LogError(
                "Erro permanente ao processar documento {DocumentId}: {Message}. Marcando como Failed.",
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
    /// Nota: Esta implementação MVP usa pattern matching em mensagens, que pode ser
    /// fragile para localização. Para hardening futuro, considere:
    /// - Checar tipos específicos de exceção do Azure (RequestFailedException com error codes)
    /// - Centralizar detecção de erros transitórios em biblioteca compartilhada de resiliência
    /// </summary>
    private static bool IsTransientException(Exception ex)
    {
        // Tipos de exceções transitórias comuns
        return ex is HttpRequestException
            || ex is TimeoutException
            || ex is OperationCanceledException
            || (ex.InnerException != null && IsTransientException(ex.InnerException))
            || ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase);
    }
}
