namespace MeAjudaAi.Shared.Models;

/// <summary>
/// Envelope padrão para respostas da API
/// </summary>
public sealed record Response<T>
{
    /// <summary>
    /// Dados da resposta
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Código HTTP da resposta
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Mensagem descritiva (opcional)
    /// </summary>
    public string? Message { get; init; }

    public Response(T? data, int statusCode = 200, string? message = null)
    {
        Data = data;
        StatusCode = statusCode;
        Message = message;
    }
}
