using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Jobs;

/// <summary>
/// Job para processar verificação de documentos individuais.
/// Este job é enfileirado quando um documento é enviado.
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

    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processamento do documento {DocumentId}", documentId);

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Documento {DocumentId} não encontrado", documentId);
            return;
        }

        // Só processa documentos que estão Uploaded ou PendingVerification
        if (document.Status != EDocumentStatus.Uploaded && 
            document.Status != EDocumentStatus.PendingVerification)
        {
            _logger.LogInformation(
                "Documento {DocumentId} já foi processado (Status: {Status})",
                documentId,
                document.Status);
            return;
        }

        try
        {
            document.MarkAsPendingVerification();
            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            // Verifica se arquivo existe no blob storage
            var exists = await _blobStorageService.ExistsAsync(document.FileUrl, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Arquivo não encontrado no blob storage: {BlobName}", document.FileUrl);
                document.MarkAsFailed("Arquivo não encontrado no blob storage");
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

            if (ocrResult.Success && ocrResult.Confidence >= 0.7f)
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
            
            document.MarkAsFailed($"Erro durante processamento: {ex.Message}");
            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);
            
            throw; // Re-throw para permitir retry pelo sistema de jobs
        }
    }
}
