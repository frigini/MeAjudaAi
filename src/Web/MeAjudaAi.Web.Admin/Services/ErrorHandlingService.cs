using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Web.Admin.Services.Interfaces;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Informações sobre um erro incluindo correlation ID para rastreamento.
/// </summary>
/// <param name="Message">Mensagem de erro</param>
/// <param name="CorrelationId">ID de correlação para rastreamento</param>
/// <param name="StatusCode">Código de status HTTP</param>
public record ErrorInfo(string Message, string CorrelationId, int StatusCode = 400);

/// <summary>
/// Serviço para tratamento padronizado de erros com retry logic e mensagens amigáveis.
/// </summary>
public class ErrorHandlingService(
    ILogger<ErrorHandlingService> logger,
    ICorrelationIdProvider correlationIdProvider)
{

    /// <summary>
    /// Trata erro de API e retorna mensagem amigável.
    /// </summary>
    /// <param name="result">Resultado da API</param>
    /// <param name="operation">Nome da operação (para logging)</param>
    /// <returns>Mensagem amigável para o usuário</returns>
    public string HandleApiError<T>(Result<T> result, string operation)
    {
        if (result.IsSuccess)
        {
            logger.LogWarning("HandleApiError called for successful result in operation: {Operation}", operation);
            return string.Empty;
        }

        var correlationId = correlationIdProvider.GetOrCreate();
        var backendMessage = result.Error?.Message;
        var statusCode = result.Error?.StatusCode ?? 500;

        logger.LogError(
            "API operation failed: Operation={Operation}, StatusCode={StatusCode}, CorrelationId={CorrelationId}",
            operation, statusCode, correlationId);

        // Passa nullable backendMessage para permitir mapeamento por status code quando backend não retorna mensagem
        return GetUserFriendlyMessage(statusCode, backendMessage);
    }

    /// <summary>
    /// Obtém mensagem amigável baseada no código de status HTTP.
    /// </summary>
    /// <param name="statusCode">Código de status HTTP</param>
    /// <param name="backendMessage">Mensagem do backend (prioritária)</param>
    /// <returns>Mensagem amigável</returns>
    public string GetUserFriendlyMessage(int statusCode, string? backendMessage = null)
    {
        // Usa mensagem do backend se disponível (já deve ser amigável)
        if (!string.IsNullOrWhiteSpace(backendMessage))
        {
            return backendMessage;
        }

        // Caso contrário, mapeia por status code
        return GetMessageFromHttpStatus(statusCode);
    }

    /// <summary>
    /// Executa operação de API com tratamento padronizado de erros e correlation tracking.
    /// Retry logic é tratado pelo Polly no HttpClient (3 tentativas com exponential backoff).
    /// Este método foca em error mapping e logging com correlation IDs.
    /// </summary>
    /// <typeparam name="T">Tipo de retorno</typeparam>
    /// <param name="apiCall">Função que executa a chamada à API</param>
    /// <param name="operation">Nome da operação (para logging)</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Resultado da operação</returns>
    /// <remarks>
    /// ARQUITETURA DE RESILIÊNCIA:
    /// - Retry Logic: Tratado pelo Polly no HttpClient (PollyPolicies.GetCombinedPolicy)
    ///   * 3 tentativas com exponential backoff: 2s → 4s → 8s
    ///   * Apenas para erros transientes (5xx, 408 timeout, network errors)
    ///   * Circuit breaker: abre após 5 falhas consecutivas
    /// - Error Mapping: Convertido neste método (HTTP status → mensagens amigáveis)
    /// - Correlation Tracking: Activity.Current.Id para rastreamento end-to-end
    /// - Cancellation Support: Verifica CancellationToken antes de executar
    /// 
    /// EXEMPLOS:
    /// - LoadProviders (GET): await ExecuteWithErrorHandlingAsync(() => api.Get(...), "carregar provedores", ct)
    /// - CreateProvider (POST): await ExecuteWithErrorHandlingAsync(() => api.Post(...), "criar provedor", ct)
    /// - UpdateProvider (PUT): await ExecuteWithErrorHandlingAsync(() => api.Put(...), "atualizar provedor", ct)
    /// </remarks>
    public async Task<Result<T>> ExecuteWithErrorHandlingAsync<T>(
        Func<CancellationToken, Task<Result<T>>> apiCall,
        string operation,
        CancellationToken cancellationToken = default)
    {
        var correlationId = correlationIdProvider.GetOrCreate();

        try
        {
            // Check cancellation before executing
            cancellationToken.ThrowIfCancellationRequested();

            var result = await apiCall(cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Operation '{Operation}' succeeded [CorrelationId: {CorrelationId}]",
                    operation, correlationId);
                return result;
            }

            var statusCode = result.Error?.StatusCode ?? 500;
            var backendMessage = result.Error?.Message;

            logger.LogError(
                "Operation '{Operation}' failed: StatusCode={StatusCode}, CorrelationId={CorrelationId}",
                operation, statusCode, correlationId);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation(
                "Operation '{Operation}' canceled [CorrelationId: {CorrelationId}]",
                operation, correlationId);
            
            throw; // Re-throw to let caller handle cancellation
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex,
                "Network exception in operation '{Operation}' [CorrelationId: {CorrelationId}]",
                operation, correlationId);

            return Result<T>.Failure(Error.Internal("Erro de conexão. Verifique sua internet."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected exception in operation '{Operation}' [CorrelationId: {CorrelationId}]",
                operation, correlationId);

            return Result<T>.Failure(Error.Internal("Ocorreu um erro inesperado. Tente novamente."));
        }
    }

    /// <summary>
    /// Mapeia código de status HTTP para mensagem amigável em português.
    /// </summary>
    /// <param name="statusCode">Código de status HTTP</param>
    /// <returns>Mensagem amigável</returns>
    private string GetMessageFromHttpStatus(int statusCode)
    {
        return statusCode switch
        {
            400 => "Requisição inválida. Verifique os dados fornecidos.",
            401 => "Você não está autenticado. Faça login novamente.",
            403 => "Você não tem permissão para realizar esta ação.",
            404 => "Recurso não encontrado.",
            408 => "A requisição demorou muito. Tente novamente.",
            409 => "Conflito. O recurso já existe ou foi modificado.",
            422 => "Dados inválidos. Verifique as informações fornecidas.",
            429 => "Muitas requisições. Aguarde um momento e tente novamente.",
            500 => "Erro interno do servidor. Nossa equipe foi notificada.",
            502 => "Servidor temporariamente indisponível. Tente novamente.",
            503 => "Serviço temporariamente indisponível. Tente novamente mais tarde.",
            504 => "O servidor não respondeu a tempo. Tente novamente.",
            _ => statusCode >= 500
                ? "Erro no servidor. Tente novamente mais tarde."
                : "Ocorreu um erro inesperado. Tente novamente."
        };
    }
}
