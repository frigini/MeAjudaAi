using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Handlers.Queries;

[Collection("BookingsIntegrationTests")]
public class GetProviderAvailabilityQueryHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task GetAvailability_ExistingProvider_ShouldReturnSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());
        await CreateScheduleAsync(providerId);

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetProviderAvailabilityQuery, Result<AvailabilityDto>>>();
        var query = new GetProviderAvailabilityQuery(providerId, tomorrow, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DayOfWeek.Should().Be(tomorrow.DayOfWeek);
    }

    [Fact]
    public async Task GetAvailability_NonExistingProvider_ShouldReturnNotFound()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetProviderAvailabilityQuery, Result<AvailabilityDto>>>();
        var query = new GetProviderAvailabilityQuery(Guid.NewGuid(), tomorrow, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }
}
