using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Modules.Locations.Application.ModuleApi;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Events;
using MeAjudaAi.Modules.Locations.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using MeAjudaAi.Modules.Locations.Infrastructure.Filters;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Locations.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo Locations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo Locations.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddServices(configuration);
        services.AddEventHandlers();

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<LocationsDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                if (MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
                {
                    // Fallback para testes/dev quando a string de conexão não é crítica na inicialização do DI
                    connectionString = MeAjudaAi.Shared.Database.Constants.DatabaseConstants.DefaultTestConnectionString;
                }
                else
                {
                    throw new InvalidOperationException("DefaultConnection is not configured");
                }
            }

            options.UseNpgsql(connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "locations");
                    npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Locations.Infrastructure");
                });

            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            var hostEnvironment = serviceProvider.GetService<IHostEnvironment>();
            var isDevelopment = hostEnvironment?.IsDevelopment() == true;
            
            if (isDevelopment)
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });

        // Registrar Func<LocationsDbContext> para uso em migrations (design-time)
        services.AddScoped<Func<LocationsDbContext>>(provider => () =>
        {
            var context = provider.GetRequiredService<LocationsDbContext>();
            return context;
        });

        // Unit of Work e Repositórios
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Locations, (sp, key) => sp.GetRequiredService<LocationsDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LocationsDbContext>());

        services.AddScoped<IRepository<AllowedCity, Guid>>(sp => sp.GetRequiredService<LocationsDbContext>());

        // Consultas otimizadas
        services.AddScoped<IAllowedCityQueries, DbContextAllowedCityQueries>();

        // Registrar ExceptionHandler para exceções de domínio
        services.AddExceptionHandler<LocationsExceptionHandler>();

        // Command e Query Handlers
        var applicationAssembly = typeof(Application.Handlers.CreateAllowedCityHandler).Assembly;

        // Registrar todos os ICommandHandler<T> e ICommandHandler<T, TResult>
        var commandHandlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
                .Select(i => new { Interface = i, Implementation = t }))
            .ToList();

        foreach (var handler in commandHandlerTypes)
        {
            services.AddScoped(handler.Interface, sp =>
            {
                var uow = sp.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.Locations);
                return ActivatorUtilities.CreateInstance(sp, handler.Implementation, uow);
            });
        }

        // Registrar todos os IQueryHandler<T, TResult>
        var queryHandlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }))
            .ToList();

        foreach (var handler in queryHandlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }
    }

    /// <summary>
    /// Configura os clientes HTTP e serviços externos do módulo.
    /// </summary>
    private static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // HTTP clients para APIs de CEP
        // ServiceDefaults já configura resiliência (retry, circuit breaker, timeout)
        services.AddHttpClient<ViaCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:ViaCep:BaseUrl"]
                ?? "https://viacep.com.br/";
            
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<BrasilApiCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:BrasilApi:BaseUrl"]
                ?? "https://brasilapi.com.br/";
            
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<OpenCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:OpenCep:BaseUrl"]
                ?? "https://opencep.com/";
            
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // HTTP client para Nominatim (geocoding)
        services.AddHttpClient<NominatimClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:Nominatim:BaseUrl"]
                ?? "https://nominatim.openstreetmap.org/";
            client.BaseAddress = new Uri(baseUrl);

            // Configurar User-Agent conforme política de uso do Nominatim
            var userAgent = configuration["Locations:ExternalApis:Nominatim:UserAgent"]
                ?? "MeAjudaAi-Tests/1.0 (https://github.com/frigini/MeAjudaAi)";
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        // HTTP client para IBGE Localidades
        services.AddHttpClient<IIbgeClient, IbgeClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:IBGE:BaseUrl"]
                ?? "https://servicodados.ibge.gov.br/api/v1/localidades/";

            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            client.BaseAddress = new Uri(baseUrl);
        });

        // Serviços de negócio
        services.AddScoped<ICepLookupService, CepLookupService>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        services.AddScoped<IIbgeService, IbgeService>();

        // Adapter para middleware (Shared → Locations)
        services.AddScoped<IGeographicValidationService, GeographicValidationService>();

        // Module API
        services.AddScoped<ILocationsModuleApi, LocationsModuleApi>();
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Locations.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<AllowedCityCreatedDomainEvent>, AllowedCityCreatedDomainEventHandler>();
        services.AddScoped<IEventHandler<AllowedCityUpdatedDomainEvent>, AllowedCityUpdatedDomainEventHandler>();
        services.AddScoped<IEventHandler<AllowedCityDeletedDomainEvent>, AllowedCityDeletedDomainEventHandler>();
    }
}
