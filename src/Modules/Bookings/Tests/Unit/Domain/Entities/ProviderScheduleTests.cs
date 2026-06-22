using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Domain.Entities;

public class ProviderScheduleTests
{
    [Fact]
    public void Create_Should_InitializeWithDefaultTimeZone()
    {
        // Arrange & Act
        var providerId = Guid.NewGuid();
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .Build();

        // Assert
        schedule.ProviderId.Should().Be(providerId);
        schedule.TimeZoneId.Should().Be(TimeZoneConstants.DefaultTimeZoneId);
        schedule.Availabilities.Should().BeEmpty();
    }

    [Fact]
    public void SetAvailability_Should_AddOrUpdateDay()
    {
        // Arrange
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
        var availability = new AvailabilityBuilder()
            .WithDayOfWeek(DayOfWeek.Monday)
            .WithSingleSlot(new TimeOnly(8, 0), new TimeOnly(12, 0))
            .Build();

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
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
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
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
        schedule.SetAvailability(new AvailabilityBuilder()
            .WithDayOfWeek(DayOfWeek.Monday)
            .WithSingleSlot(new TimeOnly(8, 0), new TimeOnly(12, 0))
            .Build());
        
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
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
        var dateTime = new DateTime(2026, 4, 20, 9, 0, 0); // Segunda-feira
        schedule.SetAvailability(new AvailabilityBuilder()
            .WithDayOfWeek(DayOfWeek.Monday)
            .WithSingleSlot(new TimeOnly(8, 0), new TimeOnly(12, 0))
            .Build());

        // Act & Assert
        schedule.IsAvailable(dateTime, TimeSpan.Zero).Should().BeFalse();
        schedule.IsAvailable(dateTime, TimeSpan.FromHours(-1)).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_Should_ReturnFalse_When_CrossesMidnight()
    {
        // Arrange
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
        // Slot cobre até o final do dia para garantir que falhemos por cruzamento de data
        schedule.SetAvailability(new AvailabilityBuilder()
            .WithDayOfWeek(DayOfWeek.Monday)
            .WithSingleSlot(new TimeOnly(22, 0), TimeOnly.MaxValue)
            .Build());

        var dateTime = new DateTime(2026, 4, 20, 23, 30, 0); // Segunda, 23:30
        var duration = TimeSpan.FromHours(1); // Vai até 00:30 do dia seguinte

        // Act
        var result = schedule.IsAvailable(dateTime, duration);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateTimeZone_Should_ChangeTimeZone_When_Valid()
    {
        // Arrange
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
        var newTimeZone = "UTC";

        // Act
        schedule.UpdateTimeZone(newTimeZone);

        // Assert
        schedule.TimeZoneId.Should().Be(newTimeZone);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateTimeZone_Should_Throw_When_NullOrWhitespace(string? timeZone)
    {
        // Arrange
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();

        // Act
        var act = () => schedule.UpdateTimeZone(timeZone!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateTimeZone_Should_Throw_When_InvalidTimeZoneId()
    {
        // Arrange
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();

        // Act
        var act = () => schedule.UpdateTimeZone("Invalid/TimeZone");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("TimeZoneId inválido*");
    }

    [Fact]
    public void ClearAvailabilities_Should_EmptyTheList()
    {
        // Arrange
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(Guid.NewGuid())
            .Build();
        schedule.SetAvailability(new AvailabilityBuilder()
            .WithDayOfWeek(DayOfWeek.Monday)
            .WithSingleSlot(new TimeOnly(8, 0), new TimeOnly(12, 0))
            .Build());
        schedule.Availabilities.Should().NotBeEmpty();

        // Act
        schedule.ClearAvailabilities();

        // Assert
        schedule.Availabilities.Should().BeEmpty();
    }
}