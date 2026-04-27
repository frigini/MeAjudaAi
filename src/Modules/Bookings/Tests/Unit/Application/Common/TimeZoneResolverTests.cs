using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Common;

[Trait("Category", "Unit")]
public class TimeZoneResolverTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    [Fact]
    public void ResolveTimeZone_ShouldReturnTimeZone_WhenIdIsValid()
    {
        // Act
        var result = TimeZoneResolver.ResolveTimeZone("UTC", _loggerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("UTC");
    }

    [Fact]
    public void ResolveTimeZone_ShouldReturnFallback_WhenIdIsInvalid()
    {
        // Act
        var result = TimeZoneResolver.ResolveTimeZone("Invalid/ID", _loggerMock.Object);

        // Assert
        result.Should().NotBeNull();
        // Fallback is usually "E. South America Standard Time" (Windows) or "America/Sao_Paulo" (IANA)
        result!.Id.Should().BeOneOf("E. South America Standard Time", "America/Sao_Paulo");
    }

    [Fact]
    public void CreateValidatedBookingDto_WithAmbiguousDSTTime_ShouldReturnSuccessWithMaxOffset()
    {
        // Arrange
        // Em 2024, PST (Pacific) volta o relógio em 3 de Novembro (ambiguidade 01:00-02:00)
        // O horário 01:30 AM acontece duas vezes (PDT depois PST).
        TimeZoneInfo? pst = TestTimeZones.GetPacific();
        if (pst == null) return;

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 11, 3);
        // Start is ambiguous (1:30 AM)
        var start = new TimeOnly(1, 30);
        // End is NOT ambiguous (2:30 AM). On 03/11/2024, clock rolls back from 02:00 PDT to 01:00 PST.
        // Making 01:00-02:00 ambiguous. 02:00 and 02:30 occur only once as PST (-08:00).
        var end = new TimeOnly(2, 30);
        var slot = TimeSlot.Create(start, end);
        var booking = Booking.Create(providerId, clientId, serviceId, date, slot);

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Start offset should be the maximum of the two ambiguous offsets (PDT -07:00 > PST -08:00)
        var startDateTime = booking.Date.ToDateTime(booking.TimeSlot.Start);
        var startOffsets = pst.GetAmbiguousTimeOffsets(startDateTime);
        var expectedStartOffset = startOffsets.Max();
        result.Value.Start.Offset.Should().Be(expectedStartOffset);

        // End offset should be the unambiguous offset for 2:30 AM PST (-08:00)
        var endDateTime = booking.Date.ToDateTime(booking.TimeSlot.End);
        var expectedEndOffset = pst.GetUtcOffset(endDateTime);
        result.Value.End.Offset.Should().Be(expectedEndOffset);
    }

    [Fact]
    public void CreateValidatedBookingDto_WithStandardTime_ShouldReturnSuccessWithCorrectOffset()
    {
        // Arrange
        TimeZoneInfo? pst = TestTimeZones.GetPacific();
        if (pst == null) return;

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 6, 10); // Verão (PDT: -7)
        var slot = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));
        var booking = Booking.Create(providerId, clientId, serviceId, date, slot);

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Start.Offset.Should().Be(TimeSpan.FromHours(-7));
        result.Value.End.Offset.Should().Be(TimeSpan.FromHours(-7));
    }

    [Fact]
    public void CreateValidatedBookingDto_WithInvalidTime_ShouldReturnFailure()
    {
        // Arrange
        TimeZoneInfo? pst = TestTimeZones.GetPacific();
        if (pst == null) return;

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 3, 10);
        // Em 2024, PST (Pacific) pula de 02:00 para 03:00 em 10 de Março.
        // O horário 02:30 AM não existe.
        var start = new TimeOnly(2, 30);
        var end = new TimeOnly(3, 30);
        var slot = TimeSlot.Create(start, end);
        var booking = Booking.Create(providerId, clientId, serviceId, date, slot);

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Horário inválido");
    }

    private static class TestTimeZones
    {
        public static TimeZoneInfo? GetPacific()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("Pacific Standard Time", out var tz))
                return tz;
            
            if (TimeZoneInfo.TryFindSystemTimeZoneById("America/Los_Angeles", out tz))
                return tz;

            return null;
        }
    }
}
