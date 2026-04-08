using FluentAssertions;
using MeAjudaAi.Shared.Exceptions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
public class UnprocessableEntityExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange & Act
        var ex = new UnprocessableEntityException("dados inválidos");
        
        // Assert
        ex.Message.Should().Be("dados inválidos");
    }

    [Fact]
    public void Constructor_Default_ShouldInheritFromException()
    {
        // Assert
        typeof(UnprocessableEntityException).Should().BeDerivedFrom<Exception>();
    }
}
