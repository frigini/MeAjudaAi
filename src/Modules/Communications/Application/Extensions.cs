using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Application.Handlers.Commands;
using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.ModuleApi;
using MeAjudaAi.Modules.Communications.Application.Services.Outbox;
using MeAjudaAi.Modules.Communications.Application.Workers;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Modules.Communications.Application;

public static class Extensions
{
    /// <summary>
    /// Registra os serviços da camada de Application do módulo Communications.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddModuleValidators(Assembly.GetExecutingAssembly());

        // Public API
        services.AddScoped<ICommunicationsModuleApi, CommunicationsModuleApi>();

        // Serviços de aplicação
        services.AddScoped<IOutboxProcessorService, OutboxProcessorService>();

        // Command Handlers
        services.AddScoped<ICommandHandler<CreateEmailTemplateCommand, Result<Guid>>, EmailTemplateCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateEmailTemplateCommand, Result>, EmailTemplateCommandHandler>();
        services.AddScoped<ICommandHandler<SetEmailTemplateStatusCommand, Result>, EmailTemplateCommandHandler>();

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

        // Novos Handlers
        services.AddScoped<IEventHandler<ProviderDeletedIntegrationEvent>, ProviderDeletedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<UserDeletedIntegrationEvent>, UserDeletedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ReviewApprovedIntegrationEvent>, ReviewApprovedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionActivatedIntegrationEvent>, SubscriptionActivatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionCanceledIntegrationEvent>, SubscriptionCanceledIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionExpiredIntegrationEvent>, SubscriptionExpiredIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionRenewedIntegrationEvent>, SubscriptionRenewedIntegrationEventHandler>();

        // Background Workers
        if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Testing", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            services.AddHostedService<CommunicationsOutboxWorker>();
        }

        return services;
    }
}
