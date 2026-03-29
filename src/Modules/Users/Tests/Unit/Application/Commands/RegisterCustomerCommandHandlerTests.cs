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
}
