using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Application.ModuleApi;
using MeAjudaAi.Modules.Communications.Application.Services;
using MeAjudaAi.Modules.Communications.Application.Services.Email;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
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
        services.AddScoped<IEventHandler<ProviderRegisteredIntegrationEvent>, ProviderRegisteredIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderAwaitingVerificationIntegrationEvent>, ProviderAwaitingVerificationIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderVerificationStatusUpdatedIntegrationEvent>, ProviderVerificationStatusUpdatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<DocumentVerifiedIntegrationEvent>, DocumentVerifiedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<DocumentRejectedIntegrationEvent>, DocumentRejectedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<BookingCreatedIntegrationEvent>, BookingCreatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<BookingConfirmedIntegrationEvent>, BookingConfirmedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<BookingCancelledIntegrationEvent>, BookingCancelledIntegrationEventHandler>();
        services.AddScoped<IEventHandler<BookingRejectedIntegrationEvent>, BookingRejectedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<BookingCompletedIntegrationEvent>, BookingCompletedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<UserProfileUpdatedIntegrationEvent>, UserProfileUpdatedIntegrationEventHandler>();

        // Background Workers
        if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Testing", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            services.AddHostedService<CommunicationsOutboxWorker>();
        }

        return services;
    }
}
