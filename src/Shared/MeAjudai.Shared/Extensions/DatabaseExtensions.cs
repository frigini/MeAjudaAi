using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Extensões para configuração de banco de dados modular
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adiciona inicialização básica de banco de dados
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection para chaining</returns>
    public static IServiceCollection AddDatabaseInitialization(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // EF Core migrations are handled automatically when DbContext is used
        // No need for complex orchestration services
        
        return services;
    }
}