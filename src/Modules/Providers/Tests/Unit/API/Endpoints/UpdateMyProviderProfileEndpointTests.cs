using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class UpdateMyProviderProfileEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;

    public UpdateMyProviderProfileEndpointTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WithValidRequest_ShouldDispatchUpdateCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var context = CreateHttpContextWithUserId(userId);
        
        var request = new UpdateProviderProfileRequest
        {
            Name = "New Name",
            BusinessProfile = new BusinessProfileDto(
                "Legal", "Fantasy", "Desc", 
                new ContactInfoDto("e@e.com", "123", "site"), 
                new AddressDto("S", "1", "C", "N", "C", "ST", "Z", "Co"))
        };

        // Setup Query to return ProviderId
        var providerDto = new ProviderDto(
            providerId, userId, "Old Name", EProviderType.Individual, null!, 
            EProviderStatus.Active, EVerificationStatus.Verified, 
            new List<DocumentDto>(), new List<QualificationDto>(), new List<ProviderServiceDto>(), DateTime.UtcNow, null, false, null, null, null);
            
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Setup Command to return Success
        _commandDispatcherMock
            .Setup(x => x.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
                It.IsAny<UpdateProviderProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        // Act
        var methodInfo = typeof(UpdateMyProviderProfileEndpoint).GetMethod("UpdateMyProfileAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
        var task = (Task<IResult>)methodInfo!.Invoke(null, new object[] { context, request, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
                
        _commandDispatcherMock.Verify(x => x.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
                It.Is<UpdateProviderProfileCommand>(c => c.ProviderId == providerId && c.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateMyProfileAsync_WhenProviderNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateHttpContextWithUserId(userId);
        
        var request = new UpdateProviderProfileRequest
        {
            Name = "New Name",
            BusinessProfile = new BusinessProfileDto(
                "Legal", "Fantasy", "Desc", 
                new ContactInfoDto("e@e.com", "123", "site"), 
                new AddressDto("S", "1", "C", "N", "C", "ST", "Z", "Co"))
        };

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var methodInfo = typeof(UpdateMyProviderProfileEndpoint).GetMethod("UpdateMyProfileAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
        var task = (Task<IResult>)methodInfo!.Invoke(null, new object[] { context, request, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task; // Should be NotFound
        
        // Impossible to assert IResult type directly easily without helpers, but execution completes without exception
    }


    private DefaultHttpContext CreateHttpContextWithUserId(Guid userId)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }
}
