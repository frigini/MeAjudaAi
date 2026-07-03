using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
[Collection("BookingsIntegrationTests")]
public class DbContextProviderScheduleQueriesTests : BookingsIntegrationTestBase
{
    private DbContextProviderScheduleQueries _queries = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _queries = new DbContextProviderScheduleQueries(serviceProvider.GetRequiredService<BookingsDbContext>());
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingSchedule_ShouldReturnSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .Build();
        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            context.ProviderSchedules.Add(schedule);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _queries.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Act
        var result = await _queries.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().BeNull();
    }
}