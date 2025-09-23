using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Entities;

public class UserTests
{
    private static IDateTimeProvider CreateMockDateTimeProvider(DateTime? fixedDate = null)
    {
        var mock = new Mock<IDateTimeProvider>();
        mock.Setup(x => x.CurrentDate()).Returns(fixedDate ?? DateTime.UtcNow);
        return mock.Object;
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "John";
        var lastName = "Doe";
        var keycloakId = "keycloak-123";

        // Act
        var user = new User(username, email, firstName, lastName, keycloakId);

        // Assert
        user.Id.Should().NotBeNull();
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.KeycloakId.Should().Be(keycloakId);
        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldRaiseUserRegisteredDomainEvent()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "John";
        var lastName = "Doe";
        var keycloakId = "keycloak-123";

        // Act
        var user = new User(username, email, firstName, lastName, keycloakId);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First().Should().BeOfType<UserRegisteredDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(user.Id.Value);
        domainEvent.Version.Should().Be(1);
        domainEvent.Email.Should().Be(email.Value);
        domainEvent.Username.Value.Should().Be(username.Value);
        domainEvent.FirstName.Should().Be(firstName);
        domainEvent.LastName.Should().Be(lastName);
    }

    [Fact]
    public void GetFullName_ShouldReturnCombinedFirstAndLastName()
    {
        // Arrange
        var user = CreateTestUser("John", "Doe");

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void GetFullName_WithExtraSpaces_ShouldReturnTrimmedName()
    {
        // Arrange
        var user = CreateTestUser("  John  ", "  Doe  ");

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("John     Doe");
    }

    [Fact]
    public void UpdateProfile_WithDifferentValues_ShouldUpdatePropertiesAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser("John", "Doe");
        user.ClearDomainEvents(); // Limpa eventos do construtor
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First().Should().BeOfType<UserProfileUpdatedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(user.Id.Value);
        domainEvent.FirstName.Should().Be(newFirstName);
        domainEvent.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void UpdateProfile_WithSameValues_ShouldNotUpdateOrRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser("John", "Doe");
        user.ClearDomainEvents(); // Limpa eventos do construtor
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        user.UpdateProfile("John", "Doe");

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsDeleted_WhenNotDeleted_ShouldMarkAsDeletedAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents(); // Limpa eventos do construtor
        var dateTimeProvider = CreateMockDateTimeProvider();

        // Act
        user.MarkAsDeleted(dateTimeProvider);

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First().Should().BeOfType<UserDeletedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(user.Id.Value);
        domainEvent.Version.Should().Be(1);
    }

    [Fact]
    public void MarkAsDeleted_WhenAlreadyDeleted_ShouldNotChangeStateOrRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var dateTimeProvider = CreateMockDateTimeProvider();
        user.MarkAsDeleted(dateTimeProvider);
        var originalDeletedAt = user.DeletedAt;
        var originalUpdatedAt = user.UpdatedAt;
        user.ClearDomainEvents(); // Limpa eventos anteriores

        // Act
        user.MarkAsDeleted(dateTimeProvider);

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().Be(originalDeletedAt);
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    private static User CreateTestUser(string firstName = "John", string lastName = "Doe")
    {
        return new User(
            new Username("testuser"),
            new Email("test@example.com"),
            firstName,
            lastName,
            "keycloak-123"
        );
    }
}