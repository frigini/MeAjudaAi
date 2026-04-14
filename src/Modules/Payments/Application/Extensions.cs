using System.Reflection;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateSubscriptionCommand, string>, CreateSubscriptionCommandHandler>();

        return services;
    }
}
