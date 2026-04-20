using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

[Trait("Category", "Unit")]
public class BaseDbContextTests
{
    private readonly Mock<IDomainEventProcessor> _processorMock = new();

    [Fact]
    public async Task SaveChangesAsync_ShouldProcessDomainEvents_WhenTheyExist()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var domainEvent = new Mock<IDomainEvent>().Object;
        var sut = new TestDbContext(options, _processorMock.Object, new List<IDomainEvent> { domainEvent });

        await sut.SaveChangesAsync();

        _processorMock.Verify(p => p.ProcessDomainEventsAsync(
            It.Is<IEnumerable<IDomainEvent>>(l => l.Contains(domainEvent)), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
        sut.ClearCalled.Should().BeTrue();
    }

    private class TestDbContext : BaseDbContext
    {
        private readonly List<IDomainEvent> _events;
        public bool ClearCalled { get; private set; }

        public TestDbContext(DbContextOptions options, IDomainEventProcessor processor, List<IDomainEvent> events) 
            : base(options, processor)
        {
            _events = events;
        }

        protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_events);

        protected override void ClearDomainEvents() => ClearCalled = true;
    }
}
