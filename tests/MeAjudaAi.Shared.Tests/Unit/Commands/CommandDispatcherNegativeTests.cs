using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Commands;

public class CommandDispatcherNegativeTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<CommandDispatcher>> _loggerMock = new();
    private readonly CommandDispatcher _sut;

    public CommandDispatcherNegativeTests()
    {
        _sut = new CommandDispatcher(_serviceProviderMock.Object, _loggerMock.Object);
    }

    public interface ITestCommand : ICommand { }

    [Fact]
    public async Task SendAsync_WhenHandlerNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var commandMock = new Mock<ITestCommand>();
        // GetRequiredService calls GetService and throws if null
        _serviceProviderMock.Setup(s => s.GetService(typeof(ICommandHandler<ITestCommand>)))
            .Returns(default(ICommandHandler<ITestCommand>));

        // Act
        var act = () => _sut.SendAsync(commandMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
