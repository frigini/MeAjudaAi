using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Payments;
using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Queries;
using MeAjudaAi.Modules.Payments.Application.ModuleApi;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Application;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPaymentsModuleApi, PaymentsModuleApi>();
        services.AddScoped<IReturnUrlResolver, ReturnUrlResolver>();
        services.AddScoped<ICommandHandler<CreateSubscriptionCommand, string>, CreateSubscriptionCommandHandler>();
        services.AddScoped<ICommandHandler<CreateBillingPortalSessionCommand, string>, CreateBillingPortalSessionCommandHandler>();
        services.AddScoped<IQueryHandler<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>, GetActiveSubscriptionByProviderQueryHandler>();

        return services;
    }
}
