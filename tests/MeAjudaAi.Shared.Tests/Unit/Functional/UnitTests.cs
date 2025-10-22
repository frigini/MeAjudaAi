namespace MeAjudaAi.Shared.Tests.Unit.Functional;

[Trait("Category", "Unit")]
public class UnitTests
{
    [Fact]
    public void Value_ShouldReturnSingletonInstance()
    {
        // Act
        var unit1 = MeAjudaAi.Shared.Functional.Unit.Value;
        var unit2 = MeAjudaAi.Shared.Functional.Unit.Value;

        // Assert
        unit1.Should().Be(unit2);
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // Arrange
        var unit1 = MeAjudaAi.Shared.Functional.Unit.Value;
        var unit2 = MeAjudaAi.Shared.Functional.Unit.Value;

        // Act & Assert
        unit1.Equals(unit2).Should().BeTrue();
        (unit1 == unit2).Should().BeTrue();
        (unit1 != unit2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var unit = MeAjudaAi.Shared.Functional.Unit.Value;

        // Act & Assert
        unit.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var unit = MeAjudaAi.Shared.Functional.Unit.Value;
        var other = "string";

        // Act & Assert
        unit.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentValue()
    {
        // Arrange
        var unit1 = MeAjudaAi.Shared.Functional.Unit.Value;
        var unit2 = MeAjudaAi.Shared.Functional.Unit.Value;

        // Act & Assert
        unit1.GetHashCode().Should().Be(unit2.GetHashCode());
        unit1.GetHashCode().Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnParentheses()
    {
        // Arrange
        var unit = MeAjudaAi.Shared.Functional.Unit.Value;

        // Act
        var result = unit.ToString();

        // Assert
        result.Should().Be("()");
    }

    [Fact]
    public void Constructor_ShouldCreateUnitInstance()
    {
        // Act
        var unit = new MeAjudaAi.Shared.Functional.Unit();

        // Assert
        unit.Should().NotBeNull();
        unit.Equals(MeAjudaAi.Shared.Functional.Unit.Value).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquality_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var unit1 = MeAjudaAi.Shared.Functional.Unit.Value;
        var unit2 = new MeAjudaAi.Shared.Functional.Unit();

        // Act & Assert
        (unit1 == unit2).Should().BeTrue();
    }

    [Fact]
    public void OperatorInequality_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var unit1 = MeAjudaAi.Shared.Functional.Unit.Value;
        var unit2 = new MeAjudaAi.Shared.Functional.Unit();

        // Act & Assert
        (unit1 != unit2).Should().BeFalse();
    }
}
