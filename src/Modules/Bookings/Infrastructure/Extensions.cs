using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Bookings.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<BookingsDbContext>(options =>
        {
            var connStr = configuration.GetConnectionString("Bookings") ??
                          configuration.GetConnectionString("DefaultConnection") ?? 
                          configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
                connStr = DatabaseConstants.DefaultTestConnectionString;
            }

            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException("Bookings connection string is missing.");
            }

            options.UseNpgsql(connStr, m => 
            {
                m.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                m.MigrationsHistoryTable("__EFMigrationsHistory", "bookings");
                m.MigrationsAssembly(typeof(BookingsDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Bookings, (sp, key) => sp.GetRequiredService<BookingsDbContext>());

        // Repositories
        services.AddScoped<IRepository<Booking, Guid>>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddScoped<IRepository<ProviderSchedule, Guid>>(sp => sp.GetRequiredService<BookingsDbContext>());

        // Queries
        services.AddScoped<IBookingQueries, DbContextBookingQueries>();
        services.AddScoped<IProviderScheduleQueries, DbContextProviderScheduleQueries>();
        services.AddScoped<IBookingCommandService, DbContextBookingCommandService>();
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