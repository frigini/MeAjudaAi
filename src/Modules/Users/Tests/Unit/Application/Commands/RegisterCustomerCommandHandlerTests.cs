using FluentAssertions;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<IUserDomainService> _userDomainServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<RegisterCustomerCommandHandler>> _loggerMock;
    private readonly RegisterCustomerCommandHandler _handler;

    public RegisterCustomerCommandHandlerTests()
    {
        _userDomainServiceMock = new Mock<IUserDomainService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<RegisterCustomerCommandHandler>>();
        _handler = new RegisterCustomerCommandHandler(
            _userDomainServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WhenFlowSucceeds()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            Name: "Test Customer",
            Email: "customer@example.com",
            Password: "Password123!",
            PhoneNumber: "11988887777",
            TermsAccepted: true,
            AcceptedPrivacyPolicy: true
        );

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var user = User.Create(new Username("test_user_slug"), new Email(command.Email), "Test", "Customer", Guid.NewGuid().ToString(), command.PhoneNumber).Value!;
        _userDomainServiceMock.Setup(x => x.CreateUserAsync(
            It.IsAny<Username>(),
            It.IsAny<Email>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userDomainServiceMock.Verify(x => x.CreateUserAsync(
            It.IsAny<Username>(),
            It.IsAny<Email>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenTermsNotAccepted()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "email@test.com", "Password123!", "11999999999", false, true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(RegisterCustomerCommandHandler.TermsNotAcceptedError);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenPrivacyPolicyNotAccepted()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "email@test.com", "Password123!", "11999999999", true, false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(RegisterCustomerCommandHandler.PrivacyPolicyNotAcceptedError);
        _userDomainServiceMock.Verify(x => x.CreateUserAsync(It.IsAny<Username>(), It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "invalid-email", "Password123!", "11999999999", true, true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenEmailAlreadyInUse()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "existing@test.com", "Password123!", "11999999999", true, true);
        var existingUser = User.Create(new Username("existing"), new Email(command.Email), "Existing", "User", Guid.NewGuid().ToString(), null).Value!;

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(409);
        result.Error.Message.Should().Contain("já está em uso");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenNameHasOnlyOnePart()
    {
        // Arrange
        var command = new RegisterCustomerCommand("SoloName", "email@test.com", "Password123!", "11999999999", true, true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("sobrenome é obrigatório");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenFirstNameTooShort()
    {
        // Arrange
        var command = new RegisterCustomerCommand("J Doe", "email@test.com", "Password123!", "11999999999", true, true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("primeiro nome deve ter pelo menos");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenLastNameTooShort()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John D", "email@test.com", "Password123!", "11999999999", true, true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("sobrenome deve ter pelo menos");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCreateUserAsyncFails()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "email@test.com", "Password123!", "11999999999", true, true);
        var error = Error.Internal("Service failure");

        _userDomainServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<Username>(), It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Failure(error));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_AndTriggerCompensation_WhenAddAsyncThrowsExceptionAndUserNotFound()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "email@test.com", "Password123!", "11999999999", true, true);
        var user = User.Create(new Username("test_user"), new Email(command.Email), "John", "Doe", Guid.NewGuid().ToString(), null).Value!;

        _userDomainServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<Username>(), It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        _userRepositoryMock.Setup(x => x.GetByIdNoTrackingAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userDomainServiceMock.Setup(x => x.DeactivateUserInKeycloakAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _userDomainServiceMock.Verify(x => x.DeactivateUserInKeycloakAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_AndNotTriggerCompensation_WhenAddAsyncThrowsExceptionAndUserFound()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "email@test.com", "Password123!", "11999999999", true, true);
        var user = User.Create(new Username("test_user"), new Email(command.Email), "John", "Doe", Guid.NewGuid().ToString(), null).Value!;

        _userDomainServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<Username>(), It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        _userRepositoryMock.Setup(x => x.GetByIdNoTrackingAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _userDomainServiceMock.Verify(x => x.DeactivateUserInKeycloakAsync(user.Id, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_AndLogCritical_WhenCompensationFails()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "email@test.com", "Password123!", "11999999999", true, true);
        var user = User.Create(new Username("test_user"), new Email(command.Email), "John", "Doe", Guid.NewGuid().ToString(), null).Value!;

        _userDomainServiceMock.Setup(x => x.CreateUserAsync(
            It.IsAny<Username>(), It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        _userRepositoryMock.Setup(x => x.GetByIdNoTrackingAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userDomainServiceMock.Setup(x => x.DeactivateUserInKeycloakAsync(user.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Keycloak Failure"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _userDomainServiceMock.Verify(x => x.DeactivateUserInKeycloakAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to compensate Keycloak user")),
                It.Is<Exception>(ex => ex.Message.Contains("Keycloak Failure")),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
