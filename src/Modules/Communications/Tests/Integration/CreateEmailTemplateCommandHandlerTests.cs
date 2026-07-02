using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration;

public class CreateEmailTemplateCommandHandlerTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateEmailTemplateCommand, Result<Guid>>>();
        var command = new CreateEmailTemplateCommand(
            "test_template",
            "Test Subject",
            "<p>Test HTML</p>",
            "Test Text",
            false,
            "pt-BR",
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var template = await db.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == result.Value);
        template.Should().NotBeNull();
        template!.TemplateKey.Should().Be("test_template");
    }

    [Fact]
    public async Task Create_WithDuplicateKey_ShouldSucceed()
    {
        // Arrange
        var key = $"duplicate_{Guid.NewGuid()}";

        using var scope1 = CreateScope();
        var handler1 = scope1.ServiceProvider.GetRequiredService<ICommandHandler<CreateEmailTemplateCommand, Result<Guid>>>();
        await handler1.HandleAsync(new CreateEmailTemplateCommand(key, "Subject 1", "<p>HTML 1</p>", "Text 1", false, "pt-BR", Guid.NewGuid()), CancellationToken.None);

        using var scope2 = CreateScope();
        var handler2 = scope2.ServiceProvider.GetRequiredService<ICommandHandler<CreateEmailTemplateCommand, Result<Guid>>>();
        var command2 = new CreateEmailTemplateCommand(key, "Subject 2", "<p>HTML 2</p>", "Text 2", false, "en-US", Guid.NewGuid());

        // Act
        var result = await handler2.HandleAsync(command2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var templates = await db.EmailTemplates.AsNoTracking().Where(t => t.TemplateKey == key).ToListAsync();
        templates.Should().HaveCount(2);
    }
}
