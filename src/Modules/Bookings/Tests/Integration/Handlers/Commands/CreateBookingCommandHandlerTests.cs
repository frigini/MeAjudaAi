using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Handlers.Commands;

public class CreateBookingCommandHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockProvidersApi().SeedProviderService(providerId, serviceId);
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        await CreateScheduleAsync(providerId);

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var start = new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero);
        var end = new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(11, 0)), TimeSpan.Zero);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>>();
        var command = new CreateBookingCommand(providerId, clientId, serviceId, start, end, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProviderId.Should().Be(providerId);
        result.Value.ClientId.Should().Be(clientId);
    }

    [Fact]
    public async Task Create_NonExistingProvider_ShouldReturnNotFound()
    {
        // Arrange
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>>();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero),
            new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(11, 0)), TimeSpan.Zero),
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Create_InactiveService_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid(), isActive: false);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>>();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), serviceId,
            new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero),
            new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(11, 0)), TimeSpan.Zero),
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Create_OverlappingBooking_ShouldReturnConflict()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockProvidersApi().SeedProviderService(providerId, serviceId);
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        await CreateScheduleAsync(providerId);

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        await CreateBookingAsync(providerId, clientId, serviceId, tomorrow, new TimeOnly(10, 0), new TimeOnly(11, 0));

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>>();
        var command = new CreateBookingCommand(
            providerId, clientId, serviceId,
            new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(10, 30)), TimeSpan.Zero),
            new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(11, 30)), TimeSpan.Zero),
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
