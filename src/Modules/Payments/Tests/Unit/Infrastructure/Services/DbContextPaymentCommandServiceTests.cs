using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Services;

public class DbContextPaymentCommandServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PaymentsDbContext _dbContext;
    private readonly Mock<ILogger<DbContextPaymentCommandService>> _loggerMock;
    private readonly DbContextPaymentCommandService _sut;

    public DbContextPaymentCommandServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PaymentsDbContext(options);
        _dbContext.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<DbContextPaymentCommandService>>();
        
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IUnitOfWork>(ModuleKeys.Payments, _dbContext);
        var serviceProvider = services.BuildServiceProvider();
        
        _sut = new DbContextPaymentCommandService(
            serviceProvider.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.Payments),
            _dbContext,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SaveInboxMessageAsync_ShouldAddAndSave()
    {
        // Arrange
        var type = "checkout.session.completed";
        var content = "{}";
        var externalEventId = "evt_1";

        // Act
        var result = await _sut.SaveInboxMessageAsync(type, content, externalEventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var message = await _dbContext.InboxMessages.FirstOrDefaultAsync(m => m.ExternalEventId == externalEventId);
        message.Should().NotBeNull();
        message!.Type.Should().Be(type);
    }

    [Fact]
    public async Task SaveInboxMessageAsync_ShouldSkipIfEventExists()
    {
        // Arrange
        var type = "checkout.session.completed";
        var content = "{}";
        var externalEventId = "evt_1";
        
        _dbContext.InboxMessages.Add(new InboxMessage(type, content, externalEventId));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.SaveInboxMessageAsync(type, content, externalEventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var count = await _dbContext.InboxMessages.CountAsync(m => m.ExternalEventId == externalEventId);
        count.Should().Be(1);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}



