using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public Guid ProviderId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ServiceId { get; private set; }
    public TimeSlot TimeSlot { get; private set; }
    public EBookingStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? CancellationReason { get; private set; }
    public uint Version { get; private set; } // For optimistic concurrency

    private Booking() { } // Required by EF Core

    private Booking(Guid providerId, Guid clientId, Guid serviceId, TimeSlot timeSlot)
    {
        ProviderId = providerId;
        ClientId = clientId;
        ServiceId = serviceId;
        TimeSlot = timeSlot;
        Status = EBookingStatus.Pending;
    }

    public static Booking Create(Guid providerId, Guid clientId, Guid serviceId, TimeSlot timeSlot)
    {
        return new Booking(providerId, clientId, serviceId, timeSlot);
    }

    public void Confirm()
    {
        if (Status != EBookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be confirmed.");
        }

        Status = EBookingStatus.Confirmed;
        MarkAsUpdated();
    }

    public void Reject(string reason)
    {
        if (Status != EBookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be rejected.");
        }

        Status = EBookingStatus.Rejected;
        RejectionReason = reason;
        MarkAsUpdated();
    }

    public void Cancel(string reason)
    {
        // Só permite cancelar se estiver pendente ou confirmado
        if (Status != EBookingStatus.Pending && Status != EBookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Only pending or confirmed bookings can be cancelled.");
        }

        Status = EBookingStatus.Cancelled;
        CancellationReason = reason;
        // Ao cancelar, garantimos que motivos de rejeição anteriores sejam limpos se necessário, 
        // mas aqui optamos por manter o histórico de campos nullable e apenas mudar o status.
        MarkAsUpdated();
    }

    public void Complete()
    {
        if (Status != EBookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed bookings can be marked as completed.");
        }

        Status = EBookingStatus.Completed;
        MarkAsUpdated();
    }
}
