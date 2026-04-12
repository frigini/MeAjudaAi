using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para garantir que o cookie de Antiforgery seja enviado em requisições GET.
/// Isso permite que SPAs leiam o cookie e enviem o token nos headers de requisições subsequentes (POST, PUT, DELETE).
/// </summary>
public sealed class AntiforgeryCookieMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly IAntiforgery _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));

    public async Task InvokeAsync(HttpContext context)
    {
        // Se for uma requisição GET, gera e armazena os tokens (configurando o cookie)
        if (HttpMethods.IsGet(context.Request.Method))
        {
            try
            {
                var tokens = _antiforgery.GetAndStoreTokens(context);
                
                // Opcional: Adicionar o token também no header da resposta atual para facilitar a captura inicial
                if (tokens.RequestToken != null)
                {
                    context.Response.Headers.Append("X-XSRF-TOKEN", tokens.RequestToken);
                }
            }
            catch (Exception)
            {
                // Falha silenciosa para evitar erro 500 se o Antiforgery/DataProtection não estiver configurado corretamente
                // Comum em ambientes de teste ou inicialização
            }
        }

        await _next(context);
    }
}
