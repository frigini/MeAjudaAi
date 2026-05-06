using MeAjudaAi.Modules.Locations.Application.ModuleApi;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using MeAjudaAi.Modules.Locations.Infrastructure.Filters;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Database;
using Microsoft.AspNetCore.Builder;
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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar DbContext para Locations module
        services.AddDbContext<LocationsDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                if (MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment())
                {
                    // Fallback para testes/dev quando a string de conexão não é crítica na inicialização do DI
#pragma warning disable S2068 // "password" detected here, make sure this is not a hard-coded credential
                    connectionString = MeAjudaAi.Shared.Database.DatabaseConstants.DefaultTestConnectionString;
#pragma warning restore S2068
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
            var environment = serviceProvider.GetService<IHostEnvironment>();
            var isDevelopment = environment?.IsDevelopment() == true;
            
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

        // Registrar IUnitOfWork e IAllowedCityQueries
        services.AddScoped<IUnitOfWork, LocationsDbContext>();
        services.AddScoped<IAllowedCityQueries, DbContextAllowedCityQueries>();

        // Registrar ExceptionHandler para exceções de domínio
        services.AddExceptionHandler<LocationsExceptionHandler>();

        // Registrar HTTP clients para APIs de CEP
        // ServiceDefaults já configura resiliência (retry, circuit breaker, timeout)
        services.AddHttpClient<ViaCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:ViaCep:BaseUrl"]
                ?? "https://viacep.com.br/"; // Fallback para testes
            
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<BrasilApiCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:BrasilApi:BaseUrl"]
                ?? "https://brasilapi.com.br/"; // Fallback para testes
            
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<OpenCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:OpenCep:BaseUrl"]
                ?? "https://opencep.com/"; // Fallback para testes
            
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Registrar HTTP client para Nominatim (geocoding)
        services.AddHttpClient<NominatimClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:Nominatim:BaseUrl"]
                ?? "https://nominatim.openstreetmap.org/"; // Fallback para testes
            client.BaseAddress = new Uri(baseUrl);

            // Configurar User-Agent conforme política de uso do Nominatim
            var userAgent = configuration["Locations:ExternalApis:Nominatim:UserAgent"]
                ?? "MeAjudaAi-Tests/1.0 (https://github.com/frigini/MeAjudaAi)"; // Fallback para testes
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        // Registrar HTTP client para IBGE Localidades
        services.AddHttpClient<IIbgeClient, IbgeClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:IBGE:BaseUrl"]
                ?? "https://servicodados.ibge.gov.br/api/v1/localidades/"; // Fallback para testes

            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            client.BaseAddress = new Uri(baseUrl);
        });

        // Registrar serviços
        services.AddScoped<ICepLookupService, CepLookupService>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        services.AddScoped<IIbgeService, IbgeService>();

        // Registrar adapter para middleware (Shared → Locations)
        services.AddScoped<IGeographicValidationService, GeographicValidationService>();

        // Registrar Module API
        services.AddScoped<ILocationsModuleApi, LocationsModuleApi>();

        // Registrar Command e Query Handlers automaticamente
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
            services.AddScoped(handler.Interface, handler.Implementation);
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

        return services;
    }
}
