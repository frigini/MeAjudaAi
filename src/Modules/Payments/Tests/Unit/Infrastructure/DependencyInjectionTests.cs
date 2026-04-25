using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
public class DependencyInjectionTests
{
    [Fact]
    public async Task AddInfrastructure_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:Payments", DatabaseConstants.DefaultTestConnectionString},
            {"Stripe:ApiKey", "sk_test_123"},
            {"ClientBaseUrl", "https://test.com"},
            {"Payments:SuccessUrl", "success"},
            {"Payments:CancelUrl", "cancel"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);
        envMock.SetupGet(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());

        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddInfrastructure(configuration, envMock.Object);
        services.AddLogging();
        
        await using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<PaymentsDbContext>().Should().NotBeNull();
        provider.GetRequiredService<ISubscriptionRepository>().Should().NotBeNull();
        provider.GetRequiredService<IPaymentTransactionRepository>().Should().NotBeNull();
        provider.GetRequiredService<IPaymentGateway>().Should().NotBeNull();
        provider.GetRequiredService<IStripeService>().Should().NotBeNull();
        provider.GetRequiredService<IStripeClient>().Should().NotBeNull();
        
        // Hosted Services
        var hostedServices = provider.GetServices<IHostedService>();
        hostedServices.Should().Contain(s => s is ProcessInboxJob);
        
        // Lifetime Check
        services.Single(d => d.ServiceType == typeof(ISubscriptionRepository)).Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}
