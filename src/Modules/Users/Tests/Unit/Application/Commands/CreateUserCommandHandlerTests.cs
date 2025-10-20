using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserDomainService> _userDomainServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userDomainServiceMock = new Mock<IUserDomainService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();
        _handler = new CreateUserCommandHandler(_userDomainServiceMock.Object, _userRepositoryMock.Object, _loggerMock.Object);
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
            Roles: ["Customer"]
        );

        var user = new UserBuilder()
            .WithUsername(command.Username)
            .WithEmail(command.Email)
            .WithFirstName(command.FirstName)
            .WithLastName(command.LastName)
            .Build();

        // Configura as validações para passar (sem usuários existentes)
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userDomainServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<Username>(),
                It.IsAny<Email>(),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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

        // Verifica se todos os métodos foram chamados
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()), Times.Once);
        _userDomainServiceMock.Verify(
            x => x.CreateUserAsync(
                It.Is<Username>(u => u.Value == command.Username),
                It.Is<Email>(e => e.Value == command.Email),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
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
            Roles: ["Customer"]
        );

        var error = Error.BadRequest("Failed to create user");

        _userDomainServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<Username>(),
                It.IsAny<Email>(),
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
                It.IsAny<Username>(),
                It.IsAny<Email>(),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "testuser",
            Email: "existing@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "password123",
            Roles: ["Customer"]
        );

        var existingUser = new UserBuilder()
            .WithUsername("existinguser")
            .WithEmail(command.Email)
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("email already exists");

        // Verifica que o check de username e o serviço de domínio não foram chamados
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()), Times.Never);
        _userDomainServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Username>(),
                It.IsAny<Email>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "existinguser",
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "password123",
            Roles: ["Customer"]
        );

        var existingUser = new UserBuilder()
            .WithUsername(command.Username)
            .WithEmail("existing@example.com")
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Username already taken");

        // Verifica que o serviço de domínio não foi chamado
        _userDomainServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Username>(),
                It.IsAny<Email>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "password123",
            Roles: ["Customer"]
        );

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to create user");

        // Verifica que métodos subsequentes não foram chamados
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()), Times.Never);
        _userDomainServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<Username>(),
                It.IsAny<Email>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
