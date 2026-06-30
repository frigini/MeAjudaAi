using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

public class GetPublicProviderByIdIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task GetPublicProviderById_ActiveProvider_ShouldReturnDto()
    {
        // Arrange
        var businessProfile = CreateTestBusinessProfile("active@test.com");
        var provider = await CreateProviderAsync(
            Guid.NewGuid(),
            "Active Provider",
            EProviderType.Individual,
            businessProfile);
            
        provider.CompleteBasicInfo();
        provider.Activate();
        
        DbContext.Providers.Update(provider);
        await DbContext.SaveChangesAsync();

        var dispatcher = GetService<IQueryDispatcher>();
        var query = new GetPublicProviderByIdOrSlugQuery(provider.Id.Value.ToString());

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
        result.Value.Name.Should().Be("Active Provider");
        result.Value.FantasyName.Should().Be(businessProfile.FantasyName);
    }

    [Fact]
    public async Task GetPublicProviderById_InactiveProvider_ShouldReturnNotFound()
    {
        // Arrange
        var businessProfile = CreateTestBusinessProfile("suspended@test.com");
        var provider = await CreateProviderAsync(
            Guid.NewGuid(),
            "Suspended Provider",
            EProviderType.Individual,
            businessProfile);
            
        provider.CompleteBasicInfo();
        provider.Activate();
        provider.Suspend("Test suspension");
        
        DbContext.Providers.Update(provider);
        await DbContext.SaveChangesAsync();

        var dispatcher = GetService<IQueryDispatcher>();
        var query = new GetPublicProviderByIdOrSlugQuery(provider.Id.Value.ToString());

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPublicProviderById_NonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        var dispatcher = GetService<IQueryDispatcher>();
        var query = new GetPublicProviderByIdOrSlugQuery(Guid.NewGuid().ToString());

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }
}
