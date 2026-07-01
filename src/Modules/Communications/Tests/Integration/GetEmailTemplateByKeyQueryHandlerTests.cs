using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration;

public class GetEmailTemplateByKeyQueryHandlerTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task GetByKey_ExistingTemplate_ShouldReturnDto()
    {
        var key = $"key_test_{Guid.NewGuid():N}";
        await CreateEmailTemplateAsync(templateKey: key);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetEmailTemplateByKeyQuery, Result<EmailTemplate?>>>();
        var query = new GetEmailTemplateByKeyQuery(key, "pt-BR", Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TemplateKey.Should().Be(key);
    }

    [Fact]
    public async Task GetByKey_NonExistingTemplate_ShouldReturnNull()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetEmailTemplateByKeyQuery, Result<EmailTemplate?>>>();
        var query = new GetEmailTemplateByKeyQuery("nonexistent", "pt-BR", Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetByKey_InactiveTemplate_ShouldReturnNull()
    {
        var key = $"inactive_test_{Guid.NewGuid():N}";
        await CreateEmailTemplateAsync(templateKey: key, isActive: false);

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetEmailTemplateByKeyQuery, Result<EmailTemplate?>>>();
        var query = new GetEmailTemplateByKeyQuery(key, "pt-BR", Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
