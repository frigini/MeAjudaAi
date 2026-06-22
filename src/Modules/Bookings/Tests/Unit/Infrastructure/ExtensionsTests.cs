using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
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
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString
            })
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddLogging();
        services.AddSingleton(new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>().Object);

        // Act
        MeAjudaAi.Modules.Bookings.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);
        var provider = services.BuildServiceProvider();

        // Assert - Core services
        provider.GetRequiredService<IBookingQueries>().Should().NotBeNull();
        provider.GetRequiredService<IProviderScheduleQueries>().Should().NotBeNull();
        provider.GetRequiredService<IBookingCommandService>().Should().NotBeNull();

        // Assert - Event handlers
        provider.GetRequiredService<IEventHandler<BookingCreatedDomainEvent>>().Should().NotBeNull();
        provider.GetRequiredService<IEventHandler<BookingConfirmedDomainEvent>>().Should().NotBeNull();
        provider.GetRequiredService<IEventHandler<BookingCancelledDomainEvent>>().Should().NotBeNull();
        provider.GetRequiredService<IEventHandler<BookingCompletedDomainEvent>>().Should().NotBeNull();
        provider.GetRequiredService<IEventHandler<BookingRejectedDomainEvent>>().Should().NotBeNull();
    }
}
