using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class SetProviderScheduleRequestValidatorTests
{
    private readonly SetProviderScheduleRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Availabilities_Is_Null()
    {
        // Arrange
        var request = new SetProviderScheduleRequest(Guid.NewGuid(), null!);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleRequest.Availabilities));
    }

    [Fact]
    public void Should_Have_Error_When_DayOfWeek_Is_Duplicated()
    {
        // Arrange
        var request = new SetProviderScheduleRequest(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, [new AvailableSlotDto(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1))]),
            new AvailabilityDto(DayOfWeek.Monday, [new AvailableSlotDto(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1))])
        ]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleRequest.Availabilities));
    }

    [Fact]
    public void Should_Have_Error_When_Slots_Overlap()
    {
        // Arrange
        var request = new SetProviderScheduleRequest(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, 
            [
                new AvailableSlotDto(DateTimeOffset.Now.Date.AddHours(8), DateTimeOffset.Now.Date.AddHours(10)),
                new AvailableSlotDto(DateTimeOffset.Now.Date.AddHours(9), DateTimeOffset.Now.Date.AddHours(11))
            ])
        ]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Have_Error_When_End_Before_Start()
    {
        // Arrange
        var request = new SetProviderScheduleRequest(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, 
            [
                new AvailableSlotDto(DateTimeOffset.Now.Date.AddHours(10), DateTimeOffset.Now.Date.AddHours(8))
            ])
        ]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        // Arrange
        var request = new SetProviderScheduleRequest(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, 
            [
                new AvailableSlotDto(DateTimeOffset.Now.Date.AddHours(8), DateTimeOffset.Now.Date.AddHours(10)),
                new AvailableSlotDto(DateTimeOffset.Now.Date.AddHours(11), DateTimeOffset.Now.Date.AddHours(13))
            ])
        ]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
