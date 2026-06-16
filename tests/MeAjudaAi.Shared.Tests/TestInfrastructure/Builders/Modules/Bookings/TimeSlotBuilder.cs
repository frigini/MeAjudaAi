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
