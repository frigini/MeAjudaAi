namespace MeAjudaAi.AppHost.Helpers;

/// <summary>
/// Métodos auxiliares para detecção robusta de ambiente
/// </summary>
public static class EnvironmentHelpers
{
    /// <summary>
    /// Determina se a aplicação atual está sendo executada em um ambiente de teste
    /// usando verificações robustas case-insensitive em múltiplas variáveis de ambiente
    /// </summary>
    /// <param name="builder">O distributed application builder</param>
    /// <returns>True se estiver executando em ambiente de teste, false caso contrário</returns>
    public static bool IsTesting(IDistributedApplicationBuilder builder)
    {
        // Verifica o nome do ambiente do builder (case-insensitive)
        var builderEnv = builder.Environment.EnvironmentName;
        if (!string.IsNullOrEmpty(builderEnv) &&
            string.Equals(builderEnv, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Verifica DOTNET_ENVIRONMENT primeiro, depois faz fallback para ASPNETCORE_ENVIRONMENT
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;

        if (!string.IsNullOrEmpty(envName) &&
            string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Verifica variável de ambiente INTEGRATION_TESTS com parsing booleano robusto
        var integrationTestsValue = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        if (!string.IsNullOrEmpty(integrationTestsValue))
        {
            // Trata tanto padrões "true"/"false" quanto "1"/"0" de forma case-insensitive
            if (bool.TryParse(integrationTestsValue, out var boolResult))
            {
                return boolResult;
            }

            // Trata "1" como true (comum em ambientes CI/CD)
            if (string.Equals(integrationTestsValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Obtém o nome do ambiente efetivo com prioridade de fallback: DOTNET_ENVIRONMENT -> ASPNETCORE_ENVIRONMENT -> ambiente do builder
    /// </summary>
    /// <param name="builder">O distributed application builder</param>
    /// <returns>O nome do ambiente efetivo ou string vazia se não encontrado</returns>
    private static string GetEffectiveEnvName(IDistributedApplicationBuilder builder)
    {
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.IsNullOrEmpty(dotnetEnv)) return dotnetEnv;
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(aspnetEnv)) return aspnetEnv;
        return builder.Environment.EnvironmentName ?? string.Empty;
    }

    /// <summary>
    /// Verifica se o ambiente atual corresponde ao nome do ambiente alvo (case-insensitive)
    /// </summary>
    /// <param name="builder">O distributed application builder</param>
    /// <param name="target">O nome do ambiente alvo a verificar</param>
    /// <returns>True se o ambiente atual corresponder ao alvo, false caso contrário</returns>
    private static bool IsEnv(IDistributedApplicationBuilder builder, string target) =>
        string.Equals(GetEffectiveEnvName(builder), target, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determina se a aplicação atual está sendo executada em um ambiente de desenvolvimento
    /// </summary>
    /// <param name="builder">O distributed application builder</param>
    /// <returns>True se estiver executando em ambiente de desenvolvimento, false caso contrário</returns>
    public static bool IsDevelopment(IDistributedApplicationBuilder builder) => IsEnv(builder, "Development");

    /// <summary>
    /// Determina se a aplicação atual está sendo executada em um ambiente de produção
    /// </summary>
    /// <param name="builder">O distributed application builder</param>
    /// <returns>True se estiver executando em ambiente de produção, false caso contrário</returns>
    public static bool IsProduction(IDistributedApplicationBuilder builder) => IsEnv(builder, "Production");

    /// <summary>
    /// Obtém o nome do ambiente atual com prioridade de fallback: DOTNET_ENVIRONMENT -> ASPNETCORE_ENVIRONMENT -> ambiente do builder
    /// </summary>
    /// <param name="builder">O distributed application builder</param>
    /// <returns>O nome do ambiente ou string vazia se não encontrado</returns>
    public static string GetEnvironmentName(IDistributedApplicationBuilder builder) => GetEffectiveEnvName(builder);
}
