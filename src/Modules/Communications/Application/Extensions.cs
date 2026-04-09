using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Application.ModuleApi;
using MeAjudaAi.Modules.Communications.Application.Services;
using MeAjudaAi.Modules.Communications.Application.Services.Email;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Application;

public static class Extensions
{
    /// <summary>
    /// Registra os serviços da camada de Application do módulo Communications.
    /// </summary>
    public static IServiceCollection AddCommunicationsApplication(this IServiceCollection services)
    {
        // Public API
        services.AddScoped<ICommunicationsModuleApi, CommunicationsModuleApi>();

        // Serviços de aplicação
        services.AddScoped<IOutboxProcessorService, OutboxProcessorService>();
        services.AddScoped<IEmailService, StubEmailService>();

        // Integration Event Handlers
        services.AddScoped<IEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderActivatedIntegrationEvent>, ProviderActivatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderAwaitingVerificationIntegrationEvent>, ProviderAwaitingVerificationIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderVerificationStatusUpdatedIntegrationEvent>, ProviderVerificationStatusUpdatedIntegrationEventHandler>();

        return services;
    }
}
