using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class UploadMyDocumentEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;

    public UploadMyDocumentEndpointTests()
    {
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
    }

    private static System.Reflection.MethodInfo UploadDocumentMethod()
    {
        var method = typeof(UploadMyDocumentEndpoint).GetMethod(
            "UploadMyDocumentAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull("UploadMyDocumentAsync must exist as a private static method on UploadMyDocumentEndpoint");
        return method!;
    }

    [Fact]
    public async Task UploadDocumentAsync_WithValidRequest_ShouldUploadAndReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        
        var request = new AddDocumentRequest("12345678909", EDocumentType.CPF);
        
        var providerDto = new ProviderDto(
            providerId, userId, "Test", EProviderType.Individual, null!, 
            EProviderStatus.PendingBasicInfo, EVerificationStatus.Pending, EProviderTier.Standard,
            new List<DocumentDto>(), new List<QualificationDto>(), new List<ProviderServiceDto>(), DateTime.UtcNow, null, false, null, null, null);

        // Mock Query (Get provider by user id)
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Mock Command (Add document)
        var commandResult = Result<ProviderDto>.Success(providerDto);
        _commandDispatcherMock
            .Setup(x => x.SendAsync<AddDocumentCommand, Result<ProviderDto>>(
                It.Is<AddDocumentCommand>(c => c.ProviderId == providerId && c.DocumentNumber == request.Number), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResult);

        // Act
        var methodInfo = UploadDocumentMethod();
        // Corrected parameter order: Context, Request, QueryDispatcher, CommandDispatcher, CancellationToken
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, request, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto>>>();
        var okResult = (Ok<Result<ProviderDto>>)result;
        okResult.Value.Should().NotBeNull();
        okResult.Value!.IsSuccess.Should().BeTrue();
        // okResult.Value.Value.Should().BeEquivalentTo(providerDto); // Optional verification
        
        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        
        _commandDispatcherMock.Verify(x => x.SendAsync<AddDocumentCommand, Result<ProviderDto>>(
                It.Is<AddDocumentCommand>(c => c.ProviderId == providerId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        var request = new AddDocumentRequest("12345678909", EDocumentType.CPF);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var methodInfo = UploadDocumentMethod();
        // Corrected parameter order
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, request, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        // Corrected expected type: NotFound<Response<object>> instead of NotFound<string>
        result.Should().BeOfType<NotFound<Response<object>>>();
        
        _commandDispatcherMock.Verify(x => x.SendAsync<AddDocumentCommand, Result<ProviderDto>>(
                It.IsAny<AddDocumentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadDocumentAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        var request = new AddDocumentRequest("12345678909", EDocumentType.CPF);
        
        var providerDto = new ProviderDto(
            providerId, userId, "Test", EProviderType.Individual, null!, 
            EProviderStatus.PendingBasicInfo, EVerificationStatus.Pending, EProviderTier.Standard,
            new List<DocumentDto>(), new List<QualificationDto>(), new List<ProviderServiceDto>(), DateTime.UtcNow, null, false, null, null, null);

        _queryDispatcherMock.Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        _commandDispatcherMock.Setup(x => x.SendAsync<AddDocumentCommand, Result<ProviderDto>>(
                It.IsAny<AddDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure("Invalid document"));

        // Act
        var methodInfo = UploadDocumentMethod();
        // Corrected parameter order
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, request, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        // Corrected expected type: BadRequest<Result<ProviderDto>>
        result.Should().BeOfType<BadRequest<Result<ProviderDto>>>();
    }
}
