using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Commands;

public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public bool WasCalled { get; private set; }
    public TestCommand? ReceivedCommand { get; private set; }

    public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        ReceivedCommand = command;
        return Task.CompletedTask;
    }
}

public class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, string>
{
    public bool WasCalled { get; private set; }
    public TestCommandWithResult? ReceivedCommand { get; private set; }

    public Task<string> HandleAsync(TestCommandWithResult command, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        ReceivedCommand = command;
        return Task.FromResult($"Processed: {command.Data}");
    }
}
