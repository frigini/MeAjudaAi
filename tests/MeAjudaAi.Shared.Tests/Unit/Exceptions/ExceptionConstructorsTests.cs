using MeAjudaAi.Shared.Exceptions;
using FluentAssertions;
using Xunit;
using FluentValidation.Results;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
public class ExceptionConstructorsTests
{
    [Fact]
    public void BusinessRuleException_Constructor_ShouldSetProperties()
    {
        var ruleName = "Rule001";
        var message = "Business rule violated";
        var ex = new BusinessRuleException(ruleName, message);

        ex.Message.Should().Be(message);
        ex.RuleName.Should().Be(ruleName);
    }

    [Fact]
    public void NotFoundException_Constructor_ShouldSetProperties()
    {
        var name = "User";
        var key = "123";
        var ex = new NotFoundException(name, key);

        ex.Message.Should().Contain(name);
        ex.Message.Should().Contain(key);
    }

    [Fact]
    public void ValidationException_Constructor_ShouldSetErrors()
    {
        var failures = new List<ValidationFailure>
        {
            new("Prop1", "Error1")
        };
        var ex = new MeAjudaAi.Shared.Exceptions.ValidationException(failures);

        ex.Errors.Should().BeEquivalentTo(failures);
        ex.Message.Should().Be("One or more validation failures have occurred.");
    }
    
    [Fact]
    public void ValidationException_DefaultConstructor_ShouldInitializeEmptyErrors()
    {
        var ex = new MeAjudaAi.Shared.Exceptions.ValidationException();
        ex.Errors.Should().NotBeNull();
        ex.Errors.Should().BeEmpty();
    }
}
