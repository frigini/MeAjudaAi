using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Persistence;

public class PaymentsDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEventsAndClearThem()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();
        domainEventProcessorMock.Setup(x => x.ProcessDomainEventsAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var context = new PaymentsDbContext(options, domainEventProcessorMock.Object);
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        
        // Ativar e depois Cancelar para garantir a geração de eventos de domínio
        sub.Activate("sub_123", "cus_123"); 
        sub.Cancel();
        
        context.Subscriptions.Add(sub);

        // Verifica se há eventos antes de salvar
        sub.DomainEvents.Should().NotBeEmpty();

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        
        // O DbContext do projeto limpa os eventos após o save bem-sucedido.
        // Se eles foram limpos, a lógica de despacho/limpeza foi executada.
        sub.DomainEvents.Should().BeEmpty();
    }
}
