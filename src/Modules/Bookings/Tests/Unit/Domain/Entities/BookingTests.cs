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
        var date = new DateOnly(2026, 4, 22);
        var timeSlot = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));

        // Act
        var booking = Booking.Create(providerId, clientId, serviceId, date, timeSlot);

        // Assert
        booking.Status.Should().Be(EBookingStatus.Pending);
        booking.ProviderId.Should().Be(providerId);
        booking.ClientId.Should().Be(clientId);
        booking.ServiceId.Should().Be(serviceId);
        booking.Date.Should().Be(date);
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
    public void Cancel_Should_ChangeStatusToCancelled_When_Pending()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var reason = "Client changed mind";
        var previousUpdatedAt = booking.UpdatedAt;

        // Act
        booking.Cancel(reason);

        // Assert
        booking.Status.Should().Be(EBookingStatus.Cancelled);
        booking.CancellationReason.Should().Be(reason);
        booking.UpdatedAt.Should().NotBeNull();
        if (previousUpdatedAt != null)
        {
            booking.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt.Value);
        }
    }

    [Fact]
    public void Cancel_Should_ChangeStatusToCancelled_When_Confirmed()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Confirm();
        var reason = "Provider emergency";
        var previousUpdatedAt = booking.UpdatedAt;

        // Act
        booking.Cancel(reason);

        // Assert
        booking.Status.Should().Be(EBookingStatus.Cancelled);
        booking.CancellationReason.Should().Be(reason);
        booking.UpdatedAt.Should().NotBeNull();
        if (previousUpdatedAt != null)
        {
            booking.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt.Value);
        }
    }

    [Fact]
    public void Cancel_Should_Throw_When_Rejected()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Reject("Busy");

        // Act
        var act = () => booking.Cancel("Change mind");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending or confirmed bookings can be cancelled.");
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

    [Fact]
    public void Complete_Should_Throw_When_Pending()
    {
        // Arrange
        var booking = CreatePendingBooking();

        // Act
        var act = () => booking.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only confirmed bookings can be marked as completed.");
    }

    private static Booking CreatePendingBooking()
    {
        return Booking.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
    }
}
