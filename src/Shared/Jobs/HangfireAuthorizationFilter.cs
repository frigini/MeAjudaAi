using Hangfire.Dashboard;
using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Shared.Jobs;

/// <summary>
/// Filtro de autorização para o Hangfire Dashboard.
/// Em produção, apenas administradores do sistema podem acessar.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Verificar ambiente usando ambas as variáveis de ambiente
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                         Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                         "Production";

        // Em desenvolvimento ou testes, permite acesso livre
        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Em produção, requer autenticação E role de administrador do sistema
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Apenas administradores do sistema podem acessar o dashboard
        return httpContext.User.IsSystemAdmin();
    }
}
