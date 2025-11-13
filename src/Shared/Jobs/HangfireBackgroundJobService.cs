using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Jobs;

/// <summary>
/// Implementação do serviço de background jobs usando Hangfire.
/// 
/// Hangfire persiste jobs em PostgreSQL e executa em background workers.
/// Suporta retry automático, dashboard de monitoramento e jobs recorrentes.
/// </summary>
public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireBackgroundJobService> _logger;

    public HangfireBackgroundJobService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireBackgroundJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null) where T : notnull
    {
        try
        {
            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                _backgroundJobClient.Schedule(methodCall, delay.Value);
                _logger.LogInformation(
                    "Job agendado para {JobType}.{Method} com delay de {Delay}",
                    typeof(T).Name,
                    GetMethodName(methodCall),
                    delay.Value);
            }
            else
            {
                _backgroundJobClient.Enqueue(methodCall);
                _logger.LogInformation(
                    "Job enfileirado para {JobType}.{Method}",
                    typeof(T).Name,
                    GetMethodName(methodCall));
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enfileirar job para {JobType}", typeof(T).Name);
            throw;
        }
    }

    public Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null)
    {
        try
        {
            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                _backgroundJobClient.Schedule(methodCall, delay.Value);
                _logger.LogInformation(
                    "Job agendado para {Method} com delay de {Delay}",
                    GetMethodName(methodCall),
                    delay.Value);
            }
            else
            {
                _backgroundJobClient.Enqueue(methodCall);
                _logger.LogInformation(
                    "Job enfileirado para {Method}",
                    GetMethodName(methodCall));
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enfileirar job");
            throw;
        }
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        try
        {
            _recurringJobManager.AddOrUpdate(
                jobId,
                methodCall,
                cronExpression,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time") // UTC-3 (Brasília)
                });

            _logger.LogInformation(
                "Job recorrente configurado: {JobId} com cron {CronExpression}",
                jobId,
                cronExpression);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar job recorrente {JobId}", jobId);
            throw;
        }
    }

    private static string GetMethodName(Expression expression)
    {
        if (expression is LambdaExpression lambda && lambda.Body is MethodCallExpression methodCall)
        {
            return methodCall.Method.Name;
        }

        return "Unknown";
    }
}
