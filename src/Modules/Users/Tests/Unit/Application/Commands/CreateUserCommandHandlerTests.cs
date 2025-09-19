using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Common;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserDomainService> _userDomainServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateUserCommandHandlerTests()
    {
        _userDomainServiceMock = new Mock<IUserDomainService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();
        _handler = new CreateUserCommandHandler(_userDomainServiceMock.Object, _userRepositoryMock.Object, _loggerMock.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "password123",
            Roles: new[] { "Customer" }
        );

        var user = new UserBuilder()
            .WithUsername(command.Username)
            .WithEmail(command.Email)
            .WithFirstName(command.FirstName)
            .WithLastName(command.LastName)
            .Build();

        _userDomainServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MeAjudaAi.Modules.Users.Domain.ValueObjects.Username>(),
                It.IsAny<MeAjudaAi.Modules.Users.Domain.ValueObjects.Email>(),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be(command.Username);
        result.Value.Email.Should().Be(command.Email);
        result.Value.FirstName.Should().Be(command.FirstName);
        result.Value.LastName.Should().Be(command.LastName);

        _userDomainServiceMock.Verify(
            x => x.CreateUserAsync(
                It.Is<MeAjudaAi.Modules.Users.Domain.ValueObjects.Username>(u => u.Value == command.Username),
                It.Is<MeAjudaAi.Modules.Users.Domain.ValueObjects.Email>(e => e.Value == command.Email),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDomainServiceFails_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "password123",
            Roles: new[] { "Customer" }
        );

        var error = Error.BadRequest("Failed to create user");

        _userDomainServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MeAjudaAi.Modules.Users.Domain.ValueObjects.Username>(),
                It.IsAny<MeAjudaAi.Modules.Users.Domain.ValueObjects.Email>(),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Failure(error));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        _userDomainServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<MeAjudaAi.Modules.Users.Domain.ValueObjects.Username>(),
                It.IsAny<MeAjudaAi.Modules.Users.Domain.ValueObjects.Email>(),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}