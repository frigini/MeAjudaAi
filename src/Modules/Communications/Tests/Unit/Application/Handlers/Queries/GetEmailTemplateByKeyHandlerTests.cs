using FluentAssertions;
using MeAjudaAi.Modules.Communications.Application.Handlers.Queries;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Queries;

public class GetEmailTemplateByKeyHandlerTests
{
    private readonly Mock<IEmailTemplateQueries> _emailTemplateQueriesMock;
    private readonly GetEmailTemplateByKeyHandler _handler;

    public GetEmailTemplateByKeyHandlerTests()
    {
        _emailTemplateQueriesMock = new Mock<IEmailTemplateQueries>();
        _handler = new GetEmailTemplateByKeyHandler(_emailTemplateQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WithTemplate_WhenTemplateExists()
    {
        // Arrange
        var template = EmailTemplate.Create("template1", "subject1", "body1", "en");
        _emailTemplateQueriesMock.Setup(q => q.GetActiveByKeyAsync("template1", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetEmailTemplateByKeyQuery("template1", "en");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(template);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WithNull_WhenTemplateDoesNotExist()
    {
        // Arrange
        _emailTemplateQueriesMock.Setup(q => q.GetActiveByKeyAsync("template1", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);

        var query = new GetEmailTemplateByKeyQuery("template1", "en");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
