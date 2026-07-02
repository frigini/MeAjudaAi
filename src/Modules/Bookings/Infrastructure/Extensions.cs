using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Bookings, Schemas.Bookings, DatabaseRoleConstants.Bookings);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<BookingsDbContext>((serviceProvider, options) =>
        {
            var connStr = configuration.GetConnectionString("DefaultConnection")
                          ?? configuration.GetConnectionString("Bookings")
                          ?? configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
                connStr = DatabaseConstants.DefaultTestConnectionString;
            }

            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException(
                    "Connection string not found. Configure ConnectionStrings:DefaultConnection, Bookings, or meajudaai-db.");
            }

            options.UseNpgsql(connStr, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                npgsqlOptions.MigrationsAssembly(typeof(BookingsDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Bookings);
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Bookings, (sp, key) => sp.GetRequiredService<BookingsDbContext>());

        // Repositories
        services.AddScoped<IRepository<Booking, Guid>>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddScoped<IRepository<ProviderSchedule, Guid>>(sp => sp.GetRequiredService<BookingsDbContext>());

        // Queries
        services.AddScoped<IBookingQueries, DbContextBookingQueries>();
        services.AddScoped<IProviderScheduleQueries, DbContextProviderScheduleQueries>();
        services.AddScoped<IBookingCommandService, BookingCommandService>();
    }

    private static void AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<BookingCreatedDomainEvent>, BookingCreatedDomainEventHandler>();
        services.AddScoped<IEventHandler<BookingConfirmedDomainEvent>, BookingConfirmedDomainEventHandler>();
        services.AddScoped<IEventHandler<BookingCancelledDomainEvent>, BookingCancelledDomainEventHandler>();
        services.AddScoped<IEventHandler<BookingCompletedDomainEvent>, BookingCompletedDomainEventHandler>();
        services.AddScoped<IEventHandler<BookingRejectedDomainEvent>, BookingRejectedDomainEventHandler>();
    }
}