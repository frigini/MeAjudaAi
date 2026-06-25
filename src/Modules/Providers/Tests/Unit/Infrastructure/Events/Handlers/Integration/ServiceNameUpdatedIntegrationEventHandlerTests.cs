using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class ServiceNameUpdatedIntegrationEventHandlerTests : BaseInMemoryDatabaseTest<ProvidersDbContext>
{
    private readonly Mock<ILogger<ServiceNameUpdatedIntegrationEventHandler>> _loggerMock = new();

    public ServiceNameUpdatedIntegrationEventHandlerTests() : base(options => new ProvidersDbContext(options))
    {
    }

    private ServiceNameUpdatedIntegrationEventHandler CreateHandler() => new(DbContext, _loggerMock.Object);

    [Fact]
    public async Task HandleAsync_WhenProviderServicesExist_ShouldUpdateServiceNameAndSaveChanges()
    {
        var serviceId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithName("Provider Name")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(new BusinessProfile("Provider Name", new ContactInfo("test@test.com"), null))
            .Build();

        provider.AddService(serviceId, "Old Name");
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var evt = new ServiceNameUpdatedIntegrationEvent("ServiceCatalogs", serviceId, "New Name");

        var handler = CreateHandler();

        await handler.HandleAsync(evt);

        var updatedProvider = await DbContext.Providers.Include(p => p.Services).FirstAsync();
        updatedProvider.Services.First(s => s.ServiceId == serviceId).ServiceName.Should().Be("New Name");
        DbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenNoProviderUsesService_ShouldNotThrow()
    {
        var evt = new ServiceNameUpdatedIntegrationEvent("ServiceCatalogs", Guid.NewGuid(), "New Name");
        var handler = CreateHandler();

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseFails_ShouldLogAndRethrow()
    {
        var evt = new ServiceNameUpdatedIntegrationEvent("ServiceCatalogs", Guid.NewGuid(), "New Name");

        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new ProvidersDbContext(options);
        var handler = new ServiceNameUpdatedIntegrationEventHandler(context, _loggerMock.Object);

        context.Dispose();

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().ThrowAsync<ObjectDisposedException>();
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
