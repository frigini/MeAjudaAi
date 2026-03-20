using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

public class ActivateMyProviderProfileIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task ActivateMyProfile_WithValidData_ShouldPersistIsActiveTrue()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithId(Guid.NewGuid())
            .Build();
            
        // Setup initial state as inactive
        provider.DeactivateProfile();

        await DbContext.Providers.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var command = new ActivateProviderProfileCommand(provider.Id.Value);

        using var scope = CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        // Act
        var result = await commandDispatcher.SendAsync<ActivateProviderProfileCommand, Result>(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verificar BD com cold read (novo scope para evitar cached entity)
        using var verifyScope = CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var updatedProvider = await verifyDbContext.Providers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == provider.Id);
        updatedProvider.Should().NotBeNull();
        updatedProvider!.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public async Task ActivateMyProfile_WhenProviderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var command = new ActivateProviderProfileCommand(Guid.NewGuid());

        using var scope = CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        // Act
        var result = await commandDispatcher.SendAsync<ActivateProviderProfileCommand, Result>(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
    }
}
