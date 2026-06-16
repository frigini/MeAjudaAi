using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

[ExcludeFromCodeCoverage]
public class TimeSlotBuilder : BaseBuilder<TimeSlot>
{
    private TimeOnly? _start;
    private TimeOnly? _end;

    public TimeSlotBuilder()
    {
        Faker = new Faker<TimeSlot>()
            .CustomInstantiator(f => TimeSlot.Create(
                _start ?? new TimeOnly(9, 0),
                _end ?? new TimeOnly(10, 0)));
    }

    public TimeSlotBuilder WithStart(TimeOnly start)
    {
        _start = start;
        return this;
    }

    public TimeSlotBuilder WithEnd(TimeOnly end)
    {
        _end = end;
        return this;
    }

    public TimeSlotBuilder WithDuration(int startHour, int durationHours)
    {
        if (startHour is < 0 or >= 24)
            throw new ArgumentException("Start hour must be between 0 and 23");
        if (startHour + durationHours > 24)
            throw new ArgumentException("End hour must not exceed 24");
        if (durationHours < 1)
            throw new ArgumentException("Duration must be at least 1 hour");

        _start = new TimeOnly(startHour, 0);
        _end = new TimeOnly(startHour + durationHours, 0);
        return this;
    }

    public TimeSlotBuilder Morning()
    {
        _start = new TimeOnly(9, 0);
        _end = new TimeOnly(12, 0);
        return this;
    }

    public TimeSlotBuilder Afternoon()
    {
        _start = new TimeOnly(13, 0);
        _end = new TimeOnly(17, 0);
        return this;
    }
}
