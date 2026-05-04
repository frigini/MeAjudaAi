using FluentAssertions;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class BaseDbContextTests
{
    private record TestId(Guid Value);
    private record TestDomainEvent(Guid AggregateId) : DomainEvent(AggregateId);

    private class TestAggregate : AggregateRoot<TestId>
    {
        public TestAggregate(TestId id) : base(id) { }
        public void TriggerEvent() => AddDomainEvent(new TestDomainEvent(Guid.NewGuid()));
    }

    private class TestDbContext : BaseDbContext
    {
        public TestDbContext(DbContextOptions options, IDomainEventProcessor processor) 
            : base(options, processor) { }

        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregate>().HasKey(x => x.Id);
            modelBuilder.Entity<TestAggregate>().Property(x => x.Id)
                .HasConversion(id => id.Value, value => new TestId(value));
        }
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchAndClearDomainEvents_ForStronglyTypedIdAggregate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var processorMock = new Mock<IDomainEventProcessor>();
        var sut = new TestDbContext(options, processorMock.Object);

        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        aggregate.TriggerEvent();
        sut.Aggregates.Add(aggregate);

        // Act
        await sut.SaveChangesAsync();

        // Assert
        processorMock.Verify(x => x.ProcessDomainEventsAsync(
            It.Is<IEnumerable<IDomainEvent>>(events => events.Any(e => e is TestDomainEvent)),
            It.IsAny<CancellationToken>()), Times.Once);

        aggregate.DomainEvents.Should().BeEmpty();
    }
}
