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
        public TestDbContext(DbContextOptions options) : base(options) { }

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

    private static DbContextOptions<TestDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchAndClearDomainEvents_ForStronglyTypedIdAggregate()
    {
        var processorMock = new Mock<IDomainEventProcessor>();
        var sut = new TestDbContext(CreateOptions(), processorMock.Object);

        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        aggregate.TriggerEvent();
        sut.Aggregates.Add(aggregate);

        await sut.SaveChangesAsync();

        processorMock.Verify(x => x.ProcessDomainEventsAsync(
            It.Is<IEnumerable<IDomainEvent>>(events => events.Any(e => e is TestDomainEvent)),
            It.IsAny<CancellationToken>()), Times.Once);

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutDomainEventProcessor_ShouldSaveWithoutDispatching()
    {
        // Context created without processor (design-time / no-processor constructor)
        var sut = new TestDbContext(CreateOptions());

        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        aggregate.TriggerEvent();
        sut.Aggregates.Add(aggregate);

        // Should not throw even though there is no processor
        var result = await sut.SaveChangesAsync();

        result.Should().Be(1);
        // Events remain on aggregate since there was no processor to dispatch them
        aggregate.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoDomainEvents_ShouldNotCallProcessor()
    {
        var processorMock = new Mock<IDomainEventProcessor>();
        var sut = new TestDbContext(CreateOptions(), processorMock.Object);

        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        // No TriggerEvent call - no events
        sut.Aggregates.Add(aggregate);

        await sut.SaveChangesAsync();

        processorMock.Verify(x => x.ProcessDomainEventsAsync(
            It.IsAny<IEnumerable<IDomainEvent>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleDomainEvents_ShouldDispatchAllAndClear()
    {
        var processorMock = new Mock<IDomainEventProcessor>();
        var sut = new TestDbContext(CreateOptions(), processorMock.Object);

        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        aggregate.TriggerEvent();
        aggregate.TriggerEvent();
        aggregate.TriggerEvent();
        sut.Aggregates.Add(aggregate);

        await sut.SaveChangesAsync();

        processorMock.Verify(x => x.ProcessDomainEventsAsync(
            It.Is<IEnumerable<IDomainEvent>>(events => events.Count() == 3),
            It.IsAny<CancellationToken>()), Times.Once);

        aggregate.DomainEvents.Should().BeEmpty();
    }
}
