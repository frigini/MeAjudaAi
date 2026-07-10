using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Documents;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers.Commands;

public class RequestVerificationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IRepository<OutboxMessage, Guid>> _mockOutboxRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RequestVerificationCommandHandler>> _mockLogger;
    private readonly Mock<IStringLocalizer<Strings>> _mockLocalizer;
    private readonly RequestVerificationCommandHandler _handler;

    public RequestVerificationCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockOutboxRepo = new Mock<IRepository<OutboxMessage, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RequestVerificationCommandHandler>>();
        _mockLocalizer = new Mock<IStringLocalizer<Strings>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);
        _mockUow.Setup(x => x.GetRepository<OutboxMessage, Guid>()).Returns(_mockOutboxRepo.Object);

        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "DocumentNotFound"), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"Documento com ID {args[0]} não encontrado."));
        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "HttpContextNotAvailable")])
            .Returns(new LocalizedString("HttpContextNotAvailable", "Contexto HTTP não disponível."));
        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "UserNotAuthenticated")])
            .Returns(new LocalizedString("UserNotAuthenticated", "Usuário não autenticado."));
        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "UserIdNotFoundInToken")])
            .Returns(new LocalizedString("UserIdNotFoundInToken", "ID do usuário não encontrado no token."));
        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "DocumentVerificationNotAllowed")])
            .Returns(new LocalizedString("DocumentVerificationNotAllowed", "Você não tem autorização para solicitar a verificação deste documento."));
        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "DocumentStatusInvalidForVerification"), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"Documento está com status {args[0]} e não pode ser marcado para verificação."));
        _mockLocalizer
            .Setup(x => x[It.Is<string>(s => s == "DocumentVerificationError")])
            .Returns(new LocalizedString("DocumentVerificationError", "Erro ao solicitar verificação do documento, tente novamente mais tarde."));

        _handler = new RequestVerificationCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockLocalizer.Object);
    }

    private void SetupAuthenticatedUser(Guid? userId, string role = "provider", bool isAuthenticated = true)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(AuthConstants.Claims.Subject, userId.Value.ToString()));
        }
        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, isAuthenticated ? "TestAuth" : null);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldMarkAsPendingVerificationAndEnqueueJob()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = new DocumentBuilder().WithProviderId(providerId).AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        
        SetupAuthenticatedUser(providerId);

        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockOutboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => 
            m.Type == OutboxMessageTypes.DocumentVerification && 
            m.Payload.Contains(command.DocumentId.ToString()))), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((Document)null!);

        var command = new RequestVerificationCommand(documentId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("NotFound");
        result.Error!.Message.Should().Contain("não encontrado");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenHttpContextIsNull_ShouldReturnFailure()
    {
        // Arrange
        var document = new DocumentBuilder().AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Unauthorized");
        result.Error!.Message.Should().Contain("não disponível");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        var document = new DocumentBuilder().AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        
        SetupAuthenticatedUser(Guid.NewGuid(), role: "provider", isAuthenticated: false);

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Unauthorized");
        result.Error!.Message.Should().Contain("não autenticado");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdNotFoundInToken_ShouldReturnFailure()
    {
        // Arrange
        var document = new DocumentBuilder().AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        
        SetupAuthenticatedUser(null, role: "provider");

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Unauthorized");
        result.Error!.Message.Should().Contain("não encontrado no token");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDifferentProviderId_AndNotAdmin_ShouldReturnFailure()
    {
        // Arrange
        var documentProviderId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var document = new DocumentBuilder().WithProviderId(documentProviderId).AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        
        SetupAuthenticatedUser(loggedUserId, role: "provider");

        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Unauthorized");
        result.Error!.Message.Should().Contain("não tem autorização");
        
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDifferentProviderId_AndIsAdmin_ShouldSucceed()
    {
        // Arrange
        var documentProviderId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var document = new DocumentBuilder().WithProviderId(documentProviderId).AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        
        SetupAuthenticatedUser(adminId, role: RoleConstants.Admin);

        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsNotEligible_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = new DocumentBuilder().WithProviderId(providerId).AsIdentityDocument().WithFileName("identity.pdf").WithFileUrl("blob-key-123").Build();
        document.MarkAsPendingVerification(); // Status is now PendingVerification

        SetupAuthenticatedUser(providerId);

        _mockQueries.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RequestVerificationCommand(document.Id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("BadRequest");
        result.Error!.Message.Should().Contain("não pode ser marcado para verificação");
        
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUnexpectedExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NpgsqlException("Database connection failure"));

        var command = new RequestVerificationCommand(documentId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("InternalError");
        result.Error!.Message.Should().Contain("tente novamente mais tarde");
        
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenInvalidOperationExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        var command = new RequestVerificationCommand(documentId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("InternalError");
        result.Error!.Message.Should().Contain("tente novamente mais tarde");
        
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOutboxRepo.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.Never);
    }
}