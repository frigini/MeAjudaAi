using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

namespace MeAjudaAi.Modules.Providers.Tests.Infrastructure;

/// <summary>
/// Extensões para configurar infraestrutura de testes específica do módulo Providers
/// </summary>
public static class ProvidersTestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona toda a infraestrutura de testes necessária para o módulo Providers
    /// </summary>
    public static IServiceCollection AddProvidersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);

        // Adicionar serviços compartilhados essenciais (incluindo TimeProvider)
        services.AddSingleton(TimeProvider.System);

        // Usar extensões compartilhadas
        services.AddTestLogging();
        services.AddTestCache(options.Cache);
        
        var configBuilder = new ConfigurationBuilder();
        if (options.FeatureFlags?.Any() == true)
        {
            configBuilder.AddInMemoryCollection(
                options.FeatureFlags.Select(kv => new KeyValuePair<string, string?>($"FeatureManagement:{kv.Key}", kv.Value.ToString())));
        }
        services.AddSingleton<IConfiguration>(configBuilder.Build());

        services.AddFeatureManagement();

        // Adicionar serviços de cache do Shared (incluindo ICacheService)
        // Para testes, usar implementação simples sem dependências complexas
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, MeAjudaAi.Shared.Tests.TestInfrastructure.Services.TestCacheService>();

        // Configurar DbContext
        services.AddDbContext<ProvidersDbContext>((serviceProvider, dbOptions) =>
        {
            if (options.Database.UseInMemoryDatabase)
            {
                dbOptions.UseInMemoryDatabase(options.Database.DatabaseName);
                dbOptions.ConfigureWarnings(warnings => 
                {
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning);
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
                });
            }
            else
            {
                string connectionString;
                
                if (!string.IsNullOrWhiteSpace(options.Database.ConnectionString))
                {
                    connectionString = options.Database.ConnectionString;
                }
                else
                {
                    var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
                    connectionString = container.GetConnectionString();
                }

                dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                    npgsqlOptions.CommandTimeout(60);
                });

                dbOptions.ConfigureWarnings(warnings =>
                {
                    // Suprimir warnings de pending model changes em testes
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
                });
            }
        }, ServiceLifetime.Scoped);

        // Adicionar repositórios específicos do Providers
        services.AddScoped<IProviderRepository, ProviderRepository>();

        // Adicionar serviços de aplicação específicos do Providers
        services.AddScoped<IProviderQueryService, ProviderQueryService>();

        // Adicionar Dispatcher de Queries e Commands
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        
        // Registrar handlers de teste explicitamente
        services.AddScoped<IQueryHandler<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>, GetPublicProviderByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>, GetProviderByUserIdQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateProviderProfileCommand, Result<ProviderDto>>, UpdateProviderProfileCommandHandler>();

        return services;
    }
}
