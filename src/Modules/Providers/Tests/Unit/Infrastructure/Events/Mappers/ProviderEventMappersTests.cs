using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Mappers;

[Trait("Category", "Unit")]
public class ProviderEventMappersTests
{
    [Fact]
    public void ToIntegrationEvent_FromProviderRegisteredDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderRegisteredDomainEvent(
            providerId, 1, userId, "Test Provider", EProviderType.Individual, "test@test.com", "test-provider");

        // Act
        var result = domainEvent.ToIntegrationEvent();

        // Assert
        result.Source.Should().Be("Providers");
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("Test Provider");
        result.ProviderType.Should().Be("Individual");
        result.Email.Should().Be("test@test.com");
        result.Slug.Should().Be("test-provider");
        result.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToIntegrationEvent_FromProviderDeletedDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderDeletedDomainEvent(providerId, 1, "Test Provider", userId.ToString());

        // Act
        var result = domainEvent.ToIntegrationEvent(userId);

        // Assert
        result.Source.Should().Be("Providers");
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("Test Provider");
        result.Reason.Should().Be("Provider deleted");
        result.DeletedBy.Should().Be(userId.ToString());
        result.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToIntegrationEvent_FromProviderVerificationStatusUpdatedDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId, 1, EVerificationStatus.Pending, EVerificationStatus.Verified, userId.ToString());

        // Act
        var result = domainEvent.ToIntegrationEvent(userId, "Test Provider");

        // Assert
        result.Source.Should().Be("Providers");
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("Test Provider");
        result.PreviousStatus.Should().Be("Pending");
        result.NewStatus.Should().Be("Verified");
        result.UpdatedBy.Should().Be(userId.ToString());
    }

    [Fact]
    public void ToIntegrationEvent_FromProviderProfileUpdatedDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderProfileUpdatedDomainEvent(
            providerId, 1, "New Name", "new@test.com", "new-name", userId.ToString(), new string[] { "Name", "Email" });
        string[] fields = { "Name", "Email" };

        // Act
        var result = domainEvent.ToIntegrationEvent(userId, fields);

        // Assert
        result.Source.Should().Be("Providers");
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("New Name");
        result.NewEmail.Should().Be("new@test.com");
        result.Slug.Should().Be("new-name");
        result.UpdatedFields.Should().BeEquivalentTo(fields);
        result.UpdatedBy.Should().Be(userId.ToString());
    }

    [Fact]
    public void ToIntegrationEvent_FromProviderAwaitingVerificationDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderAwaitingVerificationDomainEvent(providerId, 1, userId, "Test", userId.ToString());

        // Act
        var result = domainEvent.ToIntegrationEvent();

        // Assert
        result.Source.Should().Be("Providers");
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("Test");
        result.UpdatedBy.Should().Be(userId.ToString());
        result.TransitionedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToIntegrationEvent_FromProviderActivatedDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId, 1, userId, "Test", userId.ToString());

        // Act
        var result = domainEvent.ToIntegrationEvent();

        // Assert
        result.Source.Should().Be("Providers");
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("Test");
        result.ActivatedBy.Should().Be(userId.ToString());
        result.ActivatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
