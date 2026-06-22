using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using MeAjudaAi.Modules.Payments.Infrastructure.Services;
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
using Microsoft.Extensions.Options;
using Stripe;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<PaymentsOptions>(configuration.GetSection(PaymentsOptions.SectionName));
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<PaymentsOptions>>().Value);
        services.AddSingleton<IValidateOptions<PaymentsOptions>>(new PaymentsOptionsValidator(configuration));

        services.AddPersistence(configuration, environment);
        services.AddServices(configuration, environment);
        services.AddEventHandlers();
        services.AddJobs();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Payments, Schemas.Payments, DatabaseRoleConstants.Payments);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
        {
            var connStr = configuration.GetConnectionString("Payments") ??
                          configuration.GetConnectionString("DefaultConnection") ?? 
                          configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
                connStr = DatabaseConstants.DefaultTestConnectionString;
            }

            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException("Payments connection string is missing.");
            }

            options.UseNpgsql(connStr, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "payments"));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Payments, (sp, key) => sp.GetRequiredService<PaymentsDbContext>());

        // Registra repositórios
        services.AddScoped<IRepository<MeAjudaAi.Modules.Payments.Domain.Entities.Subscription, Guid>>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IRepository<PaymentTransaction, Guid>>(sp => sp.GetRequiredService<PaymentsDbContext>());

        // Queries
        services.AddScoped<ISubscriptionQueries, DbContextSubscriptionQueries>();
        services.AddScoped<IPaymentTransactionQueries, DbContextPaymentTransactionQueries>();
        services.AddScoped<IPaymentsHealthQueries, DbContextPaymentsHealthQueries>();
        services.AddScoped<IPaymentCommandService, PaymentCommandService>();
    }

    private static void AddServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var isBypassEnvironment = EnvironmentHelpers.IsSecurityBypassEnvironment(environment);

        var apiKey = configuration["Stripe:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) && !isBypassEnvironment)
        {
            throw new InvalidOperationException("Stripe:ApiKey is missing or empty in configuration.");
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddScoped<IStripeClient>(_ => new StripeClient(apiKey));
            services.AddScoped<IStripeService, StripeService>();
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        }
        else
        {
            services.AddScoped<IStripeService, MockStripeService>();
            services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        }
    }

    private static void AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<SubscriptionActivatedDomainEvent>, SubscriptionActivatedDomainEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionCanceledDomainEvent>, SubscriptionCanceledDomainEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionExpiredDomainEvent>, SubscriptionExpiredDomainEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionRenewedDomainEvent>, SubscriptionRenewedDomainEventHandler>();
    }

    private static void AddJobs(this IServiceCollection services)
    {
        services.AddHostedService<ProcessInboxJob>();
    }
}
