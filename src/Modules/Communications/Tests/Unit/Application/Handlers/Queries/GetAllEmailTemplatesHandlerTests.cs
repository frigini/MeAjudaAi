using MeAjudaAi.Modules.Communications.Application.Handlers.Queries;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

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
            new EmailTemplateBuilder().WithKey("template1").WithSubject("subject1").WithHtmlBody("body1").WithLanguage("en").Build(),
            new EmailTemplateBuilder().WithKey("template2").WithSubject("subject2").WithHtmlBody("body2").WithLanguage("en").Build()
        };
        _emailTemplateQueriesMock.Setup(q => q.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var query = new GetAllEmailTemplatesQuery(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(2);
        result.Value.Should().BeEquivalentTo(templates);
    }
}
