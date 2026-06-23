using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class ExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString,
                ["Stripe:ApiKey"] = "sk_test_dummy_key",
                ["Payments:SuccessUrl"] = "/success",
                ["Payments:CancelUrl"] = "/cancel"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddLogging();
        services.AddSingleton(new Mock<IMessageBus>().Object);

        // Act
        MeAjudaAi.Modules.Payments.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);

        // Assert - verify registrations exist in the collection
        services.Should().Contain(s => s.ServiceType == typeof(PaymentsOptions));
        services.Should().Contain(s => s.ServiceType == typeof(IPaymentGateway));
        services.Should().Contain(s => s.ServiceType == typeof(IStripeService));
        services.Should().Contain(s => s.ServiceType == typeof(ISubscriptionQueries));
        services.Should().Contain(s => s.ServiceType == typeof(IPaymentTransactionQueries));
        services.Should().Contain(s => s.ServiceType == typeof(IPaymentCommandService));
        services.Should().Contain(s => s.ServiceType == typeof(Stripe.IStripeClient));

        // Assert - event handlers
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<SubscriptionActivatedDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<SubscriptionCanceledDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<SubscriptionExpiredDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<SubscriptionRenewedDomainEvent>));
    }
}
