using System.Linq.Expressions;
using MeAjudaAi.Shared.Jobs;

namespace MeAjudaAi.Shared.Tests.Mocks.Jobs;

/// <summary>
/// Mock do serviço de background jobs para uso em testes.
/// Armazena expressões de jobs enfileirados para verificação nos testes.
/// </summary>
public class MockBackgroundJobService : IBackgroundJobService
{
    private readonly object _lock = new();
    private readonly List<EnqueuedJobEntry> _enqueuedJobs = [];
    private readonly List<RecurringJobEntry> _recurringJobs = [];

    /// <summary>
    /// Jobs enfileirados para execução imediata ou com delay.
    /// </summary>
    public IReadOnlyList<EnqueuedJobEntry> EnqueuedJobs
    {
        get
        {
            lock (_lock)
            {
                return _enqueuedJobs.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Jobs recorrentes agendados.
    /// </summary>
    public IReadOnlyList<RecurringJobEntry> RecurringJobs
    {
        get
        {
            lock (_lock)
            {
                return _recurringJobs.AsReadOnly();
            }
        }
    }

    public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null) where T : notnull
    {
        lock (_lock)
        {
            _enqueuedJobs.Add(new EnqueuedJobEntry(
                MethodCall: methodCall,
                Delay: delay,
                JobId: Guid.NewGuid().ToString()));
        }
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null)
    {
        lock (_lock)
        {
            _enqueuedJobs.Add(new EnqueuedJobEntry(
                MethodCall: methodCall,
                Delay: delay,
                JobId: Guid.NewGuid().ToString()));
        }
        return Task.CompletedTask;
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        lock (_lock)
        {
            _recurringJobs.Add(new RecurringJobEntry(
                JobId: jobId,
                MethodCall: methodCall,
                CronExpression: cronExpression));
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Limpa todos os jobs registrados.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _enqueuedJobs.Clear();
            _recurringJobs.Clear();
        }
    }
}

/// <summary>
/// Representa um job enfileirado.
/// </summary>
public record EnqueuedJobEntry(
    LambdaExpression MethodCall,
    TimeSpan? Delay,
    string JobId);

/// <summary>
/// Representa um job recorrente agendado.
/// </summary>
public record RecurringJobEntry(
    string JobId,
    LambdaExpression MethodCall,
    string CronExpression);
