using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Handlers.Queries;

[Collection("BookingsIntegrationTests")]
public class GetBookingsByProviderQueryHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task GetBookings_WithBookings_ShouldReturnList()
    {
        // Arrange
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

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBookings_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<ModuleBookingDto>>>>();
        var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookings_IsolatedByProvider_ShouldNotReturnOtherProvidersBookings()
    {
        // Arrange
        var providerA = Guid.NewGuid();
        var providerB = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        GetMockProvidersApi().SeedProvider(providerA, Guid.NewGuid());
        GetMockProvidersApi().SeedProvider(providerB, Guid.NewGuid());
        GetMockServiceCatalogsApi().SeedService(serviceId, Guid.NewGuid());
        await CreateScheduleAsync(providerA);
        await CreateScheduleAsync(providerB);

        await CreateBookingAsync(providerA, clientId, serviceId);
        await CreateBookingAsync(providerB, clientId, serviceId);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<ModuleBookingDto>>>>();
        var query = new GetBookingsByProviderQuery(providerA, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.Should().OnlyContain(b => b.ProviderId == providerA);
    }
}
