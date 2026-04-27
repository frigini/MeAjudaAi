using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Domain.ValueObjects;

public class AvailabilityTests : BaseUnitTest
{
    [Fact]
    public void Create_Should_OrderSlotsByStartTime()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var lateSlot = TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(15, 0));
        var earlySlot = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0));
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
    public void Create_Should_NotThrow_When_SlotsAreAdjacent()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(12, 0));
        var slots = new[] { slot1, slot2 };

        // Act
        var act = () => Availability.Create(day, slots);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_Should_ThrowException_When_SlotsOverlap()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(11, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(12, 0));
        var slots = new[] { slot1, slot2 };

        // Act
        var act = () => Availability.Create(day, slots);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Availability slots for {day} cannot overlap.");
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var avail1 = Availability.Create(DayOfWeek.Monday, [slot1]);
        var avail2 = Availability.Create(DayOfWeek.Monday, [slot1]);

        // Act & Assert
        avail1.Equals(avail2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var avail1 = Availability.Create(DayOfWeek.Monday, [slot1]);
        var avail2 = Availability.Create(DayOfWeek.Tuesday, [slot1]);

        // Act & Assert
        avail1.Equals(avail2).Should().BeFalse();
    }
}
