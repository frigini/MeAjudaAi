using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Handlers;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Handlers;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Review, ReviewId>> _repositoryMock;
    private readonly Mock<IReviewQueries> _queriesMock;
    private readonly Mock<IContentModerator> _moderatorMock;
    private readonly Mock<ILogger<CreateReviewCommandHandler>> _loggerMock;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Review, ReviewId>>();
        _queriesMock = new Mock<IReviewQueries>();
        _moderatorMock = new Mock<IContentModerator>();
        _loggerMock = new Mock<ILogger<CreateReviewCommandHandler>>();
        
        _uowMock.Setup(u => u.GetRepository<Review, ReviewId>()).Returns(_repositoryMock.Object);
        _moderatorMock.Setup(m => m.IsClean(It.IsAny<string>())).Returns(true);
        
        _handler = new CreateReviewCommandHandler(
            _uowMock.Object, 
            _queriesMock.Object, 
            _moderatorMock.Object, 
            _loggerMock.Object);
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
        _repositoryMock.Verify(r => r.Add(It.IsAny<Review>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Rating5WithoutComment_ShouldAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, null);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Approved)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Rating4WithoutComment_ShouldAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 4, null);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Approved)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Rating5WithCleanComment_ShouldNotAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "Great service!");
        _moderatorMock.Setup(m => m.IsClean(It.IsAny<string>())).Returns(true);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Rating3WithoutComment_ShouldNotAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 3, null);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Rating5WithWhitespaceComment_ShouldAutoApprove()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "   ");

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Approved)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DirtyContent_ShouldMarkAsFlagged()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "Some bad word");
        _moderatorMock.Setup(m => m.IsClean(It.IsAny<string>())).Returns(false);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(
            It.Is<Review>(rev => rev.Status == EReviewStatus.Flagged)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DuplicateReview_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var command = new CreateReviewCommand(providerId, customerId, 5, null);
        
        _queriesMock.Setup(r => r.GetByProviderAndCustomerAsync(providerId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Review.Create(providerId, customerId, 1, "Existing"));

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Você já avaliou este prestador.");
    }
}
