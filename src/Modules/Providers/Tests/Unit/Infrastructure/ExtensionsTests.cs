using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure;

public class ExtensionsTests
{
    [Fact]
    public void AddEventHandlers_ShouldRegisterAllDomainEventHandlers()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString
            })
            .Build();
        var messageBusMock = new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>();
        services.AddSingleton(messageBusMock.Object);
        services.AddLogging(); // Registrar ILogger
        var environment = new Mock<IHostEnvironment>();
        
        // Act
        // Como AddEventHandlers é internal/privado, precisaremos chamar AddInfrastructure
        MeAjudaAi.Modules.Providers.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);
        var provider = services.BuildServiceProvider();

        // Assert - Domain Handlers
        provider.GetService<IEventHandler<ProviderActivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderAwaitingVerificationDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderServiceAddedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderServiceRemovedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderRegisteredDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderDeletedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderVerificationStatusUpdatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderProfileUpdatedDomainEvent>>().Should().NotBeNull();
        
        // Assert - Integration Handlers
        provider.GetService<IEventHandler<MeAjudaAi.Shared.Messaging.Messages.Documents.DocumentVerifiedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<MeAjudaAi.Shared.Messaging.Messages.Payments.SubscriptionActivatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<MeAjudaAi.Shared.Messaging.Messages.Payments.SubscriptionCanceledIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<MeAjudaAi.Shared.Messaging.Messages.Payments.SubscriptionExpiredIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs.ServiceNameUpdatedIntegrationEvent>>().Should().NotBeNull();
    }
}
