using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Handlers;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Handlers;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _repositoryMock;
    private readonly Mock<IContentModerator> _moderatorMock;
    private readonly Mock<ILogger<CreateReviewCommandHandler>> _loggerMock;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _repositoryMock = new Mock<IReviewRepository>();
        _moderatorMock = new Mock<IContentModerator>();
        _loggerMock = new Mock<ILogger<CreateReviewCommandHandler>>();
        
        _moderatorMock.Setup(m => m.IsClean(It.IsAny<string>())).Returns(true);
        
        _handler = new CreateReviewCommandHandler(_repositoryMock.Object, _moderatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldAddReviewToRepository()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "Great!");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Rating5WithoutComment_ShouldAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, null);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Approved), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DirtyContent_ShouldNotAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "Some bad word");
        _moderatorMock.Setup(m => m.IsClean(It.IsAny<string>())).Returns(false);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Pending), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
