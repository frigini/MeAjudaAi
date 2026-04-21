using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Domain.Entities;

public class BookingTests : BaseUnitTest
{
    [Fact]
    public void Create_Should_InitializeWithPendingStatus()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var timeSlot = TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        // Act
        var booking = Booking.Create(providerId, clientId, serviceId, timeSlot);

        // Assert
        booking.Status.Should().Be(EBookingStatus.Pending);
        booking.ProviderId.Should().Be(providerId);
        booking.ClientId.Should().Be(clientId);
        booking.ServiceId.Should().Be(serviceId);
        booking.TimeSlot.Should().Be(timeSlot);
    }

    [Fact]
    public void Confirm_Should_ChangeStatusToConfirmed_When_Pending()
    {
        // Arrange
        var booking = CreatePendingBooking();

        // Act
        booking.Confirm();

        // Assert
        booking.Status.Should().Be(EBookingStatus.Confirmed);
        booking.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_Should_ThrowException_When_NotPending()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Confirm();

        // Act
        var act = () => booking.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending bookings can be confirmed.");
    }

    [Fact]
    public void Reject_Should_ChangeStatusToRejected_When_Pending()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var reason = "Provider unavailable";

        // Act
        booking.Reject(reason);

        // Assert
        booking.Status.Should().Be(EBookingStatus.Rejected);
        booking.RejectionReason.Should().Be(reason);
        booking.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_Should_ChangeStatusToCancelled()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var reason = "Client changed mind";

        // Act
        booking.Cancel(reason);

        // Assert
        booking.Status.Should().Be(EBookingStatus.Cancelled);
        booking.CancellationReason.Should().Be(reason);
        booking.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_Should_ChangeStatusToCompleted_When_Confirmed()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Confirm();

        // Act
        booking.Complete();

        // Assert
        booking.Status.Should().Be(EBookingStatus.Completed);
        booking.UpdatedAt.Should().NotBeNull();
    }

    private static Booking CreatePendingBooking()
    {
        return Booking.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)));
    }
}
