using Hangfire.Dashboard;

namespace MeAjudaAi.Shared.Jobs;

/// <summary>
/// Filtro de autorização para o Hangfire Dashboard.
/// 
/// IMPORTANTE: Em produção, implemente autenticação adequada.
/// Esta implementação permite acesso apenas em desenvolvimento.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Em desenvolvimento, permite acesso livre
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (environment == "Development" || environment == "Testing")
        {
            return true;
        }

        // Em produção, requer autenticação
        // TODO: Implementar verificação de claims/roles apropriadas
        // Exemplo: return httpContext.User.IsInRole("Admin");
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
