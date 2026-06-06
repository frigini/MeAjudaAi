using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateSubscriptionCommand, string>, CreateSubscriptionCommandHandler>();
        services.AddScoped<ICommandHandler<GetBillingPortalCommand, string>, GetBillingPortalCommandHandler>();
        services.AddScoped<IQueryHandler<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>, GetActiveSubscriptionByProviderHandler>();

        return services;
    }
}
