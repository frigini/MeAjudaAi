using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

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
        var businessProfileDto = new BusinessProfileDto(
            "Legal Name",
            "Fantasy Name",
            "Updated Description",
            new ContactInfoDto("new@email.com", "1234567890", "newsite.com"),
            new AddressDto("Street", "1", "Comp", "Neigh", "City", "ST", "12345678", "BR")
        );

        var command = new UpdateProviderProfileCommand(provider.Id.Value, newName, businessProfileDto);

        using var scope = CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        // Act
        var result = await commandDispatcher.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        result.Value.BusinessProfile.Description.Should().Be("Updated Description");

        // Verify DB
        var dbContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var updatedProvider = await dbContext.Providers.FindAsync(provider.Id);
        updatedProvider.Should().NotBeNull();
        updatedProvider!.Name.Should().Be(newName);
        updatedProvider.BusinessProfile.Description.Should().Be("Updated Description");
    }
}
