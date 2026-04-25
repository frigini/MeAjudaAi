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
        act.Should().Throw<ArgumentException>().WithMessage("O horário de início deve ser anterior ao horário de término.");
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
        act.Should().Throw<ArgumentException>().WithMessage("O horário de início deve ser anterior ao horário de término.");
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
        slot1.End.Should().Be(new TimeOnly(11, 0));
    }

    [Fact]
    public void Subtract_Should_Split_Into_Remaining_Segments()
    {
        // Arrange
        var free = TimeSlot.Create(new(9, 0), new(12, 0));
        var occupied = new[] {
            TimeSlot.Create(new(9,30), new(10,00)),
            TimeSlot.Create(new(11,00), new(11,30))
        };

        var expected = new[]
        {
            TimeSlot.Create(new(9, 0), new(9, 30)),
            TimeSlot.Create(new(10, 0), new(11, 0)),
            TimeSlot.Create(new(11, 30), new(12, 0))
        };

        // Act
        var result = free.Subtract(occupied);

        // Assert
        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    [Fact]
    public void FromDateTime_Should_Throw_When_DatesAreDifferent()
    {
        // Arrange
        var start = new DateTime(2026, 4, 22, 10, 0, 0);
        var end = new DateTime(2026, 4, 23, 11, 0, 0);

        // Act
        var act = () => TimeSlot.FromDateTime(start, end);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Start and end must be on the same date*");
    }

    [Fact]
    public void FromDateTime_Should_Throw_When_KindsAreDifferent()
    {
        // Arrange
        var start = new DateTime(2026, 4, 22, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 22, 11, 0, 0, DateTimeKind.Local);

        // Act
        var act = () => TimeSlot.FromDateTime(start, end);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Start and end must have the same DateTimeKind*");
    }

    [Fact]
    public void Subtract_Should_ReturnOriginalSlot_When_OccupiedIsAdjacent()
    {
        // Arrange
        var free = TimeSlot.Create(new(9, 0), new(10, 0));
        var occupied = new[] {
            TimeSlot.Create(new(10, 0), new(11, 0))
        };

        // Act
        var result = free.Subtract(occupied);

        // Assert
        result.Should().ContainSingle();
        result[0].Should().Be(free);
    }
    
    [Fact]
    public void Subtract_Should_ReturnEmpty_When_OccupiedFullyCovers()
    {
        // Arrange
        var free = TimeSlot.Create(new(9, 0), new(10, 0));
        var occupied = new[] {
            TimeSlot.Create(new(8, 0), new(11, 0))
        };

        // Act
        var result = free.Subtract(occupied);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));

        // Act & Assert
        slot1.Equals(slot2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var slot1 = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var slot2 = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(12, 0));

        // Act & Assert
        slot1.Equals(slot2).Should().BeFalse();
    }
}
