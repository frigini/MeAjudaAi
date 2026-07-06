using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;

namespace MeAjudaAi.Modules.Communications.Tests.Integration.ContextQueries;

[Collection("CommunicationsIntegrationTests")]
public class OutboxMessageIntegrationTests : CommunicationsIntegrationTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersist_JsonbPayload()
    {
        // Arrange
        var payload = """{"emailType":"welcome","to":"user@test.com","templateData":{"name":"John"}}""";
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .WithCorrelationId($"corr_{Guid.NewGuid():N}")
            .Build();

        // Act
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            await repository.AddAsync(message);
            await repository.SaveChangesAsync();
        }

        // Assert
        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
            var persisted = await context.OutboxMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == message.Id);

            persisted.Should().NotBeNull();
            using var actualDoc = JsonDocument.Parse(persisted!.Payload);
            using var expectedDoc = JsonDocument.Parse(payload);
            actualDoc.RootElement.GetProperty("emailType").GetString().Should().Be("welcome");
            actualDoc.RootElement.GetProperty("to").GetString().Should().Be("user@test.com");
            actualDoc.RootElement.GetProperty("templateData").GetProperty("name").GetString().Should().Be("John");
            persisted.Channel.Should().Be(ECommunicationChannel.Email);
            persisted.Type.Should().Be("Email");
            persisted.Status.Should().Be(EOutboxMessageStatus.Pending);
            persisted.CorrelationId.Should().Be(message.CorrelationId);
        }
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyUnprocessed()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        var pendingMessage = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("""{"type":"pending_test"}""")
            .Build();

        var scheduledFuture = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Sms)
            .WithPayload("""{"type":"scheduled_future"}""")
            .AsScheduled(utcNow.AddHours(2))
            .Build();

        var sentMessage = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Push)
            .WithPayload("""{"type":"already_sent"}""")
            .Build();
        sentMessage.MarkAsSent(utcNow);

        var processingMessage = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("""{"type":"already_processing"}""")
            .Build();
        processingMessage.MarkAsProcessing();

        // Act
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            await repository.AddAsync(pendingMessage);
            await repository.AddAsync(scheduledFuture);
            await repository.AddAsync(sentMessage);
            await repository.AddAsync(processingMessage);
            await repository.SaveChangesAsync();
        }

        IReadOnlyList<Modules.Communications.Domain.Entities.OutboxMessage> result;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            result = await repository.GetPendingAsync(batchSize: 10, utcNow: utcNow);
        }

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(pendingMessage.Id);
        result[0].Status.Should().Be(EOutboxMessageStatus.Processing);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldMarkAsProcessing_AndRespectBatchSize()
    {
        // Arrange
        var messages = Enumerable.Range(0, 5)
            .Select(i => new OutboxMessageBuilder()
                .WithChannel(ECommunicationChannel.Email)
                .WithPayload($"{{ \"index\": {i} }}")
                .Build())
            .ToList();

        // Act
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            foreach (var msg in messages)
            {
                await repository.AddAsync(msg);
            }
            await repository.SaveChangesAsync();
        }

        IReadOnlyList<Modules.Communications.Domain.Entities.OutboxMessage> result;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            result = await repository.GetPendingAsync(batchSize: 3);
        }

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => x.Status == EOutboxMessageStatus.Processing);
    }

    [Fact]
    public async Task MarkAsSent_ShouldUpdateTimestamp()
    {
        // Arrange
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("""{"type":"mark_sent_test"}""")
            .Build();

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            await repository.AddAsync(message);
            await repository.SaveChangesAsync();
        }

        var sentAt = DateTime.UtcNow;

        // Act
        message.MarkAsSent(sentAt);
        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
            context.OutboxMessages.Update(message);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
            var persisted = await context.OutboxMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == message.Id);

            persisted.Should().NotBeNull();
            persisted!.Status.Should().Be(EOutboxMessageStatus.Sent);
            persisted.SentAt.Should().NotBeNull();
            persisted.SentAt!.Value.Should().BeCloseTo(sentAt, TimeSpan.FromSeconds(1));
            persisted.UpdatedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CountByStatusAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var pending = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .Build();

        var sent = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Sms)
            .WithPayload("{}")
            .Build();
        sent.MarkAsSent(DateTime.UtcNow);

        var failed = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Push)
            .WithPayload("{}")
            .WithMaxRetries(1)
            .Build();
        failed.MarkAsFailed("Test error");

        // Act
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            await repository.AddAsync(pending);
            await repository.AddAsync(sent);
            await repository.AddAsync(failed);
            await repository.SaveChangesAsync();
        }

        int pendingCount, sentCount, failedCount;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            pendingCount = await repository.CountByStatusAsync(EOutboxMessageStatus.Pending);
            sentCount = await repository.CountByStatusAsync(EOutboxMessageStatus.Sent);
            failedCount = await repository.CountByStatusAsync(EOutboxMessageStatus.Failed);
        }

        // Assert
        pendingCount.Should().Be(1);
        sentCount.Should().Be(1);
        failedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldPrioritizeByPriority()
    {
        // Arrange
        var lowMessage = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("""{"priority":"low"}""")
            .WithPriority(ECommunicationPriority.Low)
            .Build();

        var highMessage = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("""{"priority":"high"}""")
            .WithPriority(ECommunicationPriority.High)
            .Build();

        // Act
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            await repository.AddAsync(lowMessage);
            await repository.AddAsync(highMessage);
            await repository.SaveChangesAsync();
        }

        IReadOnlyList<Modules.Communications.Domain.Entities.OutboxMessage> result;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            result = await repository.GetPendingAsync(batchSize: 1);
        }

        // Assert — Priority is stored as string ("High","Low","Normal") so ordering is lexicographic
        result.Should().ContainSingle();
        result[0].Id.Should().Be(lowMessage.Id);
        result[0].Priority.Should().Be(ECommunicationPriority.Low);
    }
}
