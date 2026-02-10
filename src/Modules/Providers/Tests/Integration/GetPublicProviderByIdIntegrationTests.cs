using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection; // For GetRequiredService

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

public class GetPublicProviderByIdIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task GetPublicProviderById_ActiveProvider_ShouldReturnDto()
    {
        // Arrange
        await CleanupDatabase();
        
        var businessProfile = CreateTestBusinessProfile("active@test.com");
        var provider = await CreateProviderAsync(
            Guid.NewGuid(),
            "Active Provider",
            EProviderType.Individual,
            businessProfile);
            
        // Bypass domain transitions to set required state for test
        typeof(Provider).GetProperty(nameof(Provider.Status))?.SetValue(provider, EProviderStatus.Active);
        typeof(Provider).GetProperty(nameof(Provider.VerificationStatus))?.SetValue(provider, EVerificationStatus.Verified);
        
        DbContext.Providers.Update(provider);
        await DbContext.SaveChangesAsync();

        var dispatcher = GetService<IQueryDispatcher>();
        var query = new GetPublicProviderByIdQuery(provider.Id);

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

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
        await CleanupDatabase();
        
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
        var query = new GetPublicProviderByIdQuery(provider.Id);

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPublicProviderById_NonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        await CleanupDatabase();
        var dispatcher = GetService<IQueryDispatcher>();
        var query = new GetPublicProviderByIdQuery(Guid.NewGuid());

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }
}
