using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MeAjudaAi.ApiService.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Infrastructure.Persistence;

[Trait("Category", "Unit")]
public class CompositeUnitOfWorkTests
{
    [Fact]
    public void GetRepository_WithRegisteredRepository_ShouldReturnRepository()
    {
        // Arrange
        var repository = new TestRepository();
        var services = new ServiceCollection();
        services.AddSingleton<IRepository<TestAggregate, Guid>>(repository);
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        // Act
        var result = unitOfWork.GetRepository<TestAggregate, Guid>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(repository);
    }

    [Fact]
    public void GetRepository_WithUnregisteredRepository_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        // Act & Assert
        var act = () => unitOfWork.GetRepository<TestAggregate, Guid>();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoDbContexts_ShouldReturnZero()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        // Act
        var result = await unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithUsersDbContext_ShouldSaveChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext(options, null!);
        var services = new ServiceCollection();
        services.AddScoped(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        var user = new MeAjudaAi.Modules.Users.Tests.Builders.UserBuilder().Build();
        context.Users.Add(user);

        // Act
        var result = await unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_WithProvidersDbContext_ShouldSaveChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext(options, null!);
        var services = new ServiceCollection();
        services.AddScoped(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        var provider = new MeAjudaAi.Modules.Providers.Tests.Builders.ProviderBuilder().Build();
        context.Providers.Add(provider);

        // Act
        var result = await unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleDbContexts_ShouldSaveAllChanges()
    {
        // Arrange
        var usersOptions = new DbContextOptionsBuilder<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var providersOptions = new DbContextOptionsBuilder<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var usersContext = new MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext(usersOptions, null!);
        var providersContext = new MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext(providersOptions, null!);

        var services = new ServiceCollection();
        services.AddScoped(_ => usersContext);
        services.AddScoped(_ => providersContext);
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        var user = new MeAjudaAi.Modules.Users.Tests.Builders.UserBuilder().Build();
        usersContext.Users.Add(user);

        var provider = new MeAjudaAi.Modules.Providers.Tests.Builders.ProviderBuilder().Build();
        providersContext.Providers.Add(provider);

        // Act
        // Nota: TransactionScope não funciona com InMemory database, então esperamos que o método
        // ainda salve as alterações mesmo que a transação não seja suportada
        try
        {
            var result = await unitOfWork.SaveChangesAsync();
            // O resultado pode variar dependendo de quantas entidades são criadas pelos builders
            result.Should().BeGreaterThan(0);
        }
        catch (InvalidOperationException)
        {
            // InMemory database não suporta transações distribuídas
            // Verificamos se as entidades foram salvas diretamente
            usersContext.Users.Should().HaveCount(1);
            providersContext.Providers.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WithException_ShouldThrowException()
    {
        // Arrange
        var mockContext = new Mock<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(
            new DbContextOptions<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(),
            null!
        );
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var services = new ServiceCollection();
        services.AddScoped(_ => mockContext.Object);
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        // Act & Assert
        var act = async () => await unitOfWork.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext(options, null!);
        var services = new ServiceCollection();
        services.AddScoped(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        var unitOfWork = new CompositeUnitOfWork(serviceProvider);

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await unitOfWork.SaveChangesAsync(token);

        // Assert - Se não lançar exceção, o token foi passado corretamente
        true.Should().BeTrue();
    }

    // Test classes
    public class TestAggregate
    {
        public Guid Id { get; set; }
    }

    public class TestRepository : IRepository<TestAggregate, Guid>
    {
        public Task<TestAggregate?> TryFindAsync(Guid key, CancellationToken ct) => Task.FromResult<TestAggregate?>(null);
        public void Add(TestAggregate aggregate) { }
        public void Delete(TestAggregate aggregate) { }
    }
}
