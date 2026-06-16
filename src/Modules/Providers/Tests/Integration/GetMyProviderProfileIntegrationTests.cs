using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

public class GetMyProviderProfileIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task GetMyProfile_WhenProviderExists_ShouldReturnProviderDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithType(EProviderType.Individual)
            .Build();

        await DbContext.Providers.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        using var scope = CreateScope();
        var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        var query = new GetProviderByUserIdQuery(userId);

        // Act
        var result = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);
        result.Value.UserId.Should().Be(userId);
        result.Value.Name.Should().Be(provider.Name);
    }

    [Fact]
    public async Task GetMyProfile_WhenProviderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        using var scope = CreateScope();
        var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        var query = new GetProviderByUserIdQuery(userId);

        // Act
        var result = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
