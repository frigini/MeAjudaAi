using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;
using MeAjudaAi.Shared.Database;
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
        services.AddDbContext<BookingsDbContext>(options =>
        {
            var connStr = configuration.GetConnectionString("Bookings") ??
                          configuration.GetConnectionString("DefaultConnection") ?? 
                          configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
#pragma warning disable S2068
                connStr = DatabaseConstants.DefaultTestConnectionString;
#pragma warning restore S2068
            }

            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException("Bookings connection string is missing.");
            }

            // Development must supply a real connection string for any non-local deployment
            options.UseNpgsql(connStr, m => 
            {
                m.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                m.MigrationsHistoryTable("__EFMigrationsHistory", "bookings");
                m.MigrationsAssembly(typeof(BookingsDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IProviderScheduleRepository, ProviderScheduleRepository>();

        return services;
    }
}
