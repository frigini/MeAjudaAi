using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Persistence;

public class EmailTemplateRepositoryTests
{
    [Fact]
    public async Task GetActiveByKeyAsync_ShouldReturnOverride_WhenAvailable()
    {
        // Arrange
        using var context = CommunicationsTestDb.CreateSqlite();
        var repo = new EmailTemplateRepository(context);
        
        var baseTemplate = EmailTemplate.Create("welcome", "Base", "Html", "Text", "pt-br");
        var overrideTemplate = EmailTemplate.Create("welcome", "Override", "Html", "Text", "pt-br", overrideKey: "welcome");

        await repo.AddAsync(baseTemplate);
        await repo.AddAsync(overrideTemplate);
        // repo.AddAsync already calls SaveChangesAsync

        // Act
        var result = await repo.GetActiveByKeyAsync("welcome", "pt-br");

        // Assert
        result.Should().NotBeNull();
        result!.OverrideKey.Should().Be("welcome");
    }

    [Fact]
    public async Task GetActiveByKeyAsync_ShouldReturnBase_WhenOverrideKeyIsNull()
    {
        // Arrange (Given)
        using var context = CommunicationsTestDb.CreateSqlite();
        var repo = new EmailTemplateRepository(context);
        
        var baseTemplate = EmailTemplate.Create("welcome", "Base", "Html", "Text", "pt-br");
        await repo.AddAsync(baseTemplate);
        // repo.AddAsync already calls SaveChangesAsync

        // Act (When)
        var result = await repo.GetActiveByKeyAsync("welcome", "pt-br");

        // Assert (Then)
        result.Should().NotBeNull();
        result!.OverrideKey.Should().BeNull();
        result.TemplateKey.Should().Be("welcome");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenTemplateIsSystem()
    {
        // Arrange
        using var context = CommunicationsTestDb.CreateSqlite();
        var repo = new EmailTemplateRepository(context);
        var systemTemplate = EmailTemplate.Create("sys", "Sys", "Html", "Text", "en", isSystemTemplate: true);
        
        await repo.AddAsync(systemTemplate);
        // repo.AddAsync already calls SaveChangesAsync

        // Act
        var act = () => repo.DeleteAsync(systemTemplate.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
