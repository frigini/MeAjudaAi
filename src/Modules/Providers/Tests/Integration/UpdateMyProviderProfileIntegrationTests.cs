using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

public class UpdateMyProviderProfileIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task UpdateMyProfile_WithValidData_ShouldUpdateProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originalName = "Original Name";
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName(originalName)
            .WithType(EProviderType.Individual)
            .Build();

        await DbContext.Providers.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var newName = "Updated Name";
        var businessProfileDto = new BusinessProfileDtoBuilder()
            .WithLegalName("Legal Name")
            .WithFantasyName("Fantasy Name")
            .WithDescription("Updated Description")
            .WithEmail("new@email.com")
            .Build();

        var command = new UpdateProviderProfileCommand(provider.Id.Value, newName, businessProfileDto, new List<ProviderServiceDto>());

        using var scope = CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        // Act
        var result = await commandDispatcher.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        result.Value.BusinessProfile.Description.Should().Be("Updated Description");

        // Verify DB with cold read (new scope to avoid cached entity)
        using var verifyScope = CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var updatedProvider = await verifyDbContext.Providers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == provider.Id);
        updatedProvider.Should().NotBeNull();
        updatedProvider!.Name.Should().Be(newName);
        updatedProvider.BusinessProfile.Description.Should().Be("Updated Description");
    }
}
