using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Events.ServiceCategory;

public class ServiceCategoryActivatedDomainEventTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldCreateEventWithCategoryId()
    {
        // Arrange
        var categoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCategoryActivatedDomainEvent(categoryId);

        // Assert
        domainEvent.CategoryId.Should().Be(categoryId);
        domainEvent.AggregateId.Should().Be(categoryId.Value);
        domainEvent.Version.Should().Be(1);
        domainEvent.EventType.Should().Be("ServiceCategoryActivatedDomainEvent");
        domainEvent.Id.Should().NotBeEmpty();
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldGenerateUniqueIdForEachInstance()
    {
        // Arrange
        var categoryId = ServiceCategoryId.New();

        // Act
        var event1 = new ServiceCategoryActivatedDomainEvent(categoryId);
        var event2 = new ServiceCategoryActivatedDomainEvent(categoryId);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }
}
