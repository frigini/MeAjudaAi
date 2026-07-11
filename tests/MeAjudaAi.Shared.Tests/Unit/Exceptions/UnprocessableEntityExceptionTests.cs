using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
[Trait("Component", "Exceptions")]
public class UnprocessableEntityExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "dados inválidos";

        // Act
        var ex = new UnprocessableEntityException(message);

        // Assert
        ex.Message.Should().Be(message);
        ex.EntityName.Should().BeNull();
        ex.Details.Should().BeNull();
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndEntityName_ShouldSetBoth()
    {
        // Arrange
        var message = "Invalid state transition";
        var entityName = "Booking";

        // Act
        var ex = new UnprocessableEntityException(message, entityName);

        // Assert
        ex.Message.Should().Be(message);
        ex.EntityName.Should().Be(entityName);
        ex.Details.Should().BeNull();
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageEntityNameAndDetails_ShouldSetAll()
    {
        // Arrange
        var message = "Invalid state transition";
        var entityName = "Booking";
        var details = new Dictionary<string, object?>
        {
            ["currentState"] = "Pending",
            ["requestedState"] = "Completed"
        };

        // Act
        var ex = new UnprocessableEntityException(message, entityName, details);

        // Assert
        ex.Message.Should().Be(message);
        ex.EntityName.Should().Be(entityName);
        ex.Details.Should().BeEquivalentTo(details);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Processing failed";
        var innerException = new InvalidOperationException("Root cause");

        // Act
        var ex = new UnprocessableEntityException(message, innerException);

        // Assert
        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(innerException);
        ex.EntityName.Should().BeNull();
        ex.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullEntityName_ShouldAllowNull()
    {
        // Arrange & Act
        var ex = new UnprocessableEntityException("msg", (string)null!);

        // Assert
        ex.EntityName.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullDetails_ShouldAllowNull()
    {
        // Arrange & Act
        var ex = new UnprocessableEntityException("msg", "Entity", (Dictionary<string, object?>)null!);

        // Assert
        ex.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyDetails_ShouldStoreEmptyDictionary()
    {
        // Arrange
        var details = new Dictionary<string, object?>();

        // Act
        var ex = new UnprocessableEntityException("msg", "Entity", details);

        // Assert
        ex.Details.Should().BeEmpty();
    }

    [Fact]
    public void ShouldInheritFromException()
    {
        // Arrange & Act
        var ex = new UnprocessableEntityException("msg");

        // Assert
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ShouldBeCatchableAsException()
    {
        // Arrange & Act & Assert
        var act = () => ThrowHelper();
        act.Should().Throw<Exception>()
            .Which.Message.Should().Be("test");
    }

    [Fact]
    public void Details_ShouldBeReadOnly()
    {
        // Arrange
        var details = new Dictionary<string, object?> { ["key"] = "value" };
        var ex = new UnprocessableEntityException("msg", "Entity", details);

        // Act & Assert - Details property has no setter
        var property = typeof(UnprocessableEntityException).GetProperty(nameof(UnprocessableEntityException.Details));
        property!.SetMethod.Should().BeNull("Details should be read-only");
    }

    [Fact]
    public void EntityName_ShouldBeReadOnly()
    {
        // Arrange
        var ex = new UnprocessableEntityException("msg", "Entity");

        // Act & Assert
        var property = typeof(UnprocessableEntityException).GetProperty(nameof(UnprocessableEntityException.EntityName));
        property!.SetMethod.Should().BeNull("EntityName should be read-only");
    }

    private static void ThrowHelper()
    {
        throw new UnprocessableEntityException("test");
    }
}
