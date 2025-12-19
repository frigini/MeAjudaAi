namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware que previne ataques CRIME/BREACH verificando se é seguro comprimir a resposta.
/// Deve ser registrado ANTES do UseResponseCompression() no pipeline.
/// </summary>
public class CompressionSecurityMiddleware
{
    private readonly RequestDelegate _next;

    public CompressionSecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verifica se é seguro comprimir antes de permitir que o middleware de compressão processe
        if (!Extensions.PerformanceExtensions.IsSafeForCompression(context))
        {
            // Desabilita a compressão para esta requisição removendo o Accept-Encoding
            context.Request.Headers.Remove("Accept-Encoding");
        }

        await _next(context);
    }
}
