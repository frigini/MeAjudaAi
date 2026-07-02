using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class ConfirmBookingCommandHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task Confirm_ExistingPendingBooking_ShouldSucceed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ConfirmBookingCommand, Result>>();
        var command = new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var updated = await db.Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id);
        updated.Status.Should().Be(EBookingStatus.Confirmed);
    }

    [Fact]
    public async Task Confirm_NonExistingBooking_ShouldReturnNotFound()
    {
        // Arrange
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ConfirmBookingCommand, Result>>();
        var command = new ConfirmBookingCommand(Guid.NewGuid(), false, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Confirm_ByNonOwnerProvider_ShouldReturnForbidden()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ConfirmBookingCommand, Result>>();
        var command = new ConfirmBookingCommand(booking.Id, false, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Confirm_AlreadyConfirmed_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ConfirmBookingCommand, Result>>();

        var cmd1 = new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid());
        var firstResult = await handler.HandleAsync(cmd1, CancellationToken.None);
        firstResult.IsSuccess.Should().BeTrue("the first confirmation must succeed to establish confirmed state");

        var cmd2 = new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(cmd2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
