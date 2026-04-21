using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Domain.ValueObjects;

public class AvailabilityTests : BaseUnitTest
{
    [Fact]
    public void Create_Should_OrderSlotsByStartTime()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var lateSlot = TimeSlot.Create(DateTime.UtcNow.AddHours(4), DateTime.UtcNow.AddHours(5));
        var earlySlot = TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));
        var slots = new[] { lateSlot, earlySlot };

        // Act
        var availability = Availability.Create(day, slots);

        // Assert
        availability.DayOfWeek.Should().Be(day);
        availability.Slots.Should().HaveCount(2);
        availability.Slots[0].Should().Be(earlySlot);
        availability.Slots[1].Should().Be(lateSlot);
    }

    [Fact]
    public void Create_Should_ThrowException_When_SlotsOverlap()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var slot1 = TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));
        var slot2 = TimeSlot.Create(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(4));
        var slots = new[] { slot1, slot2 };

        // Act
        var act = () => Availability.Create(day, slots);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Availability slots for {day} cannot overlap.");
    }
}
