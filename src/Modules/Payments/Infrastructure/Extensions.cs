using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace MeAjudaAi.Modules.Payments.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<PaymentsDbContext>((serviceProvider, options) =>
        {
            var resolvedConfig = serviceProvider.GetRequiredService<IConfiguration>();
            var connStr = resolvedConfig.GetConnectionString("DefaultConnection") ?? 
                          resolvedConfig.GetConnectionString("Payments") ??
                          resolvedConfig.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
#pragma warning disable S2068
                connStr = "Host=localhost;Port=5432;Database=meajudaai_test;Username=postgres;Password=test";
#pragma warning restore S2068
            }

            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException("Payments connection string is missing.");
            }

            options.UseNpgsql(connStr, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "payments"));
        });

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        services.AddHostedService<ProcessInboxJob>();

        return services;
    }
}
