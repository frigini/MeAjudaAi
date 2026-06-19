using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.API.Mappers;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_CreateBookingRequestDto_ShouldMapAllProperties()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = DateTimeOffset.UtcNow.AddHours(2);
        var request = new CreateBookingRequestDto(
            ProviderId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            Start: start,
            End: end);

        // Act
        var command = request.ToCommand(clientId, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(request.ProviderId);
        command.ClientId.Should().Be(clientId);
        command.ServiceId.Should().Be(request.ServiceId);
        command.Start.Should().Be(start);
        command.End.Should().Be(end);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToCommand_CancelBookingRequestDto_ShouldMapAllProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CancelBookingRequestDto(Reason: "Client request");

        // Act
        var command = request.ToCommand(bookingId, isAdmin: false, providerId, userId, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.BookingId.Should().Be(bookingId);
        command.Reason.Should().Be("Client request");
        command.IsSystemAdmin.Should().BeFalse();
        command.UserProviderId.Should().Be(providerId);
        command.UserClientId.Should().Be(userId);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToCommand_CancelBookingRequestDto_WhenAdmin_ShouldMapIsAdminTrue()
    {
        // Arrange
        var request = new CancelBookingRequestDto(Reason: "Policy violation");

        // Act
        var command = request.ToCommand(Guid.NewGuid(), isAdmin: true, null, null, Guid.NewGuid());

        // Assert
        command.IsSystemAdmin.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_RejectBookingRequestDto_ShouldMapAllProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var request = new RejectBookingRequestDto(Reason: "Schedule conflict");

        // Act
        var command = request.ToCommand(bookingId, isAdmin: false, providerId, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.BookingId.Should().Be(bookingId);
        command.Reason.Should().Be("Schedule conflict");
        command.IsSystemAdmin.Should().BeFalse();
        command.UserProviderId.Should().Be(providerId);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToConfirmCommand_ShouldMapAllProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        // Act
        var command = bookingId.ToConfirmCommand(isAdmin: false, providerId, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.BookingId.Should().Be(bookingId);
        command.IsSystemAdmin.Should().BeFalse();
        command.UserProviderId.Should().Be(providerId);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToCompleteCommand_ShouldMapAllProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        // Act
        var command = bookingId.ToCompleteCommand(isAdmin: false, providerId, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.BookingId.Should().Be(bookingId);
        command.IsSystemAdmin.Should().BeFalse();
        command.UserProviderId.Should().Be(providerId);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToCommand_SetProviderScheduleRequestDto_ShouldMapAllProperties()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, [
                new AvailableSlotDto(
                    DateTimeOffset.UtcNow.AddHours(8),
                    DateTimeOffset.UtcNow.AddHours(12))
            ])
        };
        var request = new SetProviderScheduleRequestDto(
            ProviderId: Guid.NewGuid(),
            Availabilities: availabilities);

        // Act
        var command = request.ToCommand(targetProviderId, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(targetProviderId);
        command.Availabilities.Should().BeSameAs(availabilities);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToQuery_ClientBookings_ShouldMapAllProperties()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        // Act
        var query = clientId.ToQuery(correlationId, page: 2, pageSize: 25, from, to);

        // Assert
        query.Should().NotBeNull();
        query.ClientId.Should().Be(clientId);
        query.CorrelationId.Should().Be(correlationId);
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(25);
        query.From.Should().Be(from);
        query.To.Should().Be(to);
    }

    [Fact]
    public void ToQuery_ClientBookings_WithNullDates_ShouldMapNulls()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var query = clientId.ToQuery(correlationId, 1, 10, null, null);

        // Assert
        query.From.Should().BeNull();
        query.To.Should().BeNull();
    }

    [Fact]
    public void ToProviderQuery_ShouldMapAllProperties()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        // Act
        var query = providerId.ToProviderQuery(correlationId, page: 3, pageSize: 50, from, to);

        // Assert
        query.Should().NotBeNull();
        query.ProviderId.Should().Be(providerId);
        query.CorrelationId.Should().Be(correlationId);
        query.Page.Should().Be(3);
        query.PageSize.Should().Be(50);
        query.From.Should().Be(from);
        query.To.Should().Be(to);
    }

    [Fact]
    public void ToQuery_BookingById_ShouldMapAllProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var query = bookingId.ToQuery(userId, providerId, isSystemAdmin: true, correlationId);

        // Assert
        query.Should().NotBeNull();
        query.BookingId.Should().Be(bookingId);
        query.UserId.Should().Be(userId);
        query.ProviderId.Should().Be(providerId);
        query.IsSystemAdmin.Should().BeTrue();
        query.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToQuery_BookingById_WithNullIds_ShouldMapNulls()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var query = bookingId.ToQuery(userId: null, providerId: null, isSystemAdmin: false, correlationId);

        // Assert
        query.UserId.Should().BeNull();
        query.ProviderId.Should().BeNull();
        query.IsSystemAdmin.Should().BeFalse();
    }

    [Fact]
    public void ToAvailabilityQuery_ShouldMapAllProperties()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 6, 17);
        var correlationId = Guid.NewGuid();

        // Act
        var query = providerId.ToAvailabilityQuery(date, correlationId);

        // Assert
        query.Should().NotBeNull();
        query.ProviderId.Should().Be(providerId);
        query.Date.Should().Be(date);
        query.CorrelationId.Should().Be(correlationId);
    }
}
