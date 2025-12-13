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
            throw new InvalidOperationException(
                $"Failed to enqueue background job of type '{typeof(T).Name}' in Hangfire queue",
                ex);
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
            throw new InvalidOperationException(
                "Failed to enqueue background job expression in Hangfire queue",
                ex);
        }
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        try
        {
            // Usar timezone cross-platform (IANA) com fallback para UTC
            TimeZoneInfo timeZone;
            try
            {
                // Tentar IANA ID primeiro (funciona em Linux/Windows no .NET 9+)
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); // UTC-3 (Brasília)
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    // Fallback para Windows ID
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                }
                catch (TimeZoneNotFoundException ex)
                {
                    // Fallback final para UTC
                    _logger.LogWarning(ex, "Timezone America/Sao_Paulo e fallback não encontrados, usando UTC");
                    timeZone = TimeZoneInfo.Utc;
                }
            }

            _recurringJobManager.AddOrUpdate(
                jobId,
                methodCall,
                cronExpression,
                new RecurringJobOptions
                {
                    TimeZone = timeZone
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
            throw new InvalidOperationException(
                $"Failed to schedule recurring Hangfire job '{jobId}' with cron expression '{cronExpression}'",
                ex);
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
