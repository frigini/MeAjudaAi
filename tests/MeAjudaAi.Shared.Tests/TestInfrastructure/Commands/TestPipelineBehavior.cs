using MeAjudaAi.Shared.Mediator;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Commands;

/// <summary>
/// Test double for IPipelineBehavior that tracks call order.
/// IMPORTANT: Call ResetCounter() in test setup to ensure deterministic CallOrder values.
/// The static counter is shared across all generic instantiations of this class.
/// </summary>
public class TestPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public bool WasCalled { get; private set; }
    public int CallOrder { get; private set; }
    private static int _globalCallCounter;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        WasCalled = true;
        CallOrder = ++_globalCallCounter;
        return await next();
    }

    public static void ResetCounter() => _globalCallCounter = 0;
}
