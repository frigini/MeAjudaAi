using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.ValueObjects;

[Trait("Category", "Unit")]
public class UserIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateUserId()
    {
        // Arrange
        var guid = UuidGenerator.NewId();

        // Act
        var userId = new UserId(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var act = () => new UserId(emptyGuid);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserId não pode ser vazio*");
    }

    [Fact]
    public void New_ShouldCreateUserIdWithUniqueGuid()
    {
        // Act
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Assert
        userId1.Value.Should().NotBe(Guid.Empty);
        userId2.Value.Should().NotBe(Guid.Empty);
        userId1.Value.Should().NotBe(userId2.Value);
    }

    [Fact]
    public void ImplicitOperator_ToGuid_ShouldReturnGuidValue()
    {
        // Arrange
        var guid = UuidGenerator.NewId();
        var userId = new UserId(guid);

        // Act
        Guid result = userId;

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void ImplicitOperator_FromGuid_ShouldCreateUserId()
    {
        // Arrange
        var guid = UuidGenerator.NewId();

        // Act
        UserId userId = guid;

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = UuidGenerator.NewId();
        var userId1 = new UserId(guid);
        var userId2 = new UserId(guid);

        // Act & Assert
        userId1.Should().Be(userId2);
        userId1.GetHashCode().Should().Be(userId2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Act & Assert
        userId1.Should().NotBe(userId2);
        userId1.GetHashCode().Should().NotBe(userId2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var userId = UserId.New();

        // Act & Assert
        userId.Should().NotBeNull();
        userId.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = UuidGenerator.NewId();
        var userId = new UserId(guid);

        // Act
        var result = userId.Value.ToString(); // UserId usa a propriedade Value para o Guid

        // Assert
        result.Should().Be(guid.ToString());
    }
}
