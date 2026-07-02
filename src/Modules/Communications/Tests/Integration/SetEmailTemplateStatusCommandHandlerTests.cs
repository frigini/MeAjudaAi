using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration;

public class SetEmailTemplateStatusCommandHandlerTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task Deactivate_ActiveTemplate_ShouldSucceed()
    {
        // Arrange
        var template = await CreateEmailTemplateAsync();

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(template.Id, false, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var updated = await db.EmailTemplates.AsNoTracking().FirstAsync(t => t.Id == template.Id);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_InactiveTemplate_ShouldSucceed()
    {
        // Arrange
        var template = await CreateEmailTemplateAsync(isActive: false);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(template.Id, true, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var updated = await db.EmailTemplates.AsNoTracking().FirstAsync(t => t.Id == template.Id);
        updated.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Deactivate_SystemTemplate_ShouldReturnBadRequest()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var systemTemplate = EmailTemplate.Create("system_deactivate", "System", "<p>System</p>", "System", "pt-BR", null, true);
        context.EmailTemplates.Add(systemTemplate);
        await context.SaveChangesAsync();

        using var updateScope = CreateScope();
        var handler = updateScope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(systemTemplate.Id, false, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_NonExistingTemplate_ShouldReturnNotFound()
    {
        // Arrange
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(Guid.NewGuid(), false, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
