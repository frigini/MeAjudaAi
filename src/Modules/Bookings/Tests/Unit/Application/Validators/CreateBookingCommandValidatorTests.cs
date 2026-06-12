using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Empty()
    {
        var command = new CreateBookingCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.ProviderId));
    }

    [Fact]
    public void Should_Have_Error_When_ClientId_Is_Empty()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.ClientId));
    }

    [Fact]
    public void Should_Have_Error_When_ServiceId_Is_Empty()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.ServiceId));
    }

    [Fact]
    public void Should_Have_Error_When_End_Is_Before_Start()
    {
        var start = DateTimeOffset.Now.AddHours(2);
        var end = DateTimeOffset.Now.AddHours(1);
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.End));
    }

    [Fact]
    public void Should_Have_Error_When_CorrelationId_Is_Empty()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), Guid.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.CorrelationId));
    }

    [Fact]
    public void Should_Be_Valid_When_All_Fields_Are_Valid()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
