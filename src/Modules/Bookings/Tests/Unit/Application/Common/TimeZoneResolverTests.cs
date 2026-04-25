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
    public void ResolveTimeZone_WithInvalidIdAndNoFallback_ShouldReturnNull()
    {
        // Act
        var result = TimeZoneResolver.ResolveTimeZone("Invalid-TZ", _loggerMock.Object, allowFallback: false);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to resolve time zone")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ResolveTimeZone_WithNullIdAndFallback_ShouldReturnBrazilTimeZone()
    {
        // Act
        var result = TimeZoneResolver.ResolveTimeZone(null, _loggerMock.Object, allowFallback: true);

        // Assert
        result.Should().NotBeNull();
        // Relaxado: apenas verifica offset, não Id específico (plataforma-dependente)
        result.BaseUtcOffset.Should().Be(TimeSpan.FromHours(-3));
    }

    [Fact]
    public void CreateValidatedBookingDto_WithInvalidDSTTime_ShouldReturnFailure()
    {
        // Arrange
        // Usando Pacific Standard Time para um teste determinístico de DST
        // Em 2024, o horário pula de 02:00 para 03:00 em 10 de Março.
        TimeZoneInfo pst = TestTimeZones.GetPacific();

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 3, 10);
        // 02:30 AM não existe em PST neste dia
        var slot = TimeSlot.Create(new TimeOnly(2, 30), new TimeOnly(3, 30));
        var booking = Booking.Create(providerId, clientId, serviceId, date, slot);

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Horário inválido");
    }

    [Fact]
    public void CreateValidatedBookingDto_WithAmbiguousDSTTime_ShouldReturnSuccessWithMaxOffset()
    {
        // Arrange
        // Em 2024, o horário volta de 02:00 para 01:00 em 3 de Novembro em PST.
        // 01:30 AM acontece duas vezes.
        TimeZoneInfo pst = TestTimeZones.GetPacific();

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 11, 3);
        // Start is ambiguous (1:30 AM). On 03/11/2024, clock rolls back from 02:00 PDT to 01:00 PST.
        // Making 01:00-02:00 occur twice.
        var start = new TimeOnly(1, 30);
        // End is NOT ambiguous (2:30 AM). 02:00 and 02:30 occur only once as PST (-08:00).
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
        TimeZoneInfo pst = TestTimeZones.GetPacific();

        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 6, 10); // Summer (PDT: -7)
        var slot = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));
        var booking = Booking.Create(providerId, clientId, serviceId, date, slot);

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Start.Offset.Should().Be(TimeSpan.FromHours(-7));
        result.Value.End.Offset.Should().Be(TimeSpan.FromHours(-7));
    }

    private static class TestTimeZones
    {
        public static TimeZoneInfo GetPacific()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
        }
    }
}
