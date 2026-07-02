using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Tests.Integration;

public class GetAllEmailTemplatesQueryHandlerTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task GetAll_WithTemplates_ShouldReturnList()
    {
        // Arrange
        await CreateEmailTemplateAsync(templateKey: "template1");
        await CreateEmailTemplateAsync(templateKey: "template2");

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllEmailTemplatesQuery, Result<IReadOnlyList<EmailTemplate>>>>();
        var query = new GetAllEmailTemplatesQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllEmailTemplatesQuery, Result<IReadOnlyList<EmailTemplate>>>>();
        var query = new GetAllEmailTemplatesQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
