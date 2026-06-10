using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;

namespace MeAjudaAi.Modules.Bookings.Domain.Entities;

/// <summary>
/// Representa um agendamento (Booking) entre um prestador e um cliente para um serviço específico.
/// Esta entidade é uma raiz de agregação (Aggregate Root) que gerencia o ciclo de vida do agendamento.
/// </summary>
public sealed class Booking : AggregateRoot<Guid>
{
    public Guid ProviderId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeSlot TimeSlot { get; private set; } = null!;
    public EBookingStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? CancellationReason { get; private set; }
    public uint RowVersion { get; private set; } // Optimistic concurrency token (xid in Postgres)

    private Booking() { } // Required by EF Core

    public Booking(Guid id, Guid providerId, Guid clientId, Guid serviceId, DateOnly date, TimeSlot timeSlot, EBookingStatus status, uint rowVersion)
    {
        Id = id;
        ProviderId = providerId;
        ClientId = clientId;
        ServiceId = serviceId;
        Date = date;
        TimeSlot = timeSlot;
        Status = status;
        RowVersion = rowVersion;
    }

    private Booking(Guid providerId, Guid clientId, Guid serviceId, DateOnly date, TimeSlot timeSlot)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        ClientId = clientId;
        ServiceId = serviceId;
        Date = date;
        TimeSlot = timeSlot;
        Status = EBookingStatus.Pending;
        RowVersion = 1;

        AddDomainEvent(new BookingCreatedDomainEvent(
            Id, (int)RowVersion, ProviderId, ClientId, ServiceId, Date));
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
            Id, (int)RowVersion, ProviderId, ClientId));
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
            Id, (int)RowVersion, ProviderId, ClientId, reason));
    }

    public void Cancel(string reason)
    {
        if (Status is not EBookingStatus.Pending and not EBookingStatus.Confirmed)
        {
            throw new InvalidBookingStateException("Only pending or confirmed bookings can be cancelled.");
        }

        Status = EBookingStatus.Cancelled;
        CancellationReason = reason;
        MarkAsUpdated();

        AddDomainEvent(new BookingCancelledDomainEvent(
            Id, (int)RowVersion, ProviderId, ClientId, reason));
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
            Id, (int)RowVersion, ProviderId, ClientId));
    }
}
