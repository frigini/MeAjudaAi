using System.Diagnostics;
using MeAjudaAi.Contracts.Functional;

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
public class ErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;

    public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
    {
        _logger = logger;
    }

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
            _logger.LogWarning("HandleApiError chamado para resultado bem-sucedido na operação: {Operation}", operation);
            return string.Empty;
        }

        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var errorMessage = result.Error?.Message ?? "Erro desconhecido";
        var statusCode = result.Error?.StatusCode ?? 500;

        _logger.LogError(
            "Erro na operação '{Operation}': {StatusCode} - {ErrorMessage} [CorrelationId: {CorrelationId}]",
            operation, statusCode, errorMessage, correlationId);

        // Retorna mensagem do backend (já deve ser amigável) ou mapeia por status code
        return GetUserFriendlyMessage(statusCode, errorMessage);
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
    /// Determina se deve tentar novamente baseado no código de status HTTP.
    /// </summary>
    /// <param name="statusCode">Código de status HTTP</param>
    /// <param name="attemptCount">Número de tentativas já realizadas</param>
    /// <returns>True se deve tentar novamente</returns>
    private bool ShouldRetry(int statusCode, int attemptCount)
    {
        // Só faz retry para erros transientes (5xx e 408) e se não excedeu número máximo de tentativas
        var isTransientError = statusCode >= 500 || statusCode == 408; // 408 = Request Timeout
        return isTransientError && attemptCount < 3;
    }

    /// <summary>
    /// Calcula delay para retry com exponential backoff.
    /// </summary>
    /// <param name="attemptCount">Número da tentativa</param>
    /// <returns>Delay em milissegundos</returns>
    private int GetRetryDelay(int attemptCount)
    {
        // Exponential backoff: 2^attempt * 1000ms
        // Attempt 0: 1 segundo
        // Attempt 1: 2 segundos
        // Attempt 2: 4 segundos
        return (int)Math.Pow(2, attemptCount) * 1000;
    }

    /// <summary>
    /// Executa operação com retry automático em caso de erros transientes.
    /// </summary>
    /// <typeparam name="T">Tipo de retorno</typeparam>
    /// <param name="apiCall">Função que executa a chamada à API</param>
    /// <param name="operation">Nome da operação (para logging)</param>
    /// <param name="maxAttempts">Número máximo de tentativas (padrão: 3)</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Result<T>>> apiCall,
        string operation,
        int maxAttempts = 3)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var result = await apiCall();

                if (result.IsSuccess)
                {
                    if (attempt > 0)
                    {
                        _logger.LogInformation(
                            "Operação '{Operation}' bem-sucedida após {Attempts} tentativas",
                            operation, attempt + 1);
                    }
                    return result;
                }

                var statusCode = result.Error?.StatusCode ?? 500;

                if (ShouldRetry(statusCode, attempt))
                {
                    var delay = GetRetryDelay(attempt);

                    _logger.LogWarning(
                        "Tentativa {Attempt} falhou para operação '{Operation}' com status {StatusCode}. Tentando novamente em {Delay}ms...",
                        attempt + 1, operation, statusCode, delay);

                    await Task.Delay(delay);
                    continue;
                }

                // Se não deve fazer retry, registra erro e retorna
                _logger.LogError(
                    "Operação '{Operation}' falhou após {Attempts} tentativas com status {StatusCode}: {ErrorMessage}",
                    operation, attempt + 1, statusCode, result.Error?.Message);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Exceção de rede na tentativa {Attempt} da operação '{Operation}'",
                    attempt + 1, operation);

                if (attempt < maxAttempts - 1)
                {
                    var delay = GetRetryDelay(attempt);
                    await Task.Delay(delay);
                    continue;
                }

                // Retorna erro de network após esgotar tentativas
                return Result<T>.Failure(Error.Internal("Erro de conexão. Verifique sua internet."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exceção inesperada na tentativa {Attempt} da operação '{Operation}'",
                    attempt + 1, operation);

                // Para exceções não transientes, não faz retry
                return Result<T>.Failure(Error.Internal($"Erro inesperado: {ex.Message}"));
            }
        }

        // Nunca deve chegar aqui, mas para segurança
        return Result<T>.Failure(Error.Internal("Operação falhou após todas as tentativas"));
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
            409 => "Conflito. O recurso já existe ou está em uso.",
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
