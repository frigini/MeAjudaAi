using FluentAssertions;
using MeAjudaAi.Modules.Communications.Application.Handlers.Queries;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Queries;

public class GetAllEmailTemplatesHandlerTests
{
    private readonly Mock<IEmailTemplateQueries> _emailTemplateQueriesMock;
    private readonly GetAllEmailTemplatesHandler _handler;

    public GetAllEmailTemplatesHandlerTests()
    {
        _emailTemplateQueriesMock = new Mock<IEmailTemplateQueries>();
        _handler = new GetAllEmailTemplatesHandler(_emailTemplateQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WithTemplatesList()
    {
        // Arrange
        var templates = new List<EmailTemplate>
        {
            EmailTemplate.Create("template1", "subject1", "body1", "en"),
            EmailTemplate.Create("template2", "subject2", "body2", "en")
        };
        _emailTemplateQueriesMock.Setup(q => q.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var query = new GetAllEmailTemplatesQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(2);
        result.Value.Should().BeEquivalentTo(templates);
    }
}
