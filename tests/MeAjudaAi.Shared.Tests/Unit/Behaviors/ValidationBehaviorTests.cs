using FluentAssertions;
using FluentValidation;
using MeAjudaAi.Shared.Behaviors;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Mediator;
using Moq;
using FVValidationFailure = FluentValidation.Results.ValidationFailure;
using FVValidationResult = FluentValidation.Results.ValidationResult;

namespace MeAjudaAi.Shared.Tests.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    // Test command
    public record TestCommand(Guid CorrelationId, string Name, int Age) : ICommand<string>;

    // Test validator
    private class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Age).GreaterThan(0).WithMessage("Age must be positive");
        }
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "John", 30);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("success");
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallNext()
    {
        // Arrange
        var validators = new List<IValidator<TestCommand>> { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "John", 30);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("success");
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
    {
        // Arrange
        var validators = new List<IValidator<TestCommand>> { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "", 0); // Invalid: empty name and zero age
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Shared.Exceptions.ValidationException>(() =>
            behavior.Handle(command, next, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain(e => e.PropertyName == "Name");
        exception.Errors.Should().Contain(e => e.PropertyName == "Age");
    }

    [Fact]
    public async Task Handle_WithPartiallyInvalidCommand_ShouldThrowValidationExceptionWithOnlyFailedRules()
    {
        // Arrange
        var validators = new List<IValidator<TestCommand>> { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "John", 0); // Invalid: only age is wrong
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Shared.Exceptions.ValidationException>(() =>
            behavior.Handle(command, next, CancellationToken.None));

        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().Contain(e => e.PropertyName == "Age" && e.ErrorMessage == "Age must be positive");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAllValidators()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestCommand>>();
        var validator2Mock = new Mock<IValidator<TestCommand>>();

        validator1Mock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVValidationResult());

        validator2Mock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVValidationResult());

        var validators = new List<IValidator<TestCommand>> { validator1Mock.Object, validator2Mock.Object };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "John", 30);
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        validator1Mock.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator2Mock.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleValidatorsHavingErrors_ShouldCombineAllErrors()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestCommand>>();
        var validator2Mock = new Mock<IValidator<TestCommand>>();

        validator1Mock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVValidationResult(new[] { new FVValidationFailure("Name", "Name is too short") }));

        validator2Mock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVValidationResult(new[] { new FVValidationFailure("Age", "Age is invalid") }));

        var validators = new List<IValidator<TestCommand>> { validator1Mock.Object, validator2Mock.Object };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "J", 30);
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Shared.Exceptions.ValidationException>(() =>
            behavior.Handle(command, next, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain(e => e.PropertyName == "Name");
        exception.Errors.Should().Contain(e => e.PropertyName == "Age");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToValidator()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        var validatorMock = new Mock<IValidator<TestCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVValidationResult());

        var validators = new List<IValidator<TestCommand>> { validatorMock.Object };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "John", 30);
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act
        await behavior.Handle(command, next, cts.Token);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(
            It.Is<IValidationContext>(ctx => ctx.InstanceToValidate.Equals(command)),
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldNotCallNext()
    {
        // Arrange
        var validators = new List<IValidator<TestCommand>> { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "", 0); // Invalid
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("success");
        };

        // Act & Assert
        await Assert.ThrowsAsync<Shared.Exceptions.ValidationException>(() =>
            behavior.Handle(command, next, CancellationToken.None));

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithValidationContext_ShouldPassCorrectCommand()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVValidationResult());

        var validators = new List<IValidator<TestCommand>> { validatorMock.Object };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);

        var command = new TestCommand(Guid.NewGuid(), "John", 30);
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(
            It.Is<IValidationContext>(ctx => ctx.InstanceToValidate.Equals(command)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
