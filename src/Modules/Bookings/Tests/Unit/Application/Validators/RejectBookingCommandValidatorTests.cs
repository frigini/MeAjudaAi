using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class RejectBookingCommandValidatorTests
{
    private readonly RejectBookingCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Reason_Is_Empty()
    {
        // Arrange
        var command = new RejectBookingCommand(Guid.NewGuid(), string.Empty, false, null, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RejectBookingCommand.Reason));
    }

    [Fact]
    public void Should_Have_Error_When_Reason_Exceeds_Length()
    {
        // Arrange
        var command = new RejectBookingCommand(Guid.NewGuid(), new string('a', 501), false, null, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RejectBookingCommand.Reason));
    }

    [Fact]
    public void Should_Not_Have_Error_When_Reason_Is_Valid()
    {
        // Arrange
        var command = new RejectBookingCommand(Guid.NewGuid(), "Valid reason", false, null, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
