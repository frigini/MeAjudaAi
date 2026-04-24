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
        // 1. Determinar o nome do ambiente a partir do objeto ou das variáveis de ambiente
        var envName = environment?.EnvironmentName 
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var isKnownBypassEnvironment = string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envName, "Integration", StringComparison.OrdinalIgnoreCase);

        if (!isKnownBypassEnvironment)
        {
            return false;
        }

        // 2. Verificar a flag específica para testes de integração — só é honrada quando o
        //    ambiente já é reconhecidamente não-produtivo (passo 1 passou), evitando que
        //    a variável INTEGRATION_TESTS=true ative bypass acidentalmente em produção.
        if (string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return isKnownBypassEnvironment;
    }
}
