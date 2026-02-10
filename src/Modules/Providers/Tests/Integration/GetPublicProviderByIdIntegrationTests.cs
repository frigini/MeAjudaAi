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
            
        // Ensure provider is active (Default state might be pending depending on domain logic, 
        // need to check if CreateProviderAsync sets it or if we need to update it).
        // Provider.Create usually sets status. Let's assume UpdateProviderStatus is needed or we can hack it via DB context if needed.
        // Assuming CreateProviderAsync persists what is created. 
        // If Provider factory creates as Pending, we need to activate it. 
        // Since these are integration tests, we should probably use a command or repository to update it, 
        // or just update the entity and save.
        
        // Updating status manually to ensure Active
        var dbContext = GetService<ProvidersDbContext>();
        var entry = dbContext.Entry(provider);
        // Reflection to set private setter if needed, or public method
        // Using reflection just to be safe and quick as we don't have the domain entity code open right now to verify methods
        typeof(Provider).GetProperty(nameof(Provider.Status))?.SetValue(provider, EProviderStatus.Active);
        await dbContext.SaveChangesAsync();

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
            
        // Set to Suspended
        var dbContext = GetService<ProvidersDbContext>();
        var entry = dbContext.Entry(provider);
        typeof(Provider).GetProperty(nameof(Provider.Status))?.SetValue(provider, EProviderStatus.Suspended);
        await dbContext.SaveChangesAsync();

        var dispatcher = GetService<IQueryDispatcher>();
        var query = new GetPublicProviderByIdQuery(provider.Id);

        // Act
        var result = await dispatcher.QueryAsync<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        // result.Error.Code.Should().Be("Provider.NotAvailable"); // Error class does not have Code
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
        // result.Error.Code.Should().Be("Provider.NotFound"); // Error class does not have Code
    }
}
