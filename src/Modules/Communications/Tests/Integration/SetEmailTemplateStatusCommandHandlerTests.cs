using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration;

public class SetEmailTemplateStatusCommandHandlerTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task Deactivate_ActiveTemplate_ShouldSucceed()
    {
        var template = await CreateEmailTemplateAsync();

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(template.Id, false, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var updated = await db.EmailTemplates.AsNoTracking().FirstAsync(t => t.Id == template.Id);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_InactiveTemplate_ShouldSucceed()
    {
        var template = await CreateEmailTemplateAsync(isActive: false);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(template.Id, true, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var updated = await db.EmailTemplates.AsNoTracking().FirstAsync(t => t.Id == template.Id);
        updated.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Deactivate_SystemTemplate_ShouldReturnBadRequest()
    {
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var systemTemplate = EmailTemplate.Create("system_deactivate", "System", "<p>System</p>", "System", "pt-BR", null, true);
        context.EmailTemplates.Add(systemTemplate);
        await context.SaveChangesAsync();

        using var updateScope = CreateScope();
        var handler = updateScope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(systemTemplate.Id, false, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_NonExistingTemplate_ShouldReturnNotFound()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetEmailTemplateStatusCommand, Result>>();
        var command = new SetEmailTemplateStatusCommand(Guid.NewGuid(), false, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
