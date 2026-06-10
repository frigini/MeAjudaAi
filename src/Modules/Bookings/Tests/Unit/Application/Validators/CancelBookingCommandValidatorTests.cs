using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class CancelBookingCommandValidatorTests
{
    private readonly CancelBookingCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Reason_Is_Empty()
    {
        // Arrange
        var command = new CancelBookingCommand(Guid.NewGuid(), string.Empty, false, null, null, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CancelBookingCommand.Reason));
    }

    [Fact]
    public void Should_Have_Error_When_Reason_Exceeds_Length()
    {
        // Arrange
        var command = new CancelBookingCommand(Guid.NewGuid(), new string('a', 501), false, null, null, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CancelBookingCommand.Reason));
    }

    [Fact]
    public void Should_Not_Have_Error_When_Reason_Is_Valid()
    {
        // Arrange
        var command = new CancelBookingCommand(Guid.NewGuid(), "Valid reason", false, null, null, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
