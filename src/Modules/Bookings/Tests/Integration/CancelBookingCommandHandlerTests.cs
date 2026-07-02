using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class CancelBookingCommandHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task Cancel_ExistingPendingBooking_ShouldSucceed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CancelBookingCommand, Result>>();
        var command = new CancelBookingCommand(booking.Id, "Changed mind", false, null, clientId, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var updated = await db.Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id);
        updated.Status.Should().Be(EBookingStatus.Cancelled);
        updated.CancellationReason.Should().Be("Changed mind");
    }

    [Fact]
    public async Task Cancel_ByProviderOwner_ShouldSucceed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CancelBookingCommand, Result>>();
        var command = new CancelBookingCommand(booking.Id, "Provider cancelled", false, providerId, null, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Cancel_NonExistingBooking_ShouldReturnNotFound()
    {
        // Arrange
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CancelBookingCommand, Result>>();
        var command = new CancelBookingCommand(Guid.NewGuid(), "reason", false, null, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Cancel_ByUnauthorizedUser_ShouldReturnForbidden()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CancelBookingCommand, Result>>();
        var command = new CancelBookingCommand(booking.Id, "Unauthorized", false, null, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(403);
    }
}
