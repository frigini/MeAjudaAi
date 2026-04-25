using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Shared.Database;
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
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:Payments", DatabaseConstants.DefaultTestConnectionString},
            {"ClientBaseUrl", "https://test.com"},
            {"Payments:SuccessUrl", "success"},
            {"Payments:CancelUrl", "cancel"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.SetupGet(e => e.EnvironmentName).Returns("Testing");

        services.AddSingleton<IConfiguration>(configuration);

        services.AddInfrastructure(configuration, envMock.Object);
        services.AddLogging();
        
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<PaymentsDbContext>().Should().NotBeNull();
        provider.GetRequiredService<ISubscriptionRepository>().Should().NotBeNull();
        provider.GetRequiredService<IPaymentTransactionRepository>().Should().NotBeNull();
        provider.GetRequiredService<IPaymentGateway>().Should().NotBeNull();
        
        var hostedServices = provider.GetServices<IHostedService>();
        hostedServices.Should().Contain(s => s is ProcessInboxJob);
        
        services.Single(d => d.ServiceType == typeof(ISubscriptionRepository)).Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}