using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

public class NotFoundExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithEntityNameAndId_ShouldSetProperties()
    {
        // Arrange
        var entityName = "User";
        var entityId = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Be($"{entityName} with id {entityId} was not found");
    }

    [Fact]
    public void Constructor_WithNumericId_ShouldSetProperties()
    {
        // Arrange
        var entityName = "Order";
        var entityId = 12345;

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain(entityId.ToString());
    }

    [Fact]
    public void Constructor_WithStringId_ShouldSetProperties()
    {
        // Arrange
        var entityName = "Product";
        var entityId = "PROD-001";

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain(entityId);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void NotFoundException_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new NotFoundException("TestEntity", 1);

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void NotFoundException_ShouldBeThrowable()
    {
        // Arrange
        var entityName = "Customer";
        var entityId = Guid.NewGuid();

        // Act
        Action act = () => throw new NotFoundException(entityName, entityId);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage($"*{entityName}*{entityId}*");
    }

    #endregion

    #region Message Format Tests

    [Fact]
    public void Message_ShouldFollowExpectedFormat()
    {
        // Arrange
        var entityName = "Invoice";
        var entityId = 999;

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.Message.Should().StartWith("Invoice with id 999");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public void Message_WithGuidId_ShouldIncludeFullGuid()
    {
        // Arrange
        var entityName = "Document";
        var entityId = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.Message.Should().Contain("12345678-1234-1234-1234-123456789abc");
    }

    #endregion

    #region Property Immutability Tests

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var exception = new NotFoundException("TestEntity", 1);

        // Assert
        exception.EntityName.Should().NotBeNull();
        exception.EntityId.Should().NotBeNull();

        // Propriedades não devem ter setters públicos (permite private/init)
        var entityNameProperty = typeof(NotFoundException).GetProperty(nameof(NotFoundException.EntityName));
        var entityIdProperty = typeof(NotFoundException).GetProperty(nameof(NotFoundException.EntityId));

        entityNameProperty.Should().NotBeNull();
        entityIdProperty.Should().NotBeNull();

        // Verifica que o setter é nulo ou não é público
        (entityNameProperty!.SetMethod == null || !entityNameProperty.SetMethod.IsPublic).Should().BeTrue();
        (entityIdProperty!.SetMethod == null || !entityIdProperty.SetMethod.IsPublic).Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithEmptyEntityName_ShouldStillCreateException()
    {
        // Arrange
        var entityName = "";
        var entityId = 1;

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().BeEmpty();
        exception.Message.Should().Contain("with id 1");
    }

    [Fact]
    public void Constructor_WithNullEntityId_ShouldHandleGracefully()
    {
        // Arrange
        var entityName = "Entity";

        // Act
#pragma warning disable CS8600, CS8604 // Null handling - testing edge case where null is passed
        object entityId = null;
        var exception = new NotFoundException(entityName, entityId);
#pragma warning restore CS8600, CS8604

        // Assert
        // Nota: Isto documenta que EntityId pode ser nulo em tempo de execução apesar da assinatura não anulável
        exception.EntityId.Should().BeNull();
    }

    #endregion
}
