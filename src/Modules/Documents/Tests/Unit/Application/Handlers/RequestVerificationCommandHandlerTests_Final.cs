using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Shared.Database;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers;

public class RequestVerificationCommandHandlerTests_Final
{
    private readonly Mock<IUnitOfWork> _mockUow = new();
    private readonly Mock<IDocumentQueries> _mockDocumentQueries = new();
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly RequestVerificationCommandHandler _handler;

    public RequestVerificationCommandHandlerTests_Final()
    {
        _handler = new RequestVerificationCommandHandler(_mockUow.Object, _mockDocumentQueries.Object, _mockHttpContextAccessor.Object, new Mock<Microsoft.Extensions.Logging.ILogger<RequestVerificationCommandHandler>>().Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNotOwnerAndNotAdmin_ShouldReturnNotFound()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        
        var httpContext = new DefaultHttpContext();
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("sub", userId.ToString()) }, "TestAuth"));
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var command = new RequestVerificationCommand(documentId);
        var result = await _handler.HandleAsync(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }
}
