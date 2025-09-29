using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.ValueObjects;

public class UserProfileTests
{
    [Fact]
    public void UserProfile_WithValidNames_ShouldCreateSuccessfully()
    {
        // Arrange
        const string firstName = "João";
        const string lastName = "Silva";

        // Act
        var userProfile = new UserProfile(firstName, lastName);

        // Assert
        userProfile.FirstName.Should().Be(firstName);
        userProfile.LastName.Should().Be(lastName);
        userProfile.PhoneNumber.Should().BeNull();
        userProfile.FullName.Should().Be("João Silva");
    }

    [Fact]
    public void UserProfile_WithValidNamesAndPhoneNumber_ShouldCreateSuccessfully()
    {
        // Arrange
        const string firstName = "João";
        const string lastName = "Silva";
        var phoneNumber = new PhoneNumber("(11) 99999-9999");

        // Act
        var userProfile = new UserProfile(firstName, lastName, phoneNumber);

        // Assert
        userProfile.FirstName.Should().Be(firstName);
        userProfile.LastName.Should().Be(lastName);
        userProfile.PhoneNumber.Should().Be(phoneNumber);
        userProfile.FullName.Should().Be("João Silva");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UserProfile_WithInvalidFirstName_ShouldThrowArgumentException(string? invalidFirstName)
    {
        // Arrange
        const string lastName = "Silva";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new UserProfile(invalidFirstName!, lastName));
        exception.Message.Should().Be("First name cannot be empty or whitespace");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UserProfile_WithInvalidLastName_ShouldThrowArgumentException(string? invalidLastName)
    {
        // Arrange
        const string firstName = "João";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new UserProfile(firstName, invalidLastName!));
        exception.Message.Should().Be("Last name cannot be empty or whitespace");
    }

    [Fact]
    public void UserProfile_WithWhitespaceInNames_ShouldTrimNames()
    {
        // Arrange
        const string firstName = "  João  ";
        const string lastName = "  Silva  ";

        // Act
        var userProfile = new UserProfile(firstName, lastName);

        // Assert
        userProfile.FirstName.Should().Be("João");
        userProfile.LastName.Should().Be("Silva");
        userProfile.FullName.Should().Be("João Silva");
    }

    [Fact]
    public void FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        const string firstName = "Maria";
        const string lastName = "Santos";
        var userProfile = new UserProfile(firstName, lastName);

        // Act
        var fullName = userProfile.FullName;

        // Assert
        fullName.Should().Be("Maria Santos");
    }

    [Fact]
    public void UserProfiles_WithSameData_ShouldBeEqual()
    {
        // Arrange
        const string firstName = "João";
        const string lastName = "Silva";
        var phoneNumber = new PhoneNumber("(11) 99999-9999");
        
        var userProfile1 = new UserProfile(firstName, lastName, phoneNumber);
        var userProfile2 = new UserProfile(firstName, lastName, phoneNumber);

        // Act & Assert
        userProfile1.Should().Be(userProfile2);
        userProfile1.GetHashCode().Should().Be(userProfile2.GetHashCode());
    }

    [Fact]
    public void UserProfiles_WithSameDataButNoPhoneNumber_ShouldBeEqual()
    {
        // Arrange
        const string firstName = "João";
        const string lastName = "Silva";
        
        var userProfile1 = new UserProfile(firstName, lastName);
        var userProfile2 = new UserProfile(firstName, lastName);

        // Act & Assert
        userProfile1.Should().Be(userProfile2);
        userProfile1.GetHashCode().Should().Be(userProfile2.GetHashCode());
    }

    [Fact]
    public void UserProfiles_WithDifferentFirstNames_ShouldNotBeEqual()
    {
        // Arrange
        var userProfile1 = new UserProfile("João", "Silva");
        var userProfile2 = new UserProfile("Maria", "Silva");

        // Act & Assert
        userProfile1.Should().NotBe(userProfile2);
    }

    [Fact]
    public void UserProfiles_WithDifferentLastNames_ShouldNotBeEqual()
    {
        // Arrange
        var userProfile1 = new UserProfile("João", "Silva");
        var userProfile2 = new UserProfile("João", "Santos");

        // Act & Assert
        userProfile1.Should().NotBe(userProfile2);
    }

    [Fact]
    public void UserProfiles_WithDifferentPhoneNumbers_ShouldNotBeEqual()
    {
        // Arrange
        const string firstName = "João";
        const string lastName = "Silva";
        var phoneNumber1 = new PhoneNumber("(11) 99999-9999");
        var phoneNumber2 = new PhoneNumber("(11) 88888-8888");
        
        var userProfile1 = new UserProfile(firstName, lastName, phoneNumber1);
        var userProfile2 = new UserProfile(firstName, lastName, phoneNumber2);

        // Act & Assert
        userProfile1.Should().NotBe(userProfile2);
    }

    [Fact]
    public void UserProfiles_OneWithPhoneNumberOneWithout_ShouldNotBeEqual()
    {
        // Arrange
        const string firstName = "João";
        const string lastName = "Silva";
        var phoneNumber = new PhoneNumber("(11) 99999-9999");
        
        var userProfile1 = new UserProfile(firstName, lastName, phoneNumber);
        var userProfile2 = new UserProfile(firstName, lastName);

        // Act & Assert
        userProfile1.Should().NotBe(userProfile2);
    }

    [Fact]
    public void UserProfile_ComparedWithNull_ShouldNotBeEqual()
    {
        // Arrange
        var userProfile = new UserProfile("João", "Silva");

        // Act & Assert
        userProfile.Should().NotBeNull();
        userProfile.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void UserProfile_ComparedWithDifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var userProfile = new UserProfile("João", "Silva");
        var differentTypeObject = "not a user profile";

        // Act & Assert
        userProfile.Equals(differentTypeObject).Should().BeFalse();
    }

    [Fact]
    public void UserProfile_WithComplexNames_ShouldHandleCorrectly()
    {
        // Arrange
        const string firstName = "Ana Maria";
        const string lastName = "Santos Silva";
        var phoneNumber = new PhoneNumber("(21) 98765-4321", "BR");

        // Act
        var userProfile = new UserProfile(firstName, lastName, phoneNumber);

        // Assert
        userProfile.FirstName.Should().Be(firstName);
        userProfile.LastName.Should().Be(lastName);
        userProfile.FullName.Should().Be("Ana Maria Santos Silva");
        userProfile.PhoneNumber.Should().Be(phoneNumber);
    }
}