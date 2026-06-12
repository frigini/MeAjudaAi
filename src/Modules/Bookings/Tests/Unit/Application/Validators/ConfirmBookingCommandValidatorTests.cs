using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class ConfirmBookingCommandValidatorTests
{
    private readonly ConfirmBookingCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_BookingId_Is_Empty()
    {
        var command = new ConfirmBookingCommand(Guid.Empty, false, null, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ConfirmBookingCommand.BookingId));
    }

    [Fact]
    public void Should_Be_Valid_When_BookingId_Is_Provided()
    {
        var command = new ConfirmBookingCommand(Guid.NewGuid(), false, null, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Be_Valid_When_IsSystemAdmin_Is_True()
    {
        var command = new ConfirmBookingCommand(Guid.NewGuid(), true, null, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
