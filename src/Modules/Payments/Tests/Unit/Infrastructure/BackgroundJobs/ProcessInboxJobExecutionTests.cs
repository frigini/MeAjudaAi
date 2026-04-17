using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using Microsoft.Data.Sqlite;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.BackgroundJobs;

public class ProcessInboxJobExecutionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PaymentsDbContext _dbContext;
    private readonly Mock<ILogger<ProcessInboxJob>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    private readonly Mock<IPaymentTransactionRepository> _paymentTransactionRepositoryMock;

    public ProcessInboxJobExecutionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PaymentsDbContext(options);
        _dbContext.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<ProcessInboxJob>>();
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _paymentTransactionRepositoryMock = new Mock<IPaymentTransactionRepository>();

        var services = new ServiceCollection();
        services.AddSingleton(_dbContext);
        services.AddSingleton(_subscriptionRepositoryMock.Object);
        services.AddSingleton(_paymentTransactionRepositoryMock.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessMessages_WhenTheyExist()
    {
        // Arrange
        var content = "{\"id\": \"evt_1\", \"type\": \"unknown\", \"data\": {\"object\": { \"object\": \"account\" }}}";
        var message = new InboxMessage("unknown", content, "evt_1");
        _dbContext.InboxMessages.Add(message);
        await _dbContext.SaveChangesAsync();

        var job = new ProcessInboxJobWrapper(_serviceProvider, _loggerMock.Object);
        var cts = new CancellationTokenSource();

        // Act
        // Run once and cancel
        await job.DoExecuteStepAsync(cts.Token);

        // Assert
        var processedMessage = await _dbContext.InboxMessages.FindAsync(message.Id);
        processedMessage!.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordError_WhenProcessingFails()
    {
        // Arrange
        var content = "invalid json"; // This will cause ParseEvent to fail
        var message = new InboxMessage("type", "{}", "evt_fail");
        // Manually set invalid content bypassing constructor validation if needed, 
        // but here we can just use a type that we know will fail in MapToStripeEventData or ProcessStripeEventAsync
        
        // Let's use a valid JSON but a type that throws
        var validContent = "{\"id\": \"evt_2\", \"type\": \"checkout.session.completed\", \"data\": {\"object\": { \"id\": \"cs_1\" }}}";
        var failMessage = new InboxMessage("checkout.session.completed", validContent, "evt_2");
        _dbContext.InboxMessages.Add(failMessage);
        await _dbContext.SaveChangesAsync();

        // This will throw InvalidOperationException: Essential data missing
        var job = new ProcessInboxJobWrapper(_serviceProvider, _loggerMock.Object);
        
        // Act
        await job.DoExecuteStepAsync(CancellationToken.None);

        // Assert
        var updatedMessage = await _dbContext.InboxMessages.FindAsync(failMessage.Id);
        updatedMessage!.ProcessedAt.Should().BeNull();
        updatedMessage.Error.Should().NotBeNull();
        updatedMessage.RetryCount.Should().Be(1);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
        _serviceProvider.Dispose();
    }

    // Helper class to expose protected ExecuteAsync or parts of it
    private class ProcessInboxJobWrapper(IServiceProvider sp, ILogger<ProcessInboxJob> logger) 
        : ProcessInboxJob(sp, logger)
    {
        public async Task DoExecuteStepAsync(CancellationToken ct)
        {
            // We implement a single pass of the loop logic from ExecuteAsync
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
            var paymentTransactionRepository = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();

            // Note: Sqlite doesn't support FOR UPDATE SKIP LOCKED, so we use a simpler query for testing
            var messages = await dbContext.InboxMessages
                .Where(m => m.ProcessedAt == null && m.RetryCount < m.MaxRetries)
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

            if (messages.Count == 0) return;

            foreach (var message in messages)
            {
                try
                {
                    var stripeEvent = Stripe.EventUtility.ParseEvent(message.Content, throwOnApiVersionMismatch: false);
                    var data = MapToStripeEventData(stripeEvent);
                    await ProcessStripeEventAsync(data, subscriptionRepository, paymentTransactionRepository, ct);
                    message.MarkAsProcessed();
                }
                catch (Exception ex)
                {
                    message.RecordError(ex.Message);
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
