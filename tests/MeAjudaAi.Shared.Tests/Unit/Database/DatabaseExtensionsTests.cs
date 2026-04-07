using System.Linq;
using FluentAssertions;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class DatabaseExtensionsTests
{
    [Fact]
    public void AddPostgres_WithValidConnectionString_ShouldSucceed()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be("Host=localhost;Database=test");
    }

    [Fact]
    public void AddPostgres_ShouldRegisterPostgresOptionsService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);

        // Assert - check if the service was registered in the collection
        var hasOptions = services.Any(d => d.ServiceType == typeof(IOptions<PostgresOptions>));
        hasOptions.Should().BeTrue();
    }
}