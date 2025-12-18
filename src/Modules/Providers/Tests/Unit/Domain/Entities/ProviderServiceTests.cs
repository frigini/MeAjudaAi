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

        // Act
        var providerService = ProviderService.Create(providerId, serviceId);

        // Assert
        providerService.Should().NotBeNull();
        providerService.ProviderId.Should().Be(providerId);
        providerService.ServiceId.Should().Be(serviceId);
        providerService.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullProviderId_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProviderId? nullProviderId = null;
        var serviceId = Guid.NewGuid();

        // Act
        var act = () => ProviderService.Create(nullProviderId!, serviceId);

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
        var act = () => ProviderService.Create(providerId, emptyServiceId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ServiceId cannot be empty*")
            .WithParameterName("serviceId");
    }

    [Fact]
    public void Create_MultipleTimes_ShouldHaveDifferentAddedAtTimes()
    {
        // Arrange
        var providerId = ProviderId.New();
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();

        // Act
        var service1 = ProviderService.Create(providerId, serviceId1);
        Thread.Sleep(10); // Pequeno delay para garantir timestamps diferentes
        var service2 = ProviderService.Create(providerId, serviceId2);

        // Assert
        service2.AddedAt.Should().BeAfter(service1.AddedAt);
    }

    [Fact]
    public void Create_WithDifferentServiceIds_ShouldCreateDifferentInstances()
    {
        // Arrange
        var providerId = ProviderId.New();
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();

        // Act
        var service1 = ProviderService.Create(providerId, serviceId1);
        var service2 = ProviderService.Create(providerId, serviceId2);

        // Assert
        service1.ServiceId.Should().NotBe(service2.ServiceId);
        service1.ProviderId.Should().Be(service2.ProviderId);
    }
}
