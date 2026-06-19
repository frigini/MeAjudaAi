using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers.Commands;

public class DeleteDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<DeleteDocumentCommandHandler>> _mockLogger;
    private readonly DeleteDocumentCommandHandler _handler;

    public DeleteDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<DeleteDocumentCommandHandler>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _handler = new DeleteDocumentCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
            _mockBlobStorage.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    private HttpContext CreateAuthenticatedAdminContext()
    {
        var claims = new List<Claim>
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, RoleConstants.Admin)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }

    private HttpContext CreateAuthenticatedNonAdminContext()
    {
        var claims = new List<Claim>
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "customer")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }

    private static Document CreateTestDocument(string fileUrl = "https://storage/doc.pdf")
    {
        return Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", fileUrl);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteDocumentAndBlob()
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockRepo
            .Setup(x => x.TryFindAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockBlobStorage.Verify(x => x.DeleteAsync(document.FileUrl, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(x => x.Delete(document), Times.Once);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFileUrl_ShouldSkipBlobDeletion()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", "some-url");
        typeof(Document).GetProperty(nameof(Document.FileUrl))!.SetValue(document, string.Empty);
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockRepo
            .Setup(x => x.TryFindAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockBlobStorage.Verify(
            x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepo.Verify(x => x.Delete(document), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var command = new DeleteDocumentCommand(documentId);

        _mockQueries
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Document*not found*");
    }

    [Fact]
    public async Task Handle_WithNullHttpContext_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        var claims = new List<Claim> { new Claim("sub", Guid.NewGuid().ToString()) };
        var identity = new ClaimsIdentity(claims, null);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithNonAdminUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedNonAdminContext());

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenRepositoryNotFound_ShouldStillReturnSuccess()
    {
        // Arrange — document exists in queries but not in repository (edge case)
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockRepo
            .Setup(x => x.TryFindAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockBlobStorage.Verify(x => x.DeleteAsync(document.FileUrl, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(x => x.Delete(It.IsAny<Document>()), Times.Never);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockRepo
            .Setup(x => x.TryFindAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("excluir");
    }

    [Fact]
    public async Task Handle_WhenOperationCanceled_ShouldRethrow()
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockRepo
            .Setup(x => x.TryFindAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.LegacySuperAdmin)]
    public async Task Handle_WithAdminEquivalentRole_ShouldAllowDeletion(string role)
    {
        // Arrange
        var document = CreateTestDocument();
        var command = new DeleteDocumentCommand(document.Id);

        var claims = new List<Claim>
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(httpContext);
        _mockRepo
            .Setup(x => x.TryFindAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
