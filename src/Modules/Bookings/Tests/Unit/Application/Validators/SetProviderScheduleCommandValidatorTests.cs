using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class SetProviderScheduleCommandValidatorTests
{
    private readonly SetProviderScheduleCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Empty()
    {
        var command = new SetProviderScheduleCommand(Guid.Empty, new List<AvailabilityDto>(), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleCommand.ProviderId));
    }

    [Fact]
    public void Should_Have_Error_When_Availabilities_Is_Null()
    {
        var command = new SetProviderScheduleCommand(Guid.NewGuid(), null!, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleCommand.Availabilities));
    }

    [Fact]
    public void Should_Have_Error_When_Availabilities_Is_Empty()
    {
        var command = new SetProviderScheduleCommand(Guid.NewGuid(), new List<AvailabilityDto>(), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleCommand.Availabilities));
    }

    [Fact]
    public void Should_Be_Valid_When_ProviderId_Is_Provided_And_Availabilities_Not_Empty()
    {
        var command = new SetProviderScheduleCommand(Guid.NewGuid(), new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(3))
            })
        }, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
