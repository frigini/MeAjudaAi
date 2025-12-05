using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

public class UserIdTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = new UserId(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var guid = Guid.Empty;

        // Act
        var act = () => new UserId(guid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("UserId cannot be empty*")
            .WithParameterName("value");
    }

    #endregion

    #region New Method Tests

    [Fact]
    public void New_ShouldCreateInstanceWithUniqueGuid()
    {
        // Act
        var userId = UserId.New();

        // Assert
        userId.Should().NotBeNull();
        userId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_CalledMultipleTimes_ShouldCreateDifferentInstances()
    {
        // Act
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Assert
        userId1.Should().NotBe(userId2);
        userId1.Value.Should().NotBe(userId2.Value);
    }

    #endregion

    #region FromString Method Tests

    [Fact]
    public void FromString_WithValidGuidString_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var userId = UserId.FromString(guidString);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void FromString_WithUpperCaseGuidString_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString().ToUpperInvariant();

        // Act
        var userId = UserId.FromString(guidString);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void FromString_WithNullString_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? guidString = null;

        // Act
        var act = () => UserId.FromString(guidString!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("GUID string cannot be null or empty*")
            .WithParameterName("guidString");
    }

    [Fact]
    public void FromString_WithEmptyString_ShouldThrowArgumentNullException()
    {
        // Arrange
        var guidString = string.Empty;

        // Act
        var act = () => UserId.FromString(guidString);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("GUID string cannot be null or empty*")
            .WithParameterName("guidString");
    }

    [Fact]
    public void FromString_WithWhitespaceString_ShouldThrowArgumentNullException()
    {
        // Arrange
        var guidString = "   ";

        // Act
        var act = () => UserId.FromString(guidString);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("GUID string cannot be null or empty*")
            .WithParameterName("guidString");
    }

    [Fact]
    public void FromString_WithInvalidGuidFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidGuidString = "invalid-guid-string";

        // Act
        var act = () => UserId.FromString(invalidGuidString);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid GUID format: {invalidGuidString}*")
            .WithParameterName("guidString");
    }

    [Fact]
    public void FromString_WithPartialGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var partialGuid = "123e4567-e89b";

        // Act
        var act = () => UserId.FromString(partialGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid GUID format: {partialGuid}*")
            .WithParameterName("guidString");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameGuid_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = new UserId(guid);
        var userId2 = new UserId(guid);

        // Act
        var result = userId1.Equals(userId2);

        // Assert
        result.Should().BeTrue();
        (userId1 == userId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentGuid_ShouldReturnFalse()
    {
        // Arrange
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Act
        var result = userId1.Equals(userId2);

        // Assert
        result.Should().BeFalse();
        (userId1 == userId2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameGuid_ShouldReturnSameHash()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = new UserId(guid);
        var userId2 = new UserId(guid);

        // Act
        var hash1 = userId1.GetHashCode();
        var hash2 = userId2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_UserIdToGuid_ShouldReturnGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId = new UserId(guid);

        // Act
        Guid result = userId;

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_UserIdToGuid_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserId? userId = null;

        // Act
        var act = () =>
        {
            Guid _ = userId!;
        };

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ImplicitConversion_GuidToUserId_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        UserId userId = guid;

        // Assert
        userId.Should().NotBeNull();
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_GuidToUserId_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var guid = Guid.Empty;

        // Act
        var act = () =>
        {
            UserId userId = guid;
            return userId;
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("UserId cannot be empty*");
    }

    [Fact]
    public void ImplicitConversion_StringToUserId_WithValidGuid_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        UserId userId = guidString;

        // Assert
        userId.Should().NotBeNull();
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_StringToUserId_WithInvalidString_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidString = "invalid-guid";

        // Act
        var act = () =>
        {
            UserId userId = invalidString;
            return userId;
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid GUID format: {invalidString}*");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId = new UserId(guid);

        // Act
        var result = userId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    #endregion

    #region Value Object Behavior Tests

    [Fact]
    public void UserIds_WithSameValue_ShouldBeConsideredEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = new UserId(guid);
        var userId2 = new UserId(guid);

        // Act & Assert
        userId1.Should().Be(userId2);
        userId1.Equals((object)userId2).Should().BeTrue();
    }

    [Fact]
    public void UserIds_CanBeUsedInCollections()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var userId1 = new UserId(guid1);
        var userId2 = new UserId(guid2);
        var userId3 = new UserId(guid1); // Same as userId1

        // Act
        var set = new HashSet<UserId> { userId1, userId2, userId3 };

        // Assert
        set.Should().HaveCount(2); // userId1 and userId3 are equal
        set.Should().Contain(userId1);
        set.Should().Contain(userId2);
    }

    [Fact]
    public void UserIds_CanBeUsedAsDictionaryKeys()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = new UserId(guid);
        var userId2 = new UserId(guid); // Same value

        var dictionary = new Dictionary<UserId, string>
        {
            [userId1] = "First entry"
        };

        // Act
        dictionary[userId2] = "Second entry"; // Should replace first

        // Assert
        dictionary.Should().HaveCount(1);
        dictionary[userId1].Should().Be("Second entry");
    }

    #endregion
}
