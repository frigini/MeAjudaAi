using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:Payments", "Host=localhost;Database=test;Username=postgres;Password=test"},
            {"Stripe:ApiKey", "sk_test_123"},
            {"ClientBaseUrl", "https://test.com"},
            {"Payments:SuccessUrl", "success"},
            {"Payments:CancelUrl", "cancel"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddInfrastructure(configuration, envMock.Object);
        services.AddLogging(); // Required by some services
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<PaymentsDbContext>().Should().NotBeNull();
        provider.GetRequiredService<ISubscriptionRepository>().Should().NotBeNull();
        provider.GetRequiredService<IPaymentTransactionRepository>().Should().NotBeNull();
        provider.GetRequiredService<IPaymentGateway>().Should().NotBeNull();
    }
}
