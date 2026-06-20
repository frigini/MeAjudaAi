using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Services;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Services;

public class DbContextPaymentCommandServiceTests : BaseSqliteInMemoryDatabaseTest<PaymentsDbContext>
{
    private readonly Mock<ILogger<DbContextPaymentCommandService>> _loggerMock;
    private readonly DbContextPaymentCommandService _sut;

    public DbContextPaymentCommandServiceTests()
        : base(options => new PaymentsDbContext(options))
    {
        _loggerMock = new Mock<ILogger<DbContextPaymentCommandService>>();

        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddKeyedSingleton<IUnitOfWork>(ModuleKeys.Payments, DbContext);
        });

        _sut = new DbContextPaymentCommandService(
            serviceProvider.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.Payments),
            DbContext,
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
        var message = await DbContext.InboxMessages.FirstOrDefaultAsync(m => m.ExternalEventId == externalEventId);
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

        DbContext.InboxMessages.Add(new InboxMessage(type, content, externalEventId));
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.SaveInboxMessageAsync(type, content, externalEventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var count = await DbContext.InboxMessages.CountAsync(m => m.ExternalEventId == externalEventId);
        count.Should().Be(1);
    }
}
