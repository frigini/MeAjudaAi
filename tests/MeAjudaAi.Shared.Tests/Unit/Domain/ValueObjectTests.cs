using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Shared.Tests.Unit.Domain;

[Trait("Category", "Unit")]
[Trait("Component", "Domain")]
public class ValueObjectTests
{
    private sealed class TestValueObject(string name, int age) : ValueObject
    {
        public string Name { get; } = name;
        public int Age { get; } = age;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return Age;
        }
    }

    private sealed class DifferentValueObject(string value) : ValueObject
    {
        public string Value { get; } = value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    #region Equals

    [Fact]
    public void Equals_WithSameValues_ShouldBeTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Alice", 30);

        // Act & Assert
        vo1.Should().Be(vo2);
        vo1.Equals(vo2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldBeFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Bob", 25);

        // Act & Assert
        vo1.Should().NotBe(vo2);
        vo1.Equals(vo2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldBeFalse()
    {
        // Arrange
        var vo = new TestValueObject("Alice", 30);

        // Act & Assert
        vo.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldBeFalse()
    {
        // Arrange
        var vo = new TestValueObject("Alice", 30);

        // Act & Assert
        vo.Equals("not a value object").Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameReference_ShouldBeTrue()
    {
        // Arrange
        var vo = new TestValueObject("Alice", 30);

        // Act & Assert
        vo.Equals(vo).Should().BeTrue();
    }

    #endregion

    #region GetHashCode

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Alice", 30);

        // Act & Assert
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHash()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Bob", 25);

        // Act & Assert
        vo1.GetHashCode().Should().NotBe(vo2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithNullComponent_ShouldNotThrow()
    {
        // Arrange
        var vo = new TestValueObject(null!, 30);

        // Act & Assert
        var act = () => vo.GetHashCode();
        act.Should().NotThrow();
    }

    #endregion

    #region Operators

    [Fact]
    public void OperatorEquals_WithSameValues_ShouldBeTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Alice", 30);

        // Act & Assert
        (vo1 == vo2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WithDifferentValues_ShouldBeFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Bob", 25);

        // Act & Assert
        (vo1 == vo2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithSameValues_ShouldBeFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Alice", 30);

        // Act & Assert
        (vo1 != vo2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithDifferentValues_ShouldBeTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new TestValueObject("Bob", 25);

        // Act & Assert
        (vo1 != vo2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_BothNull_ShouldBeTrue()
    {
        // Arrange
        TestValueObject? vo1 = null;
        TestValueObject? vo2 = null;

        // Act & Assert
        (vo1 == vo2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_LeftNull_ShouldBeFalse()
    {
        // Arrange
        TestValueObject? vo1 = null;
        var vo2 = new TestValueObject("Alice", 30);

        // Act & Assert
        (vo1 == vo2).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_RightNull_ShouldBeFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        TestValueObject? vo2 = null;

        // Act & Assert
        (vo1 == vo2).Should().BeFalse();
    }

    #endregion

    #region Different ValueObject types

    [Fact]
    public void Equals_WithDifferentValueObjectType_ShouldBeFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("Alice", 30);
        var vo2 = new DifferentValueObject("Alice");

        // Act & Assert
        vo1.Equals(vo2).Should().BeFalse();
    }

    #endregion
}
