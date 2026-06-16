using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

[ExcludeFromCodeCoverage]
public class ProviderScheduleBuilder : BaseBuilder<ProviderSchedule>
{
    private Guid? _providerId;
    private string? _timeZoneId;
    private readonly List<Availability> _availabilities = [];

    public ProviderScheduleBuilder()
    {
        Faker = new Faker<ProviderSchedule>()
            .CustomInstantiator(f =>
            {
                var schedule = ProviderSchedule.Create(
                    _providerId ?? f.Random.Guid(),
                    _timeZoneId);

                foreach (var availability in _availabilities)
                {
                    schedule.SetAvailability(availability);
                }

                return schedule;
            });
    }

    public ProviderScheduleBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public ProviderScheduleBuilder WithTimeZoneId(string timeZoneId)
    {
        _timeZoneId = timeZoneId;
        return this;
    }

    public ProviderScheduleBuilder WithAvailabilities(params Availability[] availabilities)
    {
        _availabilities.Clear();
        _availabilities.AddRange(availabilities);
        return this;
    }

    public ProviderScheduleBuilder WithSingleSlot(DayOfWeek day, TimeOnly start, TimeOnly end)
    {
        _availabilities.Clear();
        _availabilities.Add(Availability.Create(day, [TimeSlot.Create(start, end)]));
        return this;
    }
}
