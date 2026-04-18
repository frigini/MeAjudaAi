using MeAjudaAi.Shared.Caching;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

public class CachingExtensionsRegistrationTests
{
    [Fact]
    public void AddCaching_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddCaching(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ICacheService>().Should().NotBeNull();
        serviceProvider.GetService<ICacheMetrics>().Should().NotBeNull();
    }
}
