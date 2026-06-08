using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class ServiceNameUpdatedIntegrationEventHandlerTests : IDisposable
{
    private readonly ProvidersDbContext _dbContext;
    private readonly Mock<ILogger<ServiceNameUpdatedIntegrationEventHandler>> _loggerMock = new();
    private readonly ServiceNameUpdatedIntegrationEventHandler _handler;

    public ServiceNameUpdatedIntegrationEventHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProvidersDbContext(options);
        _handler = new ServiceNameUpdatedIntegrationEventHandler(_dbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderServicesExist_ShouldUpdateServiceNameAndSaveChanges()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = new Provider(Guid.NewGuid(), "Provider Name", MeAjudaAi.Modules.Providers.Domain.Enums.EProviderType.Individual, 
            new MeAjudaAi.Modules.Providers.Domain.ValueObjects.BusinessProfile("Provider Name", new MeAjudaAi.Modules.Providers.Domain.ValueObjects.ContactInfo("test@test.com"), null));
        
        provider.AddService(serviceId, "Old Name", 100);
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var evt = new ServiceNameUpdatedIntegrationEvent("ServiceCatalogs", serviceId, "New Name");

        // Act
        await _handler.HandleAsync(evt);

        // Assert
        var updatedProvider = await _dbContext.Providers.Include(p => p.Services).FirstAsync();
        updatedProvider.Services.First(s => s.ServiceId == serviceId).Name.Should().Be("New Name");
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenNoProviderUsesService_ShouldNotThrow()
    {
        // Arrange
        var evt = new ServiceNameUpdatedIntegrationEvent("ServiceCatalogs", Guid.NewGuid(), "New Name");

        // Act
        Func<Task> act = () => _handler.HandleAsync(evt);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseFails_ShouldLogAndRethrow()
    {
        // Arrange
        var evt = new ServiceNameUpdatedIntegrationEvent("ServiceCatalogs", Guid.NewGuid(), "New Name");
        
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ProvidersDbContext(options);
        var handler = new ServiceNameUpdatedIntegrationEventHandler(context, _loggerMock.Object);
        
        // Dispose to force failure on SaveChangesAsync
        context.Dispose();

        // Act
        Func<Task> act = () => handler.HandleAsync(evt);

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
