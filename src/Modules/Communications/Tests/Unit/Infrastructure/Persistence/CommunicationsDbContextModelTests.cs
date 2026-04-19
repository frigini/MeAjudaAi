using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Persistence;

public class CommunicationsDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CommunicationsDbContext>()
            .UseInMemoryDatabase(databaseName: "CommunicationsTestDb_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new CommunicationsDbContext(options, domainEventProcessorMock.Object);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("communications");
        
        // Check if entities are registered
        model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.EmailTemplate)).Should().NotBeNull();
        model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.CommunicationLog)).Should().NotBeNull();
        model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage)).Should().NotBeNull();
    }
}
