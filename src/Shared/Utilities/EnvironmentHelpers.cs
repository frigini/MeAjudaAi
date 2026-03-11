using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Classes utilitárias relacionadas ao ambiente de execução da aplicação.
/// </summary>
public static class EnvironmentHelpers
{
    /// <summary>
    /// Verifica se o ambiente atual é considerado um ambiente de teste ou desenvolvimento 
    /// que deve bypassar validações estritas de segurança (ex: CORS, Authority do Keycloak).
    /// </summary>
    /// <param name="environment">Opcional. Instância de IWebHostEnvironment para verificação robusta de ambiente.</param>
    /// <returns>True se for um ambiente de teste/desenvolvimento, False caso contrário.</returns>
    public static bool IsSecurityBypassEnvironment(IWebHostEnvironment? environment = null)
    {
        // 1. Always check for specific flag for integration tests first
        if (string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 2. Determine environment name from object or variables
        // We prioritize the passed environment object if available
        var envName = environment?.EnvironmentName 
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
        if (string.IsNullOrEmpty(envName))
        {
            return false;
        }

        // 3. Check against known bypass environments
        return string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase) 
            || string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envName, "Integration", StringComparison.OrdinalIgnoreCase);
    }
}
