using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Domain.Entities;

public class ProviderScheduleTests : BaseUnitTest
{
    [Fact]
    public void Create_Should_InitializeWithDefaultTimeZone()
    {
        // Arrange & Act
        var providerId = Guid.NewGuid();
        var schedule = ProviderSchedule.Create(providerId);

        // Assert
        schedule.ProviderId.Should().Be(providerId);
        schedule.TimeZoneId.Should().Be("E. South America Standard Time");
        schedule.Availabilities.Should().BeEmpty();
    }

    [Fact]
    public void SetAvailability_Should_AddOrUpdateDay()
    {
        // Arrange
        var schedule = ProviderSchedule.Create(Guid.NewGuid());
        var slot = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var availability = Availability.Create(DayOfWeek.Monday, [slot]);

        // Act
        schedule.SetAvailability(availability);

        // Assert
        schedule.Availabilities.Should().HaveCount(1);
        schedule.Availabilities[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void IsAvailable_Should_ReturnFalse_When_NoAvailabilityForDay()
    {
        // Arrange
        var schedule = ProviderSchedule.Create(Guid.NewGuid());
        var dateTime = new DateTime(2026, 4, 20, 10, 0, 0); // Segunda-feira
        dateTime.DayOfWeek.Should().Be(DayOfWeek.Monday);
        var duration = TimeSpan.FromHours(1);

        // Act
        var result = schedule.IsAvailable(dateTime, duration);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_Should_ReturnTrue_When_WithinSlot()
    {
        // Arrange
        var schedule = ProviderSchedule.Create(Guid.NewGuid());
        var slot = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        schedule.SetAvailability(Availability.Create(DayOfWeek.Monday, [slot]));
        
        var dateTime = new DateTime(2026, 4, 20, 9, 0, 0); // Segunda, 09:00
        var duration = TimeSpan.FromHours(1);

        // Act
        var result = schedule.IsAvailable(dateTime, duration);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_Should_ReturnFalse_When_DurationIsZeroOrNegative()
    {
        // Arrange
        var schedule = ProviderSchedule.Create(Guid.NewGuid());
        var dateTime = new DateTime(2026, 4, 20, 9, 0, 0); // Segunda-feira
        var slot = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        schedule.SetAvailability(Availability.Create(DayOfWeek.Monday, [slot]));

        // Act & Assert
        schedule.IsAvailable(dateTime, TimeSpan.Zero).Should().BeFalse();
        schedule.IsAvailable(dateTime, TimeSpan.FromHours(-1)).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_Should_ReturnFalse_When_CrossesMidnight()
    {
        // Arrange
        var schedule = ProviderSchedule.Create(Guid.NewGuid());
        // Slot cobre até o final do dia para garantir que falhemos por cruzamento de data
        var slot = TimeSlot.Create(new TimeOnly(22, 0), TimeOnly.MaxValue);
        schedule.SetAvailability(Availability.Create(DayOfWeek.Monday, [slot]));

        var dateTime = new DateTime(2026, 4, 20, 23, 30, 0); // Segunda, 23:30
        var duration = TimeSpan.FromHours(1); // Vai até 00:30 do dia seguinte

        // Act
        var result = schedule.IsAvailable(dateTime, duration);

        // Assert
        result.Should().BeFalse();
    }
}
