using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;

namespace MeAjudaAi.Modules.Bookings.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public Guid ProviderId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateOnly Date { get; private set; } // Data do agendamento
    public TimeSlot TimeSlot { get; private set; }
    public EBookingStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? CancellationReason { get; private set; }

    private Booking() { } // Required by EF Core

    private Booking(Guid providerId, Guid clientId, Guid serviceId, DateOnly date, TimeSlot timeSlot)
    {
        ProviderId = providerId;
        ClientId = clientId;
        ServiceId = serviceId;
        Date = date;
        TimeSlot = timeSlot;
        Status = EBookingStatus.Pending;

        AddDomainEvent(new BookingCreatedDomainEvent(
            Id, 1, ProviderId, ClientId, ServiceId, Date));
    }

    public static Booking Create(Guid providerId, Guid clientId, Guid serviceId, DateOnly date, TimeSlot timeSlot)
    {
        return new Booking(providerId, clientId, serviceId, date, timeSlot);
    }

    public void Confirm()
    {
        if (Status != EBookingStatus.Pending)
        {
            throw new InvalidBookingStateException("Only pending bookings can be confirmed.");
        }

        Status = EBookingStatus.Confirmed;
        MarkAsUpdated();

        AddDomainEvent(new BookingConfirmedDomainEvent(
            Id, 1, ProviderId, ClientId));
    }

    public void Reject(string reason)
    {
        if (Status != EBookingStatus.Pending)
        {
            throw new InvalidBookingStateException("Only pending bookings can be rejected.");
        }

        Status = EBookingStatus.Rejected;
        RejectionReason = reason;
        MarkAsUpdated();

        AddDomainEvent(new BookingRejectedDomainEvent(
            Id, 1, ProviderId, ClientId, reason));
    }

    public void Cancel(string reason)
    {
        // Só permite cancelar se estiver pendente ou confirmado
        if (Status != EBookingStatus.Pending && Status != EBookingStatus.Confirmed)
        {
            throw new InvalidBookingStateException("Only pending or confirmed bookings can be cancelled.");
        }

        Status = EBookingStatus.Cancelled;
        CancellationReason = reason;
        MarkAsUpdated();

        AddDomainEvent(new BookingCancelledDomainEvent(
            Id, 1, ProviderId, ClientId, reason));
    }

    public void Complete()
    {
        if (Status != EBookingStatus.Confirmed)
        {
            throw new InvalidBookingStateException("Only confirmed bookings can be marked as completed.");
        }

        Status = EBookingStatus.Completed;
        MarkAsUpdated();

        AddDomainEvent(new BookingCompletedDomainEvent(
            Id, 1, ProviderId, ClientId));
    }
}
