using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration;

public class UpdateEmailTemplateCommandHandlerTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task Update_ExistingTemplate_ShouldCreateNewVersion()
    {
        var template = await CreateEmailTemplateAsync();

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateEmailTemplateCommand, Result>>();
        var command = new UpdateEmailTemplateCommand(template.Id, "Updated Subject", "<p>Updated HTML</p>", "Updated Text", Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var templates = await db.EmailTemplates.AsNoTracking()
            .Where(t => t.TemplateKey == template.TemplateKey)
            .ToListAsync();
        templates.Should().HaveCount(2);
        templates.Should().Contain(t => t.Version == 1 && !t.IsActive);
        templates.Should().Contain(t => t.Version == 2 && t.IsActive);
    }

    [Fact]
    public async Task Update_NonExistingTemplate_ShouldReturnNotFound()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateEmailTemplateCommand, Result>>();
        var command = new UpdateEmailTemplateCommand(Guid.NewGuid(), "Subject", "<p>HTML</p>", "Text", Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Update_SystemTemplate_ShouldReturnBadRequest()
    {
        var template = await CreateEmailTemplateAsync();
        template.IsSystemTemplate.Should().BeFalse();

        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var systemTemplate = EmailTemplate.Create("system_test", "System", "<p>System</p>", "System", "pt-BR", null, true);
        context.EmailTemplates.Add(systemTemplate);
        await context.SaveChangesAsync();

        using var updateScope = CreateScope();
        var handler = updateScope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateEmailTemplateCommand, Result>>();
        var command = new UpdateEmailTemplateCommand(systemTemplate.Id, "Updated", "<p>Updated</p>", "Updated", Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
