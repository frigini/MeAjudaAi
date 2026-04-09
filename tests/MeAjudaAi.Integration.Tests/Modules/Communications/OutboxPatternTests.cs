using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Shared;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Communications.Application.Services;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Communications;

public class OutboxPatternTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Communications;

    [Fact]
    public async Task SendEmail_ShouldEnqueueInOutbox_AndProcessorShouldSendIt()
    {
        // Arrange
        var api = Services.GetRequiredService<ICommunicationsModuleApi>();
        var processor = Services.GetRequiredService<IOutboxProcessorService>();
        var dbContext = Services.GetRequiredService<CommunicationsDbContext>();

        var emailDto = new EmailMessageDto(
            To: "test@example.com",
            Subject: "Test Outbox",
            Body: "Hello from integration test"
        );

        // Act - 1. Enviar via API (deve enfileirar)
        var result = await api.SendEmailAsync(emailDto);
        result.IsSuccess.Should().BeTrue();
        var outboxId = result.Value;

        // Verificar se está no banco como Pending
        var messageBefore = await dbContext.OutboxMessages.FindAsync(outboxId);
        messageBefore.Should().NotBeNull();
        messageBefore!.Status.Should().Be(EOutboxMessageStatus.Pending);

        // Act - 2. Rodar processador
        var processedCount = await processor.ProcessPendingMessagesAsync();
        processedCount.Should().BeGreaterThanOrEqualTo(1);

        // Assert - 3. Verificar se foi marcado como Sent
        // Limpar cache do EF para pegar dados frescos do banco
        dbContext.Entry(messageBefore).State = EntityState.Detached;
        var messageAfter = await dbContext.OutboxMessages.FindAsync(outboxId);
        
        messageAfter.Should().NotBeNull();
        messageAfter!.Status.Should().Be(EOutboxMessageStatus.Sent);
        messageAfter.SentAt.Should().NotBeNull();

        // Verificar log de comunicação
        var log = await dbContext.CommunicationLogs
            .FirstOrDefaultAsync(x => x.OutboxMessageId == outboxId);
        
        log.Should().NotBeNull();
        log!.IsSuccess.Should().BeTrue();
        log.Recipient.Should().Be(emailDto.To);
    }

    [Fact]
    public async Task ScheduledMessage_ShouldNotBeProcessedBeforeTime()
    {
        // Arrange
        var api = Services.GetRequiredService<ICommunicationsModuleApi>();
        var processor = Services.GetRequiredService<IOutboxProcessorService>();
        var dbContext = Services.GetRequiredService<CommunicationsDbContext>();

        var emailDto = new EmailMessageDto(
            To: "future@example.com",
            Subject: "Future Email",
            Body: "I am from the future"
        );

        // Enfileirar com agendamento para daqui a 1 hora
        var scheduledAt = DateTime.UtcNow.AddHours(1);
        
        // Como a API atual não aceita ScheduledAt diretamente (DTO simplificado), 
        // vamos criar a entidade manualmente para testar o comportamento do repositório/processor
        var message = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage.Create(
            ECommunicationChannel.Email,
            System.Text.Json.JsonSerializer.Serialize(emailDto),
            ECommunicationPriority.Normal,
            scheduledAt: scheduledAt);

        await dbContext.OutboxMessages.AddAsync(message);
        await dbContext.SaveChangesAsync();

        // Act
        var processedCount = await processor.ProcessPendingMessagesAsync();

        // Assert
        processedCount.Should().Be(0); // Não deve processar a mensagem agendada

        var dbMessage = await dbContext.OutboxMessages.FindAsync(message.Id);
        dbMessage!.Status.Should().Be(EOutboxMessageStatus.Pending);
    }
}
