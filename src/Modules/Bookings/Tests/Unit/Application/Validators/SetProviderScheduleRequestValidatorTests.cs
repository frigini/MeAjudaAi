using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Validators;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Validators;

public class SetProviderScheduleRequestValidatorTests
{
    private readonly SetProviderScheduleRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Availabilities_Is_Null()
    {
        // Arrange
        var request = new SetProviderScheduleRequestDto(Guid.NewGuid(), null!);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleRequestDto.Availabilities));
    }

    [Fact]
    public void Should_Have_Error_When_DayOfWeek_Is_Duplicated()
    {
        // Arrange
        var baseDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var request = new SetProviderScheduleRequestDto(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, [new AvailableSlotDto(baseDate, baseDate.AddHours(1))]),
            new AvailabilityDto(DayOfWeek.Monday, [new AvailableSlotDto(baseDate, baseDate.AddHours(1))])
        ]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleRequestDto.Availabilities));
    }

    [Fact]
    public void Should_Have_Error_When_Slots_Overlap()
    {
        // Arrange
        var baseDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var request = new SetProviderScheduleRequestDto(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, 
            [
                new AvailableSlotDto(baseDate.AddHours(8), baseDate.AddHours(10)),
                new AvailableSlotDto(baseDate.AddHours(9), baseDate.AddHours(11))
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
        var baseDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var request = new SetProviderScheduleRequestDto(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, 
            [
                new AvailableSlotDto(baseDate.AddHours(10), baseDate.AddHours(8))
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
        var baseDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var request = new SetProviderScheduleRequestDto(Guid.NewGuid(), 
        [
            new AvailabilityDto(DayOfWeek.Monday, 
            [
                new AvailableSlotDto(baseDate.AddHours(8), baseDate.AddHours(10)),
                new AvailableSlotDto(baseDate.AddHours(11), baseDate.AddHours(13))
            ])
        ]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
