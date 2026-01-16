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
    /// Determina se deve tentar novamente baseado no código de status HTTP e método HTTP.
    /// </summary>
    /// <param name="statusCode">Código de status HTTP</param>
    /// <param name="attemptCount">Número de tentativas já realizadas</param>
    /// <param name="httpMethod">Método HTTP da requisição</param>
    /// <param name="allowRetryForNonIdempotent">Se true, permite retry para métodos não-idempotentes (POST/PUT/DELETE)</param>
    /// <returns>True se deve tentar novamente</returns>
    private bool ShouldRetry(int statusCode, int attemptCount, HttpMethod httpMethod, bool allowRetryForNonIdempotent)
    {
        // Nunca faz retry para conflitos (409) - indica recurso já existe ou foi modificado
        if (statusCode == 409)
        {
            return false;
        }

        // Só faz retry para erros transientes (5xx e 408 timeout)
        var isTransientError = statusCode >= 500 || statusCode == 408;
        
        if (!isTransientError || attemptCount >= 3)
        {
            return false;
        }

        // Verifica se método HTTP é seguro para retry (idempotente)
        if (allowRetryForNonIdempotent)
        {
            // Retry explicitamente permitido para métodos não-idempotentes
            return true;
        }

        // Apenas métodos idempotentes podem fazer retry por padrão (seguro contra escritas duplicadas)
        return httpMethod == HttpMethod.Get || 
               httpMethod == HttpMethod.Head || 
               httpMethod == HttpMethod.Options;
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
    /// IMPORTANTE: Por padrão, apenas métodos idempotentes (GET, HEAD, OPTIONS) fazem retry para evitar escritas duplicadas.
    /// Para métodos não-idempotentes (POST, PUT, DELETE), defina allowRetryForNonIdempotent = true explicitamente.
    /// </summary>
    /// <typeparam name="T">Tipo de retorno</typeparam>
    /// <param name="apiCall">Função que executa a chamada à API</param>
    /// <param name="operation">Nome da operação (para logging)</param>
    /// <param name="httpMethod">Método HTTP da requisição (GET, POST, PUT, DELETE, etc.)</param>
    /// <param name="maxAttempts">Número máximo de tentativas (padrão: 3)</param>
    /// <param name="allowRetryForNonIdempotent">Se true, permite retry para POST/PUT/DELETE (use com cautela!)</param>
    /// <returns>Resultado da operação</returns>
    /// <remarks>
    /// POLÍTICA DE RETRY:
    /// - GET, HEAD, OPTIONS: Retry automático em erros 5xx e 408 (até 3 tentativas)
    /// - POST, PUT, DELETE: NÃO faz retry por padrão (previne escritas duplicadas)
    /// - HTTP 409 Conflict: NUNCA faz retry (recurso já existe ou foi modificado)
    /// - Exponential backoff: 1s → 2s → 4s entre tentativas
    /// 
    /// EXEMPLOS:
    /// - LoadProviders (GET): ExecuteWithRetryAsync(..., HttpMethod.Get) ✅ Safe retry
    /// - CreateProvider (POST): ExecuteWithRetryAsync(..., HttpMethod.Post) ❌ No retry by default
    /// - DeleteProvider (DELETE): ExecuteWithRetryAsync(..., HttpMethod.Delete, allowRetryForNonIdempotent: false) ❌ No retry
    /// </remarks>
    public async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Result<T>>> apiCall,
        string operation,
        HttpMethod httpMethod,
        int maxAttempts = 3,
        bool allowRetryForNonIdempotent = false)
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
                            "Operação '{Operation}' ({HttpMethod}) bem-sucedida após {Attempts} tentativas",
                            operation, httpMethod.Method, attempt + 1);
                    }
                    return result;
                }

                var statusCode = result.Error?.StatusCode ?? 500;

                if (ShouldRetry(statusCode, attempt, httpMethod, allowRetryForNonIdempotent))
                {
                    var delay = GetRetryDelay(attempt);

                    _logger.LogWarning(
                        "Tentativa {Attempt} falhou para operação '{Operation}' ({HttpMethod}) com status {StatusCode}. Tentando novamente em {Delay}ms...",
                        attempt + 1, operation, httpMethod.Method, statusCode, delay);

                    await Task.Delay(delay);
                    continue;
                }

                // Se não deve fazer retry, registra erro e retorna
                var reason = statusCode == 409 
                    ? "conflito detectado (409) - retry não permitido" 
                    : !IsIdempotentMethod(httpMethod) && !allowRetryForNonIdempotent
                        ? $"método {httpMethod.Method} não-idempotente - retry não permitido"
                        : "erro não transiente ou tentativas excedidas";

                _logger.LogError(
                    "Operação '{Operation}' ({HttpMethod}) falhou após {Attempts} tentativas com status {StatusCode}: {ErrorMessage}. Motivo: {Reason}",
                    operation, httpMethod.Method, attempt + 1, statusCode, result.Error?.Message, reason);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Exceção de rede na tentativa {Attempt} da operação '{Operation}' ({HttpMethod})",
                    attempt + 1, operation, httpMethod.Method);

                // Apenas faz retry de exceções de rede para métodos idempotentes
                if (attempt < maxAttempts - 1 && (IsIdempotentMethod(httpMethod) || allowRetryForNonIdempotent))
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
                    "Exceção inesperada na tentativa {Attempt} da operação '{Operation}' ({HttpMethod})",
                    attempt + 1, operation, httpMethod.Method);

                // Para exceções não transientes, não faz retry
                return Result<T>.Failure(Error.Internal($"Erro inesperado: {ex.Message}"));
            }
        }

        // Nunca deve chegar aqui, mas para segurança
        return Result<T>.Failure(Error.Internal("Operação falhou após todas as tentativas"));
    }

    /// <summary>
    /// Verifica se o método HTTP é idempotente (seguro para retry).
    /// </summary>
    /// <param name="method">Método HTTP</param>
    /// <returns>True se idempotente (GET, HEAD, OPTIONS, PUT)</returns>
    private bool IsIdempotentMethod(HttpMethod method)
    {
        // GET, HEAD, OPTIONS: sempre idempotentes
        // PUT: tecnicamente idempotente (mesma operação múltiplas vezes = mesmo resultado)
        // POST, DELETE, PATCH: não-idempotentes
        return method == HttpMethod.Get || 
               method == HttpMethod.Head || 
               method == HttpMethod.Options;
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
