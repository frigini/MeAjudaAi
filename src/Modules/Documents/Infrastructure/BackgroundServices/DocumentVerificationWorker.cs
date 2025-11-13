using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.BackgroundServices;

/// <summary>
/// Background worker que processa documentos enviados, executando OCR e verificações
/// </summary>
public class DocumentVerificationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentVerificationWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public DocumentVerificationWorker(
        IServiceProvider serviceProvider,
        ILogger<DocumentVerificationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DocumentVerificationWorker iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingDocumentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar documentos pendentes");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("DocumentVerificationWorker finalizado");
    }

    private async Task ProcessPendingDocumentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var documentIntelligenceService = scope.ServiceProvider.GetRequiredService<IDocumentIntelligenceService>();
        var blobStorageService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();

        // Busca documentos que foram enviados mas ainda não processados
        var pendingDocuments = await dbContext.Documents
            .Where(d => d.Status == DocumentStatus.Uploaded)
            .OrderBy(d => d.UploadedAt)
            .Take(10) // Processa até 10 de cada vez
            .ToListAsync(cancellationToken);

        if (!pendingDocuments.Any())
        {
            _logger.LogDebug("Nenhum documento pendente para processar");
            return;
        }

        _logger.LogInformation("Processando {Count} documentos pendentes", pendingDocuments.Count);

        foreach (var document in pendingDocuments)
        {
            await ProcessDocumentAsync(document, documentIntelligenceService, blobStorageService, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessDocumentAsync(
        Document document,
        IDocumentIntelligenceService documentIntelligenceService,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processando documento {DocumentId} do provedor {ProviderId}",
                document.Id, document.ProviderId);

            document.MarkAsPendingVerification();

            // Verifica se o blob existe
            var exists = await blobStorageService.ExistsAsync(document.FileUrl, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Blob {BlobName} não encontrado para documento {DocumentId}",
                    document.FileUrl, document.Id);
                document.MarkAsFailed("Arquivo não encontrado no storage");
                return;
            }

            // Gera URL temporária para o Document Intelligence processar
            var (downloadUrl, _) = await blobStorageService.GenerateDownloadUrlAsync(
                document.FileUrl,
                cancellationToken);

            // Executa OCR
            var ocrResult = await documentIntelligenceService.AnalyzeDocumentAsync(
                downloadUrl,
                document.DocumentType.ToString(),
                cancellationToken);

            if (ocrResult.Success)
            {
                // Validações básicas
                if (ocrResult.Confidence < 0.7f)
                {
                    document.MarkAsRejected($"Confiança do OCR muito baixa: {ocrResult.Confidence:P}");
                    _logger.LogWarning("Documento {DocumentId} rejeitado por baixa confiança OCR", document.Id);
                }
                else
                {
                    document.MarkAsVerified(ocrResult.ExtractedData);
                    _logger.LogInformation("Documento {DocumentId} verificado com sucesso", document.Id);
                }
            }
            else
            {
                document.MarkAsFailed($"Falha no OCR: {ocrResult.ErrorMessage}");
                _logger.LogError("Falha no OCR para documento {DocumentId}: {Error}",
                    document.Id, ocrResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar documento {DocumentId}", document.Id);
            document.MarkAsFailed($"Erro inesperado: {ex.Message}");
        }
    }
}
