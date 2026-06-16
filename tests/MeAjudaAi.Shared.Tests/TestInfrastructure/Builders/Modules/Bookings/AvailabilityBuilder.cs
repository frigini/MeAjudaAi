using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

[ExcludeFromCodeCoverage]
public class AvailabilityBuilder : BaseBuilder<Availability>
{
    private DayOfWeek? _dayOfWeek;
    private readonly List<TimeSlot> _slots = [];

    public AvailabilityBuilder()
    {
        Faker = new Faker<Availability>()
            .CustomInstantiator(_ =>
            {
                var day = _dayOfWeek ?? DayOfWeek.Monday;
                var slots = _slots.Count > 0 ? _slots : [TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(12, 0))];
                return Availability.Create(day, slots);
            });
    }

    public AvailabilityBuilder WithDayOfWeek(DayOfWeek day)
    {
        _dayOfWeek = day;
        return this;
    }

    public AvailabilityBuilder WithSlots(params TimeSlot[] slots)
    {
        _slots.Clear();
        _slots.AddRange(slots);
        return this;
    }

    public AvailabilityBuilder WithSingleSlot(TimeOnly start, TimeOnly end)
    {
        _slots.Clear();
        _slots.Add(TimeSlot.Create(start, end));
        return this;
    }

    public AvailabilityBuilder Monday()
    {
        _dayOfWeek = DayOfWeek.Monday;
        return this;
    }

    public AvailabilityBuilder Weekday()
    {
        _dayOfWeek = DayOfWeek.Monday;
        _slots.Clear();
        _slots.Add(TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(12, 0)));
        _slots.Add(TimeSlot.Create(new TimeOnly(13, 0), new TimeOnly(17, 0)));
        return this;
    }
}
