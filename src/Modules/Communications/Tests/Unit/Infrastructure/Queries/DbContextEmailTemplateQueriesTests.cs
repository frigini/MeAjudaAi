using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Infrastructure")]
public class DbContextEmailTemplateQueriesTests : BaseInMemoryDatabaseTest<CommunicationsDbContext>
{
    private readonly DbContextEmailTemplateQueries _queries;

    public DbContextEmailTemplateQueriesTests() : base(options => new CommunicationsDbContext(options))
    {
        _queries = new DbContextEmailTemplateQueries(DbContext);
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithOverride_ShouldReturnOverrideTemplate()
    {
        var baseTemplate = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br", null, true);
        var overrideTemplate = EmailTemplate.Create("welcome", "Subject", "<html>override</html>", "override", "pt-br", "custom_key", true);
        DbContext.EmailTemplates.AddRange(baseTemplate, overrideTemplate);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetActiveByKeyAsync("custom_key", "pt-BR");

        result.Should().NotBeNull();
        result!.TemplateKey.Should().Be("welcome");
        result!.OverrideKey.Should().Be("custom_key");
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithNoOverride_ShouldFallbackToBase()
    {
        var baseTemplate = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br", null, true);
        DbContext.EmailTemplates.Add(baseTemplate);
        await DbContext.SaveChangesAsync();

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
        DbContext.EmailTemplates.AddRange(
            EmailTemplate.Create("welcome", "Subject", "<html>en</html>", "en", "en", null, true),
            EmailTemplate.Create("welcome", "Subject", "<html>pt</html>", "pt", "pt-br", null, true));
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllByKeyAsync("welcome");

        result.Should().HaveCount(2);
        result[0].Language.Should().Be("en");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTemplatesOrderedByKeyThenLanguage()
    {
        DbContext.EmailTemplates.AddRange(
            EmailTemplate.Create("ztemplate", "Subject", "<html>z</html>", "z", "pt-br", null, true),
            EmailTemplate.Create("atemplate", "Subject", "<html>a</html>", "a", "pt-br", null, true));
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].TemplateKey.Should().Be("atemplate");
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithNullLanguage_ShouldFallbackToDefault()
    {
        var template = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br", null, true);
        DbContext.EmailTemplates.Add(template);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetActiveByKeyAsync("welcome", null);

        result.Should().NotBeNull();
        result!.TemplateKey.Should().Be("welcome");
    }

    [Fact]
    public async Task GetAllByKeyAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _queries.GetAllByKeyAsync(""));
    }

    [Fact]
    public async Task GetAllAsync_WithNoActiveTemplates_ShouldReturnEmpty()
    {
        var inactive = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br");
        DbContext.EmailTemplates.Add(inactive);
        await DbContext.SaveChangesAsync();

        inactive.Deactivate();
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeInactiveTemplates()
    {
        var active = EmailTemplate.Create("active", "Subject", "<html>active</html>", "active", "pt-br");
        var inactive = EmailTemplate.Create("inactive", "Subject", "<html>inactive</html>", "inactive", "pt-br");
        DbContext.EmailTemplates.AddRange(active, inactive);
        await DbContext.SaveChangesAsync();

        inactive.Deactivate();
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(1);
        result[0].TemplateKey.Should().Be("active");
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithInactiveOverride_ShouldFallbackToBase()
    {
        var baseTemplate = EmailTemplate.Create("welcome", "Subject", "<html>base</html>", "base", "pt-br", null);
        var overrideTemplate = EmailTemplate.Create("welcome", "Subject", "<html>override</html>", "override", "pt-br", "custom_key");
        DbContext.EmailTemplates.AddRange(baseTemplate, overrideTemplate);
        await DbContext.SaveChangesAsync();

        overrideTemplate.Deactivate();
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetActiveByKeyAsync("custom_key", "pt-BR");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WhenMultipleActiveVersions_ShouldReturnHighestVersion()
    {
        var v1 = EmailTemplate.Create("welcome", "Subject v1", "<html>v1</html>", "v1", "pt-br");
        DbContext.EmailTemplates.Add(v1);
        await DbContext.SaveChangesAsync();

        var v2 = v1.CreateNewVersion("Subject v2", "<html>v2</html>", "v2");
        DbContext.EmailTemplates.Add(v2);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetActiveByKeyAsync("welcome", "pt-BR");

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Subject v2");
    }

    [Fact]
    public async Task GetAllByKeyAsync_ShouldReturnAllVersions()
    {
        DbContext.EmailTemplates.AddRange(
            EmailTemplate.Create("welcome", "Subject", "<html>v1</html>", "v1", "pt-br", null),
            EmailTemplate.Create("welcome", "Subject", "<html>v2</html>", "v2", "pt-br", null));
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllByKeyAsync("welcome");

        result.Should().HaveCount(2);
    }
}
