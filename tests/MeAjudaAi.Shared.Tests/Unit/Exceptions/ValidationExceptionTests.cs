using FluentAssertions;
using FluentValidation.Results;
using ValidationException = MeAjudaAi.Shared.Exceptions.ValidationException;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

/// <summary>
/// Testes para ValidationException - exceção customizada para erros de validação
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Exceptions")]
public class ValidationExceptionTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateExceptionWithDefaultMessage()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Message.Should().Be("One or more validation failures have occurred.");
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidationFailures_ShouldStoreErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("PropertyName1", "Error message 1"),
            new("PropertyName2", "Error message 2"),
            new("PropertyName3", "Error message 3")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Message.Should().Be("One or more validation failures have occurred.");
        exception.Errors.Should().HaveCount(3);
        exception.Errors.Should().BeEquivalentTo(failures);
    }

    [Fact]
    public void Constructor_WithEmptyFailures_ShouldCreateExceptionWithEmptyErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>();

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Errors_ShouldBeEnumerable()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Field1", "Error 1"),
            new("Field2", "Error 2")
        };
        var exception = new ValidationException(failures);

        // Act
        var errorList = exception.Errors.ToList();

        // Assert
        errorList.Should().HaveCount(2);
        errorList[0].PropertyName.Should().Be("Field1");
        errorList[0].ErrorMessage.Should().Be("Error 1");
        errorList[1].PropertyName.Should().Be("Field2");
        errorList[1].ErrorMessage.Should().Be("Error 2");
    }

    [Fact]
    public void ValidationException_ShouldInheritFromException()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_WithValidationFailures_ShouldPreserveFailureDetails()
    {
        // Arrange
        var failure = new ValidationFailure("Email", "Email is required")
        {
            ErrorCode = "EmailRequired",
            AttemptedValue = ""
        };
        var failures = new List<ValidationFailure> { failure };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        var storedFailure = exception.Errors.First();
        storedFailure.PropertyName.Should().Be("Email");
        storedFailure.ErrorMessage.Should().Be("Email is required");
        storedFailure.ErrorCode.Should().Be("EmailRequired");
        storedFailure.AttemptedValue.Should().Be("");
    }

    [Fact]
    public void Errors_WithMultipleFailuresOnSameProperty_ShouldPreserveAll()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Password", "Password is required"),
            new("Password", "Password must be at least 8 characters"),
            new("Password", "Password must contain a number")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(3);
        exception.Errors.Where(e => e.PropertyName == "Password").Should().HaveCount(3);
    }

    [Fact]
    public void DefaultConstructor_ShouldCreateEmptyErrorsCollection()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Errors.Should().BeEmpty();
        exception.Errors.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullFailure_ShouldHandleGracefully()
    {
        // Arrange
        var failures = new List<ValidationFailure> { null! };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().BeNull();
    }
}
