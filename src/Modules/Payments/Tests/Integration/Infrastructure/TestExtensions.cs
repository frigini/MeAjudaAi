using MeAjudaAi.Modules.Payments.Application;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Payments;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Tests.Integration.Infrastructure;

public static class TestExtensions
{
    public static IServiceCollection AddPaymentsTestInfrastructure(this IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddCommonModuleTestInfrastructure<PaymentsDbContext>(options);

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(MeAjudaAi.Shared.Database.Constants.ModuleKeys.Payments,
            (sp, key) => sp.GetRequiredService<PaymentsDbContext>());

        services.AddScoped<IRepository<Subscription, Guid>>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        services.AddScoped<ISubscriptionQueries, DbContextSubscriptionQueries>();
        services.AddScoped<IPaymentTransactionQueries, DbContextPaymentTransactionQueries>();
        services.AddScoped<IPaymentsHealthQueries, DbContextPaymentsHealthQueries>();

        services.AddSingleton(new PaymentsOptions
        {
            Plans = new Dictionary<string, PlanOptions>
            {
                ["basic"] = new() { Amount = 29.90m, Currency = "BRL", StripePriceId = "price_basic" },
                ["pro"] = new() { Amount = 79.90m, Currency = "BRL", StripePriceId = "price_pro" }
            }
        });

        services.AddCommands();
        services.AddQueries();
        services.AddApplication();

        return services;
    }
}
