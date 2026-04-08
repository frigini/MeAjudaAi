using FluentAssertions;
using MeAjudaAi.ApiService.Endpoints;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Endpoints;

public class ProviderRegistrationEndpointsTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger> _loggerMock;

    public ProviderRegistrationEndpointsTests()
    {
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
    }

    [Fact]
    public async Task RegisterProviderAsync_ShouldReturnBadRequest_WhenTermsNotAccepted()
    {
        // Arrange
        var request = new RegisterProviderRequest
        {
            Name = "Test Provider",
            Email = "test@example.com",
            PhoneNumber = "11999999999",
            Type = EProviderType.Individual,
            AcceptedTerms = false,
            AcceptedPrivacyPolicy = true
        };

        // Act
        var result = await CallRegisterProviderAsync(request);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.Should().Be("Você deve aceitar os Termos de Uso e a Política de Privacidade para se cadastrar.");
    }

    [Fact]
    public async Task RegisterProviderAsync_ShouldReturnBadRequest_WhenUserCreationFails()
    {
        // Arrange
        var request = CreateValidRequest();
        _commandDispatcherMock.Setup(x => x.SendAsync<CreateUserCommand, Result<UserDto>>(
            It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(new Error("Failed to create user", 400)));

        // Act
        var result = await CallRegisterProviderAsync(request);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.Should().Be("Ocorreu um erro ao registrar o usuário.");
        _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Fact]
    public async Task RegisterProviderAsync_ShouldRollbackAndReturnBadRequest_WhenProviderCreationFails()
    {
        // Arrange
        var request = CreateValidRequest();
        var userDto = CreateUserDto(Guid.NewGuid());

        _commandDispatcherMock.Setup(x => x.SendAsync<CreateUserCommand, Result<UserDto>>(
            It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        _commandDispatcherMock.Setup(x => x.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
            It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(new Error("Failed to create provider", 400)));

        _commandDispatcherMock.Setup(x => x.SendAsync<DeleteUserCommand, Result>(
            It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await CallRegisterProviderAsync(request);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.Should().Be("Ocorreu um erro ao registrar o provedor.");
        
        // Verify compensation (DeleteUserCommand) was called
        _commandDispatcherMock.Verify(x => x.SendAsync<DeleteUserCommand, Result>(
            It.Is<DeleteUserCommand>(c => c.UserId == userDto.Id), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RegisterProviderAsync_ShouldReturnCreated_WhenFlowIsSuccessful()
    {
        // Arrange
        var request = CreateValidRequest();
        var userDto = CreateUserDto(Guid.NewGuid());
        var providerDto = CreateProviderDto(Guid.NewGuid(), userDto.Id, request.Name);

        _commandDispatcherMock.Setup(x => x.SendAsync<CreateUserCommand, Result<UserDto>>(
            It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        _commandDispatcherMock.Setup(x => x.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
            It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        // Act
        var result = await CallRegisterProviderAsync(request);

        // Assert
        result.Should().BeOfType<Created<MeAjudaAi.Contracts.Models.Response<ProviderDto>>>();
        var created = (Created<MeAjudaAi.Contracts.Models.Response<ProviderDto>>)result;
        created.Location.Should().Be($"/api/v1/providers/{providerDto.Id}");
        created.Value!.Data.Id.Should().Be(providerDto.Id);
    }

    private async Task<IResult> CallRegisterProviderAsync(RegisterProviderRequest request)
    {
        var method = typeof(ProviderRegistrationEndpoints).GetMethod("RegisterProviderAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        return await (Task<IResult>)method!.Invoke(null, new object[] 
        { 
            request, 
            _commandDispatcherMock.Object, 
            _loggerFactoryMock.Object, 
            CancellationToken.None 
        })!;
    }

    private RegisterProviderRequest CreateValidRequest() => new()
    {
        Name = "Test Provider",
        Email = "test@example.com",
        PhoneNumber = "11999999999",
        Type = EProviderType.Individual,
        AcceptedTerms = true,
        AcceptedPrivacyPolicy = true
    };

    private UserDto CreateUserDto(Guid id) => new(
        id, "username", "test@test.com", "First", "Last", "First Last", id.ToString(), DateTime.UtcNow, null);

    private ProviderDto CreateProviderDto(Guid id, Guid userId, string name) => new(
        id, userId, name, "slug", EProviderType.Individual, null!, EProviderStatus.Active, EVerificationStatus.Verified, EProviderTier.Standard, [], [], [], DateTime.UtcNow, null, false, null, true);
}

public static class LoggerExtensions
{
    public static void VerifyLog(this Mock<ILogger> loggerMock, LogLevel level, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times);
    }
}
