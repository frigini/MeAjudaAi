using System.Net;

namespace MeAjudaAi.Web.Admin.Services.Resilience.Http;

/// <summary>
/// Mensagens de erro amigáveis para diferentes cenários de falha da API
/// </summary>
public static class ApiErrorMessages
{
    /// <summary>
    /// Obtém mensagem de erro amigável baseada no código de status HTTP
    /// </summary>
    public static string GetFriendlyMessage(HttpStatusCode statusCode, string? operation = null)
    {
        var operationText = string.IsNullOrWhiteSpace(operation) 
            ? "A operação" 
            : operation;

        return statusCode switch
        {
            HttpStatusCode.BadRequest => 
                $"{operationText} contém dados inválidos. Verifique os campos e tente novamente.",
            
            HttpStatusCode.Unauthorized => 
                "Sua sessão expirou. Por favor, faça login novamente.",
            
            HttpStatusCode.Forbidden => 
                "Você não tem permissão para realizar esta operação.",
            
            HttpStatusCode.NotFound => 
                $"{operationText} não pôde ser concluída porque o recurso não foi encontrado.",
            
            HttpStatusCode.Conflict => 
                $"{operationText} não pôde ser concluída devido a um conflito. O recurso pode já existir ou estar em uso.",
            
            HttpStatusCode.RequestTimeout => 
                $"{operationText} demorou muito tempo. Tente novamente.",
            
            HttpStatusCode.TooManyRequests => 
                "Muitas requisições em pouco tempo. Aguarde alguns instantes e tente novamente.",
            
            HttpStatusCode.InternalServerError => 
                $"Ocorreu um erro no servidor ao processar {operationText.ToLower()}. Nossa equipe foi notificada.",
            
            HttpStatusCode.BadGateway => 
                "O servidor está temporariamente indisponível. Tente novamente em alguns instantes.",
            
            HttpStatusCode.ServiceUnavailable => 
                "O serviço está temporariamente indisponível. Tente novamente em alguns instantes.",
            
            HttpStatusCode.GatewayTimeout => 
                $"{operationText} demorou muito tempo para responder. Tente novamente.",
            
            _ when ((int)statusCode >= 500) => 
                $"Ocorreu um erro no servidor. Nossa equipe foi notificada.",
            
            _ => 
                $"Ocorreu um erro ao processar {operationText.ToLower()}. Tente novamente."
        };
    }

    /// <summary>
    /// Obtém mensagem de erro amigável para uma exceção
    /// </summary>
    public static string GetFriendlyMessage(Exception exception, string? operation = null)
    {
        return exception switch
        {
            HttpRequestException httpEx when httpEx.StatusCode.HasValue =>
                GetFriendlyMessage(httpEx.StatusCode.Value, operation),
            
            HttpRequestException =>
                WithOperation(NetworkError),
            
            TaskCanceledException or TimeoutException =>
                WithOperation(Timeout),
            
            _ =>
                WithOperation(UnknownError)
        };

        string WithOperation(string message) =>
            string.IsNullOrWhiteSpace(operation) ? message : $"{operation}: {message}";
    }

    /// <summary>
    /// Mensagem para circuit breaker aberto
    /// </summary>
    public static string CircuitBreakerOpen => 
        "O serviço está temporariamente indisponível devido a múltiplas falhas. " +
        "Aguarde alguns instantes enquanto tentamos restabelecer a conexão.";

    /// <summary>
    /// Mensagem para timeout
    /// </summary>
    public static string Timeout => 
        "A operação demorou muito tempo para responder. " +
        "Verifique sua conexão e tente novamente.";

    /// <summary>
    /// Mensagem para erro de rede
    /// </summary>
    public static string NetworkError => 
        "Não foi possível conectar ao servidor. " +
        "Verifique sua conexão com a internet e tente novamente.";

    /// <summary>
    /// Mensagem genérica para erros desconhecidos
    /// </summary>
    public static string UnknownError => 
        "Ocorreu um erro inesperado. Tente novamente.";


}
