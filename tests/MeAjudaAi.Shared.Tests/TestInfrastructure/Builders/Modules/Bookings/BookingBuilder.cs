using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

[ExcludeFromCodeCoverage]
public class BookingBuilder : BaseBuilder<Booking>
{
    private Guid? _providerId;
    private Guid? _clientId;
    private Guid? _serviceId;
    private DateOnly? _date;
    private TimeSlot? _timeSlot;
    private EBookingStatus? _status;

    public BookingBuilder()
    {
        Faker = new Faker<Booking>()
            .CustomInstantiator(f =>
            {
                var provider = _providerId ?? f.Random.Guid();
                var client = _clientId ?? f.Random.Guid();
                var service = _serviceId ?? f.Random.Guid();
                var date = _date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
                var timeSlot = _timeSlot ?? TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));

                Booking booking;

                if (_status is null or EBookingStatus.Pending)
                {
                    booking = Booking.Create(provider, client, service, date, timeSlot);
                }
                else if (_status == EBookingStatus.Completed)
                {
                    booking = new Booking(
                        Guid.NewGuid(),
                        provider,
                        client,
                        service,
                        date,
                        timeSlot,
                        EBookingStatus.Confirmed,
                        rowVersion: 1);
                    booking.Complete();
                }
                else
                {
                    booking = new Booking(
                        Guid.NewGuid(),
                        provider,
                        client,
                        service,
                        date,
                        timeSlot,
                        _status.Value,
                        rowVersion: 1);
                }

                return booking;
            });
    }

    public BookingBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public BookingBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public BookingBuilder WithServiceId(Guid serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    public BookingBuilder WithDate(DateOnly date)
    {
        _date = date;
        return this;
    }

    public BookingBuilder WithTimeSlot(TimeSlot timeSlot)
    {
        _timeSlot = timeSlot;
        return this;
    }

    public BookingBuilder WithTimeSlot(TimeOnly start, TimeOnly end)
    {
        _timeSlot = TimeSlot.Create(start, end);
        return this;
    }

    public BookingBuilder WithStatus(EBookingStatus status)
    {
        _status = status;
        return this;
    }

    public BookingBuilder AsPending()
    {
        _status = EBookingStatus.Pending;
        return this;
    }

    public BookingBuilder AsConfirmed()
    {
        _status = EBookingStatus.Confirmed;
        return this;
    }

    public BookingBuilder AsRejected()
    {
        _status = EBookingStatus.Rejected;
        return this;
    }

    public BookingBuilder AsCancelled()
    {
        _status = EBookingStatus.Cancelled;
        return this;
    }

    public BookingBuilder AsCompleted()
    {
        _status = EBookingStatus.Completed;
        return this;
    }
}
