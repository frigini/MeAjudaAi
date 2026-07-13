using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Commands;

public record TestCommand(Guid CorrelationId, string Data) : ICommand;

public record TestCommandWithResult(Guid CorrelationId, string Data) : ICommand<string>;
