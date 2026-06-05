using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using MeAjudaAi.Modules.Payments.Infrastructure.Services;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;

namespace MeAjudaAi.Modules.Payments.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<PaymentsOptions>(configuration.GetSection(PaymentsOptions.SectionName));

        services.AddDbContext<PaymentsDbContext>(options =>
        {
            var connStr = configuration.GetConnectionString("Payments") ??
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
                throw new InvalidOperationException("Payments connection string is missing.");
            }

            options.UseNpgsql(connStr, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "payments"));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Payments, (sp, key) => sp.GetRequiredService<PaymentsDbContext>());

        services.AddScoped<ISubscriptionQueries, DbContextSubscriptionQueries>();
        services.AddScoped<IPaymentTransactionQueries, DbContextPaymentTransactionQueries>();
        services.AddScoped<IPaymentCommandService, DbContextPaymentCommandService>();
        
        services.AddScoped<IStripeClient>(provider => 
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var apiKey = config["Stripe:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new StripeClient("sk_test_dummy"); 
            }
            return new StripeClient(apiKey);
        });

        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        services.AddHostedService<ProcessInboxJob>();

        return services;
    }
}





