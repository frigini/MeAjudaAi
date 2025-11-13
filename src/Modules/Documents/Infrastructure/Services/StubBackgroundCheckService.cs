using MeAjudaAi.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Implementação STUB do serviço de verificação de antecedentes criminais.
/// 
/// Esta é uma implementação placeholder que retorna resultados simulados.
/// Para produção, deve ser substituída por integração com:
/// - Serasa Experian (APIs comerciais)
/// - APIs do Tribunal de Justiça
/// - CNJ (Conselho Nacional de Justiça)
/// - Outros provedores certificados
/// </summary>
public class StubBackgroundCheckService : IBackgroundCheckService
{
    private readonly ILogger<StubBackgroundCheckService> _logger;
    private readonly Dictionary<string, BackgroundCheckResult> _requests = new();

    public StubBackgroundCheckService(ILogger<StubBackgroundCheckService> logger)
    {
        _logger = logger;
    }

    public Task<BackgroundCheckResult> RequestCheckAsync(
        string cpf,
        string fullName,
        DateTime birthDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "STUB: Requisição de verificação de antecedentes para CPF {CPF}. " +
            "Esta é uma implementação simulada. Configure um provedor real para produção.",
            MaskCpf(cpf));

        var requestId = Guid.NewGuid().ToString();
        var result = new BackgroundCheckResult(
            RequestId: requestId,
            Status: BackgroundCheckStatus.Pending,
            HasCriminalRecord: null,
            Details: "Verificação iniciada (STUB - dados simulados)",
            CompletedAt: null,
            ErrorMessage: null);

        _requests[requestId] = result;

        return Task.FromResult(result);
    }

    public Task<BackgroundCheckResult> GetCheckStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "STUB: Consulta de status para requisição {RequestId}. " +
            "Esta é uma implementação simulada.",
            requestId);

        if (!_requests.TryGetValue(requestId, out var existingResult))
        {
            return Task.FromResult(new BackgroundCheckResult(
                RequestId: requestId,
                Status: BackgroundCheckStatus.NotAvailable,
                HasCriminalRecord: null,
                Details: null,
                CompletedAt: null,
                ErrorMessage: "Requisição não encontrada"));
        }

        // Simula processamento: marca como concluído após primeira consulta
        if (existingResult.Status == BackgroundCheckStatus.Pending)
        {
            var completedResult = existingResult with
            {
                Status = BackgroundCheckStatus.Completed,
                HasCriminalRecord = false, // Sempre retorna sem antecedentes no STUB
                Details = "Nenhum antecedente criminal encontrado (DADOS SIMULADOS - NÃO USAR EM PRODUÇÃO)",
                CompletedAt = DateTime.UtcNow
            };

            _requests[requestId] = completedResult;
            return Task.FromResult(completedResult);
        }

        return Task.FromResult(existingResult);
    }

    private static string MaskCpf(string cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
            return "***";

        return $"***{cpf[^3..]}";
    }
}
