using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class GetBookingByIdQueryHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task GetById_ExistingBooking_ShouldReturnDto()
    {
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);
        await CreateScheduleAsync(providerId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingByIdQuery, Result<ModuleBookingDto>>>();
        var query = new GetBookingByIdQuery(booking.Id, clientId, null, false, Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetById_NonExistingBooking_ShouldReturnNotFound()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingByIdQuery, Result<ModuleBookingDto>>>();
        var query = new GetBookingByIdQuery(Guid.NewGuid(), Guid.NewGuid(), null, false, Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetById_UnauthorizedUser_ShouldReturnNotFound()
    {
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());

        var booking = await CreateBookingAsync(providerId, clientId, serviceId);
        await CreateScheduleAsync(providerId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingByIdQuery, Result<ModuleBookingDto>>>();
        var query = new GetBookingByIdQuery(booking.Id, Guid.NewGuid(), null, false, Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }
}
