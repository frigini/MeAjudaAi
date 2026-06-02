using FluentAssertions;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Infrastructure")]
public class DbContextEmailTemplateQueriesTests : IDisposable
{
    private readonly CommunicationsDbContext _db;
    private readonly DbContextEmailTemplateQueries _queries;

    public DbContextEmailTemplateQueriesTests()
    {
        _db = CommunicationsTestDb.CreateSqlite();
        _queries = new DbContextEmailTemplateQueries(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetActiveByKeyAsync_WithOverride_ShouldReturnOverrideTemplate()
    {
        var baseTemplate = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br", null, true);
        var overrideTemplate = EmailTemplate.Create("welcome", "Subject", "<html>override</html>", "override", "pt-br", "custom_key", true);
        _db.EmailTemplates.AddRange(baseTemplate, overrideTemplate);
        await _db.SaveChangesAsync();

        var result = await _queries.GetActiveByKeyAsync("custom_key", "pt-BR");

        result.Should().NotBeNull();
        result!.TemplateKey.Should().Be("welcome");
        result!.OverrideKey.Should().Be("custom_key");
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithNoOverride_ShouldFallbackToBase()
    {
        var baseTemplate = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br", null, true);
        _db.EmailTemplates.Add(baseTemplate);
        await _db.SaveChangesAsync();

        var result = await _queries.GetActiveByKeyAsync("welcome", "pt-BR");

        result.Should().NotBeNull();
        result!.TemplateKey.Should().Be("welcome");
        result!.OverrideKey.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _queries.GetActiveByKeyAsync(""));
    }

    [Fact]
    public async Task GetAllByKeyAsync_ShouldReturnTemplatesOrderedByLanguageThenVersionDesc()
    {
        _db.EmailTemplates.AddRange(
            EmailTemplate.Create("welcome", "Subject", "<html>en</html>", "en", "en", null, true),
            EmailTemplate.Create("welcome", "Subject", "<html>pt</html>", "pt", "pt-br", null, true));
        await _db.SaveChangesAsync();

        var result = await _queries.GetAllByKeyAsync("welcome");

        result.Should().HaveCount(2);
        result[0].Language.Should().Be("en");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTemplatesOrderedByKeyThenLanguage()
    {
        _db.EmailTemplates.AddRange(
            EmailTemplate.Create("ztemplate", "Subject", "<html>z</html>", "z", "pt-br", null, true),
            EmailTemplate.Create("atemplate", "Subject", "<html>a</html>", "a", "pt-br", null, true));
        await _db.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].TemplateKey.Should().Be("atemplate");
    }
}
