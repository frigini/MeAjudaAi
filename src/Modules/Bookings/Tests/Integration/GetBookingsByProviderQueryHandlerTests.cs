using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class GetBookingsByProviderQueryHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task GetBookings_WithBookings_ShouldReturnList()
    {
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());
        await CreateScheduleAsync(providerId);

        await CreateBookingAsync(providerId, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<ModuleBookingDto>>>>();
        var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBookings_EmptyResult_ShouldReturnEmptyList()
    {
        var providerId = Guid.NewGuid();
        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<ModuleBookingDto>>>>();
        var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }
}
