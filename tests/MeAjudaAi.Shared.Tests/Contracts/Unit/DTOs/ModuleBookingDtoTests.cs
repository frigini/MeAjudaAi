using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit.DTOs;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ModuleBookingDtoTests
{
    [Fact]
    public void ModuleBookingDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new ModuleBookingDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            ClientId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            Date: new DateOnly(2025, 3, 15),
            StartTime: new TimeOnly(14, 0),
            EndTime: new TimeOnly(15, 0),
            Status: EBookingStatus.Confirmed
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleBookingDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(dto.Id);
        deserialized.Status.Should().Be(EBookingStatus.Confirmed);
    }

    [Fact]
    public void ModuleBookingDto_WithRejectionReason_ShouldSerialize()
    {
        // Arrange
        var dto = new ModuleBookingDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            ClientId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            Date: new DateOnly(2025, 3, 15),
            StartTime: new TimeOnly(14, 0),
            EndTime: new TimeOnly(15, 0),
            Status: EBookingStatus.Rejected,
            RejectionReason: "Schedule conflict"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleBookingDto>(json);

        // Assert
        deserialized!.RejectionReason.Should().Be("Schedule conflict");
        deserialized.CancellationReason.Should().BeNull();
    }

    [Fact]
    public void ModuleBookingDto_WithCancellationReason_ShouldSerialize()
    {
        // Arrange
        var dto = new ModuleBookingDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            ClientId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            Date: new DateOnly(2025, 3, 15),
            StartTime: new TimeOnly(14, 0),
            EndTime: new TimeOnly(15, 0),
            Status: EBookingStatus.Cancelled,
            CancellationReason: "Client request"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleBookingDto>(json);

        // Assert
        deserialized!.CancellationReason.Should().Be("Client request");
        deserialized.RejectionReason.Should().BeNull();
    }

    [Theory]
    [InlineData(EBookingStatus.Pending)]
    [InlineData(EBookingStatus.Confirmed)]
    [InlineData(EBookingStatus.Rejected)]
    [InlineData(EBookingStatus.Cancelled)]
    [InlineData(EBookingStatus.Completed)]
    public void ModuleBookingDto_ShouldSupportAllStatuses(EBookingStatus status)
    {
        // Arrange & Act
        var dto = new ModuleBookingDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            ClientId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            Date: new DateOnly(2025, 3, 15),
            StartTime: new TimeOnly(14, 0),
            EndTime: new TimeOnly(15, 0),
            Status: status
        );

        // Assert
        dto.Status.Should().Be(status);
    }
}
