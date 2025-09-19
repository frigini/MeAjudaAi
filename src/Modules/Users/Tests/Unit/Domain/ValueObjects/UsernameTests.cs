using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.ValueObjects;

public class UsernameTests
{
    [Theory]
    [InlineData("validuser")]
    [InlineData("user123")]
    [InlineData("user_name")]
    [InlineData("user-name")]
    [InlineData("user.name")]
    [InlineData("123")]
    [InlineData("a1b")]
    public void Constructor_WithValidUsername_ShouldCreateUsername(string validUsername)
    {
        // Act
        var username = new Username(validUsername);

        // Assert
        username.Value.Should().Be(validUsername.ToLowerInvariant());
    }

    [Fact]
    public void Constructor_WithExactly30Characters_ShouldCreateUsername()
    {
        // Arrange
        var thirtyCharUsername = "a".PadRight(30, '1'); // Exactly 30 characters

        // Act
        var username = new Username(thirtyCharUsername);

        // Assert
        username.Value.Should().Be(thirtyCharUsername.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithNullOrWhitespace_ShouldThrowArgumentException(string? invalidUsername)
    {
        // Act & Assert
        var act = () => new Username(invalidUsername!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be empty*");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void Constructor_WithTooShortUsername_ShouldThrowArgumentException(string shortUsername)
    {
        // Act & Assert
        var act = () => new Username(shortUsername);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username must be at least 3 characters*");
    }

    [Fact]
    public void Constructor_WithTooLongUsername_ShouldThrowArgumentException()
    {
        // Arrange
        var longUsername = new string('a', 31); // 31 characters

        // Act & Assert
        var act = () => new Username(longUsername);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot exceed 30 characters*");
    }

    [Theory]
    [InlineData("user name")] // Space
    [InlineData("user@name")] // Special character
    [InlineData("user#name")] // Special character
    [InlineData("user$name")] // Special character
    [InlineData("user%name")] // Special character
    [InlineData("user&name")] // Special character
    [InlineData("user+name")] // Special character
    [InlineData("user=name")] // Special character
    [InlineData("user!name")] // Special character
    [InlineData("user?name")] // Special character
    [InlineData("user/name")] // Special character
    [InlineData("user\\name")] // Special character
    [InlineData("user|name")] // Special character
    [InlineData("user<name")] // Special character
    [InlineData("user>name")] // Special character
    [InlineData("user:name")] // Special character
    [InlineData("user;name")] // Special character
    [InlineData("user'name")] // Special character
    [InlineData("user\"name")] // Special character
    [InlineData("user[name")] // Special character
    [InlineData("user]name")] // Special character
    [InlineData("user{name")] // Special character
    [InlineData("user}name")] // Special character
    [InlineData("user`name")] // Special character
    [InlineData("user~name")] // Special character
    public void Constructor_WithInvalidCharacters_ShouldThrowArgumentException(string invalidUsername)
    {
        // Act & Assert
        var act = () => new Username(invalidUsername);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username contains invalid characters*");
    }

    [Fact]
    public void Constructor_ShouldConvertToLowerCase()
    {
        // Arrange
        var upperCaseUsername = "TESTUSER";

        // Act
        var username = new Username(upperCaseUsername);

        // Assert
        username.Value.Should().Be("testuser");
    }

    [Fact]
    public void ImplicitOperator_ToString_ShouldReturnUsernameValue()
    {
        // Arrange
        var usernameValue = "testuser";
        var username = new Username(usernameValue);

        // Act
        string result = username;

        // Assert
        result.Should().Be(usernameValue);
    }

    [Fact]
    public void ImplicitOperator_FromString_ShouldCreateUsername()
    {
        // Arrange
        var usernameValue = "testuser";

        // Act
        Username username = usernameValue;

        // Assert
        username.Value.Should().Be(usernameValue);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var usernameValue = "testuser";
        var username1 = new Username(usernameValue);
        var username2 = new Username(usernameValue);

        // Act & Assert
        username1.Should().Be(username2);
        username1.GetHashCode().Should().Be(username2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentCasing_ShouldReturnTrue()
    {
        // Arrange
        var username1 = new Username("TESTUSER");
        var username2 = new Username("testuser");

        // Act & Assert
        username1.Should().Be(username2);
        username1.GetHashCode().Should().Be(username2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var username1 = new Username("testuser1");
        var username2 = new Username("testuser2");

        // Act & Assert
        username1.Should().NotBe(username2);
        username1.GetHashCode().Should().NotBe(username2.GetHashCode());
    }
}