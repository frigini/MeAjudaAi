using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration.Infrastructure;

public abstract class CommunicationsIntegrationTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"communications_test_{GetType().Name}",
                Username = "test_user",
                Password = "test_password",
                Schema = "communications"
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddCommunicationsTestInfrastructure(options);
    }

    protected async Task<EmailTemplate> CreateEmailTemplateAsync(
        string? templateKey = null,
        string? subject = null,
        string? htmlBody = null,
        string? textBody = null,
        string? language = null,
        bool isActive = true,
        CancellationToken cancellationToken = default)
    {
        var template = EmailTemplate.Create(
            templateKey ?? $"test_template_{Guid.NewGuid():N}",
            subject ?? "Test Subject",
            htmlBody ?? "<p>Test HTML</p>",
            textBody ?? "Test Text",
            language ?? "pt-BR",
            null,
            false);

        if (!isActive)
        {
            template.Deactivate();
        }

        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        context.EmailTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);
        return template;
    }
}
