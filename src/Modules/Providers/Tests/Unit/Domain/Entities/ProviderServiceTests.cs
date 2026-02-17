using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.Entities;

[Trait("Category", "Unit")]
public class ProviderServiceTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProviderService()
    {
        // Arrange
        var providerId = ProviderId.New();
        var serviceId = Guid.NewGuid();
        var serviceName = "Test Service";

        // Act
        var providerService = ProviderService.Create(providerId, serviceId, serviceName);

        // Assert
        providerService.Should().NotBeNull();
        providerService.ProviderId.Should().Be(providerId);
        providerService.ServiceId.Should().Be(serviceId);
        providerService.ServiceName.Should().Be(serviceName);
        providerService.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullProviderId_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProviderId? nullProviderId = null;
        var serviceId = Guid.NewGuid();

        // Act
        var act = () => ProviderService.Create(nullProviderId!, serviceId, "Service Name");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*providerId*");
    }

    [Fact]
    public void Create_WithEmptyServiceId_ShouldThrowArgumentException()
    {
        // Arrange
        var providerId = ProviderId.New();
        var emptyServiceId = Guid.Empty;

        // Act
        var act = () => ProviderService.Create(providerId, emptyServiceId, "Service Name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ServiceId cannot be empty*")
            .WithParameterName("serviceId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidServiceName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var providerId = ProviderId.New();
        var serviceId = Guid.NewGuid();

        // Act
#pragma warning disable CS8604 // Possible null reference argument
        var act = () => ProviderService.Create(providerId, serviceId, invalidName);
#pragma warning restore CS8604

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ServiceName cannot be empty*")
            .WithParameterName("serviceName");
    }

    [Fact]
    public void Create_WithDifferentServiceIds_ShouldCreateDifferentInstances()
    {
        // Arrange
        var providerId = ProviderId.New();
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();

        // Act
        var service1 = ProviderService.Create(providerId, serviceId1, "Service 1");
        var service2 = ProviderService.Create(providerId, serviceId2, "Service 2");

        // Assert
        service1.ServiceId.Should().NotBe(service2.ServiceId);
        service1.ProviderId.Should().Be(service2.ProviderId);
    }
}
