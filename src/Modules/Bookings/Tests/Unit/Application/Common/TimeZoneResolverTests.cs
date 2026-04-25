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
        // Em Windows costuma ser "E. South America Standard Time", em Linux "America/Sao_Paulo"
        result!.Id.Should().Match(id => id == "E. South America Standard Time" || id == "America/Sao_Paulo");
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
        var slot = TimeSlot.Create(new TimeOnly(1, 30), new TimeOnly(2, 30));
        var booking = Booking.Create(providerId, clientId, serviceId, date, slot);

        // Act
        var result = TimeZoneResolver.CreateValidatedBookingDto(booking, pst, _loggerMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // O maior offset deve ser escolhido (PST é -8, PDT é -7. O Max de {-8, -7} é -7)
        result.Value.Start.Offset.Should().Be(TimeSpan.FromHours(-7));
    }

    private static class TestTimeZones
    {
        public static TimeZoneInfo GetPacific()
        {
            try {
                return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            } catch {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
        }
    }
}
