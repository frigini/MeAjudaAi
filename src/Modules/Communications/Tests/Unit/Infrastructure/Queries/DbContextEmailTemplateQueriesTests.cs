using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

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
        var baseTemplate = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>base</html>")
            .WithTextBody("base")
            .WithLanguage("pt-br")
            .AsSystemTemplate()
            .Build();
        var overrideTemplate = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>override</html>")
            .WithTextBody("override")
            .WithLanguage("pt-br")
            .WithOverrideKey("custom_key")
            .AsSystemTemplate()
            .Build();
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
        var baseTemplate = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>base</html>")
            .WithTextBody("base")
            .WithLanguage("pt-br")
            .AsSystemTemplate()
            .Build();
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
            new EmailTemplateBuilder().WithKey("welcome").WithSubject("Subject").WithHtmlBody("<html>en</html>").WithTextBody("en").WithLanguage("en").AsSystemTemplate().Build(),
            new EmailTemplateBuilder().WithKey("welcome").WithSubject("Subject").WithHtmlBody("<html>pt</html>").WithTextBody("pt").WithLanguage("pt-br").AsSystemTemplate().Build());
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllByKeyAsync("welcome");

        result.Should().HaveCount(2);
        result[0].Language.Should().Be("en");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTemplatesOrderedByKeyThenLanguage()
    {
        DbContext.EmailTemplates.AddRange(
            new EmailTemplateBuilder().WithKey("ztemplate").WithSubject("Subject").WithHtmlBody("<html>z</html>").WithTextBody("z").WithLanguage("pt-br").AsSystemTemplate().Build(),
            new EmailTemplateBuilder().WithKey("atemplate").WithSubject("Subject").WithHtmlBody("<html>a</html>").WithTextBody("a").WithLanguage("pt-br").AsSystemTemplate().Build());
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].TemplateKey.Should().Be("atemplate");
    }

    [Fact]
    public async Task GetActiveByKeyAsync_WithNullLanguage_ShouldFallbackToDefault()
    {
        var template = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>base</html>")
            .WithTextBody("base")
            .WithLanguage("pt-br")
            .AsSystemTemplate()
            .Build();
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
        var inactive = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>base</html>")
            .WithTextBody("base")
            .WithLanguage("pt-br")
            .Build();
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
        var active = new EmailTemplateBuilder()
            .WithKey("active")
            .WithSubject("Subject")
            .WithHtmlBody("<html>active</html>")
            .WithTextBody("active")
            .WithLanguage("pt-br")
            .Build();
        var inactive = new EmailTemplateBuilder()
            .WithKey("inactive")
            .WithSubject("Subject")
            .WithHtmlBody("<html>inactive</html>")
            .WithTextBody("inactive")
            .WithLanguage("pt-br")
            .Build();
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
        var baseTemplate = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>base</html>")
            .WithTextBody("base")
            .WithLanguage("pt-br")
            .Build();
        var overrideTemplate = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject")
            .WithHtmlBody("<html>override</html>")
            .WithTextBody("override")
            .WithLanguage("pt-br")
            .WithOverrideKey("custom_key")
            .Build();
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
        var v1 = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("Subject v1")
            .WithHtmlBody("<html>v1</html>")
            .WithTextBody("v1")
            .WithLanguage("pt-br")
            .Build();
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
            new EmailTemplateBuilder().WithKey("welcome").WithSubject("Subject").WithHtmlBody("<html>v1</html>").WithTextBody("v1").WithLanguage("pt-br").Build(),
            new EmailTemplateBuilder().WithKey("welcome").WithSubject("Subject").WithHtmlBody("<html>v2</html>").WithTextBody("v2").WithLanguage("pt-br").Build());
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetAllByKeyAsync("welcome");

        result.Should().HaveCount(2);
    }
}
