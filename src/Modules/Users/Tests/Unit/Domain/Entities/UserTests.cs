using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Exceptions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Entities;

public class UserTests
{
    // Cria um mock do provedor de data/hora
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

    [Fact]
    public void MarkAsDeleted_WhenUserIsNotDeleted_ShouldMarkAsDeletedAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var dateTimeProvider = CreateMockDateTimeProvider(new DateTime(2023, 10, 15, 10, 30, 0, DateTimeKind.Utc));

        // Act
        user.MarkAsDeleted(dateTimeProvider);

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().Be(new DateTime(2023, 10, 15, 10, 30, 0, DateTimeKind.Utc));
        user.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "UserDeletedDomainEvent");
    }

    [Fact]
    public void MarkAsDeleted_WhenUserIsAlreadyDeleted_ShouldNotRaiseAdditionalEvents()
    {
        // Arrange
        var user = CreateTestUser();
        var dateTimeProvider = CreateMockDateTimeProvider();
        user.MarkAsDeleted(dateTimeProvider);
        var initialEventCount = user.DomainEvents.Count;

        // Act
        user.MarkAsDeleted(dateTimeProvider);

        // Assert
        user.DomainEvents.Should().HaveCount(initialEventCount);
    }

    [Fact]
    public void GetFullName_WithBothNames_ShouldReturnCombinedName()
    {
        // Arrange
        var user = CreateTestUser("Jane", "Smith");

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void GetFullName_WithOnlyFirstName_ShouldReturnTrimmedName()
    {
        // Arrange
        var user = CreateTestUser("Jane", "");

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("Jane");
    }

    [Fact]
    public void GetFullName_WithOnlyLastName_ShouldReturnTrimmedName()
    {
        // Arrange
        var user = CreateTestUser("", "Smith");

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("Smith");
    }

    [Fact]
    public void ChangeEmail_WithValidNewEmail_ShouldUpdateEmailAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var oldEmail = user.Email;
        var newEmail = "newemail@example.com";

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.Email.Value.Should().Be(newEmail);
        user.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "UserEmailChangedEvent");
    }

    [Fact]
    public void ChangeEmail_WithSameEmail_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents(); // Limpa evento inicial de registro
        var currentEmail = user.Email.Value;

        // Act
        user.ChangeEmail(currentEmail);

        // Assert
        user.DomainEvents.Should().BeEmpty(); // Nenhum novo evento deve ser disparado
    }

    [Fact]
    public void ChangeEmail_WhenUserIsDeleted_ShouldThrowException()
    {
        // Arrange
        var user = CreateTestUser();
        var dateTimeProvider = CreateMockDateTimeProvider();
        user.MarkAsDeleted(dateTimeProvider);

        // Act & Assert
        var act = () => user.ChangeEmail("newemail@example.com");
        act.Should().Throw<UserDomainException>()
           .WithMessage("*user is deleted*");
    }

    [Fact]
    public void ChangeUsername_WithValidNewUsername_ShouldUpdateUsernameAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var oldUsername = user.Username;
        var newUsername = "newusername";
        var dateTimeProvider = CreateMockDateTimeProvider(new DateTime(2023, 10, 15, 12, 0, 0, DateTimeKind.Utc));

        // Act
        user.ChangeUsername(newUsername, dateTimeProvider);

        // Assert
        user.Username.Value.Should().Be(newUsername);
        user.LastUsernameChangeAt.Should().Be(new DateTime(2023, 10, 15, 12, 0, 0, DateTimeKind.Utc));
        user.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "UserUsernameChangedEvent");
    }

    [Fact]
    public void ChangeUsername_WithSameUsername_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents(); // Limpa evento inicial de registro
        var currentUsername = user.Username.Value;
        var dateTimeProvider = CreateMockDateTimeProvider();

        // Act
        user.ChangeUsername(currentUsername, dateTimeProvider);

        // Assert
        user.DomainEvents.Should().BeEmpty(); // Nenhum novo evento deve ser disparado
    }

    [Fact]
    public void ChangeUsername_WhenUserIsDeleted_ShouldThrowException()
    {
        // Arrange
        var user = CreateTestUser();
        var dateTimeProvider = CreateMockDateTimeProvider();
        user.MarkAsDeleted(dateTimeProvider);

        // Act & Assert
        var act = () => user.ChangeUsername("newusername", dateTimeProvider);
        act.Should().Throw<UserDomainException>()
           .WithMessage("*user is deleted*");
    }

    [Fact]
    public void CanChangeUsername_WhenNoPreviewChange_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var dateTimeProvider = CreateMockDateTimeProvider();

        // Act
        var canChange = user.CanChangeUsername(dateTimeProvider);

        // Assert
        canChange.Should().BeTrue();
    }

    [Fact]
    public void CanChangeUsername_WhenRecentChange_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var changeDate = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var checkDate = new DateTime(2023, 10, 15, 12, 0, 0, DateTimeKind.Utc); // 14 dias depois

        var changeDateProvider = CreateMockDateTimeProvider(changeDate);
        var checkDateProvider = CreateMockDateTimeProvider(checkDate);

        user.ChangeUsername("newusername", changeDateProvider);

        // Act
        var canChange = user.CanChangeUsername(checkDateProvider, 30);

        // Assert
        canChange.Should().BeFalse();
    }

    [Fact]
    public void CanChangeUsername_WhenSufficientTimeHasPassed_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var changeDate = new DateTime(2023, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        var checkDate = new DateTime(2023, 10, 15, 12, 0, 0, DateTimeKind.Utc); // 44 dias depois

        var changeDateProvider = CreateMockDateTimeProvider(changeDate);
        var checkDateProvider = CreateMockDateTimeProvider(checkDate);

        user.ChangeUsername("newusername", changeDateProvider);

        // Act
        var canChange = user.CanChangeUsername(checkDateProvider, 30);

        // Assert
        canChange.Should().BeTrue();
    }


    // Cria um usuário de teste
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
