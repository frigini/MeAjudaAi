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
    /// <param name="environment">Opcional. Instância de IHostEnvironment para verificação robusta de ambiente.</param>
    /// <returns>True se for um ambiente de teste/desenvolvimento, False caso contrário.</returns>
    public static bool IsSecurityBypassEnvironment(IHostEnvironment? environment = null)
    {
        // 1. Sempre verificar a flag específica para testes de integração primeiro
        if (string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 2. Determinar o nome do ambiente a partir do objeto ou variáveis
        // Priorizamos o objeto de ambiente passado, se disponível
        var envName = environment?.EnvironmentName 
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
        if (string.IsNullOrEmpty(envName))
        {
            return false;
        }

        // 3. Verificar contra ambientes conhecidos de bypass
        return string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase) 
            || string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envName, "Integration", StringComparison.OrdinalIgnoreCase);
    }
}
