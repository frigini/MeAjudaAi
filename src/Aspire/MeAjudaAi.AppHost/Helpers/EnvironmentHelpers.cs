namespace MeAjudaAi.AppHost.Helpers;

/// <summary>
/// Helper methods for robust environment detection
/// </summary>
public static class EnvironmentHelpers
{
    /// <summary>
    /// Determines if the current application is running in a testing environment
    /// using robust case-insensitive checks across multiple environment variables
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <returns>True if running in testing environment, false otherwise</returns>
    public static bool IsTesting(IDistributedApplicationBuilder builder)
    {
        // Check builder environment name (case-insensitive)
        var builderEnv = builder.Environment.EnvironmentName;
        if (!string.IsNullOrEmpty(builderEnv) && 
            string.Equals(builderEnv, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check DOTNET_ENVIRONMENT first, then fallback to ASPNETCORE_ENVIRONMENT
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;
        
        if (!string.IsNullOrEmpty(envName) && 
            string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check INTEGRATION_TESTS environment variable with robust boolean parsing
        var integrationTestsValue = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        if (!string.IsNullOrEmpty(integrationTestsValue))
        {
            // Handle both "true"/"false" and "1"/"0" patterns case-insensitively
            if (bool.TryParse(integrationTestsValue, out var boolResult))
            {
                return boolResult;
            }
            
            // Handle "1" as true (common in CI/CD environments)
            if (string.Equals(integrationTestsValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if the current application is running in a development environment
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <returns>True if running in development environment, false otherwise</returns>
    public static bool IsDevelopment(IDistributedApplicationBuilder builder)
    {
        var builderEnv = builder.Environment.EnvironmentName;
        if (!string.IsNullOrEmpty(builderEnv) && 
            string.Equals(builderEnv, "Development", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;
        
        return !string.IsNullOrEmpty(envName) && 
               string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the current application is running in a production environment
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <returns>True if running in production environment, false otherwise</returns>
    public static bool IsProduction(IDistributedApplicationBuilder builder)
    {
        var builderEnv = builder.Environment.EnvironmentName;
        if (!string.IsNullOrEmpty(builderEnv) && 
            string.Equals(builderEnv, "Production", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;
        
        return !string.IsNullOrEmpty(envName) && 
               string.Equals(envName, "Production", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the current environment name with fallback priority: DOTNET_ENVIRONMENT -> ASPNETCORE_ENVIRONMENT -> builder environment
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <returns>The environment name or empty string if not found</returns>
    public static string GetEnvironmentName(IDistributedApplicationBuilder builder)
    {
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.IsNullOrEmpty(dotnetEnv))
            return dotnetEnv;

        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(aspnetEnv))
            return aspnetEnv;

        return builder.Environment.EnvironmentName ?? string.Empty;
    }
}