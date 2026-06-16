using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Services;

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
        result!.Id.Should().BeOneOf("E. South America Standard Time", TimeZoneConstants.DefaultTimeZoneId);
    }

    [Fact]
    public void CreateValidatedBookingDto_WithAmbiguousDSTTime_ShouldReturnSuccessWithCorrectDateAndTime()
    {
        // Arrange
        TimeZoneInfo? pst = TestTimeZones.GetPacific();
        if (pst == null) return;

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 11, 3);
        var start = new TimeOnly(1, 30);
        var end = new TimeOnly(2, 30);
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(clientId)
            .WithServiceId(serviceId)
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(start).WithEnd(end).Build())
            .Build();

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Date.Should().Be(date);
        result.Value!.StartTime.Should().Be(start);
        result.Value!.EndTime.Should().Be(end);
    }

    [Fact]
    public void CreateValidatedBookingDto_WithStandardTime_ShouldReturnSuccessWithCorrectDateAndTime()
    {
        // Arrange
        TimeZoneInfo? pst = TestTimeZones.GetPacific();
        if (pst == null) return;

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 6, 10);
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(clientId)
            .WithServiceId(serviceId)
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Date.Should().Be(date);
        result.Value!.StartTime.Should().Be(new TimeOnly(10, 0));
        result.Value!.EndTime.Should().Be(new TimeOnly(11, 0));
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
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(clientId)
            .WithServiceId(serviceId)
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(start).WithEnd(end).Build())
            .Build();

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Horário inválido");
    }

    [Fact]
    public void ResolveTimeZone_WithNullId_ShouldReturnFallback()
    {
        var result = TimeZoneResolver.ResolveTimeZone(null, _loggerMock.Object);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ResolveTimeZone_WithStrictMode_InvalidId_ShouldReturnNull()
    {
        var result = TimeZoneResolver.ResolveTimeZone("Invalid/ID", _loggerMock.Object, allowFallback: false);

        result.Should().BeNull();
    }

    [Fact]
    public void ResolveTimeZone_WithStrictMode_ValidId_ShouldReturnTimeZone()
    {
        var result = TimeZoneResolver.ResolveTimeZone("UTC", _loggerMock.Object, allowFallback: false);

        result.Should().NotBeNull();
        result!.Id.Should().Be("UTC");
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