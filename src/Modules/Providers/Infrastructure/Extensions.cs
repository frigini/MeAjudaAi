using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Infrastructure;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços de infraestrutura do módulo Providers.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuração do DbContext
        services.AddDbContext<ProvidersDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in configuration. " +
                    "Please ensure the connection string is properly configured in appsettings.json or environment variables.");
            }

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ProvidersDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
            });
        });

        // Registro do repositório
        services.AddScoped<IProviderRepository, ProviderRepository>();

        // Registro do serviço de consultas
        services.AddScoped<IProviderQueryService, ProviderQueryService>();

        return services;
    }
}
