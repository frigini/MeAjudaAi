using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

public class SecurityExtensionsTests
{
    [Fact]
    public void ValidateSecurityConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(null!, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("environment");
    }
}
