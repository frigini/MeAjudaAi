using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Domain.ValueObjects;

public class TimeSlotTests : BaseUnitTest
{
    [Fact]
    public void Create_Should_SetProperties_When_Valid()
    {
        // Arrange & Act
        var start = new TimeOnly(8, 0);
        var end = new TimeOnly(12, 0);
        var slot = TimeSlot.Create(start, end);

        // Assert
        slot.Start.Should().Be(start);
        slot.End.Should().Be(end);
        slot.Duration.Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public void Create_Should_Throw_When_StartAfterEnd()
    {
        // Act
        var act = () => TimeSlot.Create(new TimeOnly(12, 0), new TimeOnly(8, 0));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*before end time*");
    }

    [Fact]
    public void Overlaps_Should_ReturnTrue_When_SlotsOverlap()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(14, 0));

        // Act & Assert
        slot1.Overlaps(slot2).Should().BeTrue();
        slot2.Overlaps(slot1).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_Should_ReturnFalse_When_SlotsAreAdjacent()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Act & Assert
        slot1.Overlaps(slot2).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_Should_ReturnFalse_When_SlotsAreDisjoint()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(11, 0), new TimeOnly(12, 0));

        // Act & Assert
        slot1.Overlaps(slot2).Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Throw_When_StartEqualsEnd()
    {
        // Act
        var act = () => TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(10, 0));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*before end time*");
    }

    [Fact]
    public void FromDateTime_Should_IgnoreDateComponent()
    {
        // Arrange
        var dt1 = new DateTime(2026, 4, 22, 10, 0, 0);
        var dt2 = new DateTime(2026, 4, 22, 11, 0, 0);
        var dt3 = new DateTime(2026, 5, 30, 10, 0, 0);
        var dt4 = new DateTime(2026, 5, 30, 11, 0, 0);

        // Act
        var slot1 = TimeSlot.FromDateTime(dt1, dt2);
        var slot2 = TimeSlot.FromDateTime(dt3, dt4);

        // Assert
        slot1.Should().Be(slot2);
        slot1.Start.Should().Be(new TimeOnly(10, 0));
    }
}
