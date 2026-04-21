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
}
