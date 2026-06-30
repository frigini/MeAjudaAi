using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class CompleteBookingCommandHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task Complete_ConfirmedBooking_ShouldSucceed()
    {
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var confirmScope = CreateScope();
        var confirmHandler = confirmScope.ServiceProvider.GetRequiredService<ICommandHandler<ConfirmBookingCommand, Result>>();
        await confirmHandler.HandleAsync(new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid()), CancellationToken.None);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CompleteBookingCommand, Result>>();
        var command = new CompleteBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var updated = await db.Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id);
        updated.Status.Should().Be(EBookingStatus.Completed);
    }

    [Fact]
    public async Task Complete_PendingBooking_ShouldReturnBadRequest()
    {
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CompleteBookingCommand, Result>>();
        var command = new CompleteBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Complete_NonExistingBooking_ShouldReturnNotFound()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CompleteBookingCommand, Result>>();
        var command = new CompleteBookingCommand(Guid.NewGuid(), false, Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }
}
