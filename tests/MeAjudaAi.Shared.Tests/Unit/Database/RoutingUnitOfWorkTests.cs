using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Shared.Database;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public sealed class RoutingUnitOfWorkTests
{
    private sealed class FakeAggregate { public Guid Id { get; set; } = Guid.NewGuid(); }

    // UoW que também implementa IRepository<FakeAggregate, Guid>
    private sealed class FakeModuleUow : IUnitOfWork, IRepository<FakeAggregate, Guid>
    {
        public int SaveCalls;
        public Task<FakeAggregate?> TryFindAsync(Guid key, CancellationToken ct) => Task.FromResult<FakeAggregate?>(null);
        public void Add(FakeAggregate aggregate) { }
        public void Delete(FakeAggregate aggregate) { }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) { SaveCalls++; return Task.FromResult(1); }
        public IRepository<TAgg, TKey> GetRepository<TAgg, TKey>() => throw new NotSupportedException();
    }

    private sealed class OtherUow : IUnitOfWork
    {
        public int SaveCalls;
        public Task<int> SaveChangesAsync(CancellationToken ct = default) { SaveCalls++; return Task.FromResult(2); }
        public IRepository<TAgg, TKey> GetRepository<TAgg, TKey>() => throw new NotSupportedException();
    }

    [Fact]
    public void GetRepository_WithSingleMatch_ShouldReturnRepo()
    {
        var sc = new ServiceCollection();
        sc.AddScoped<IUnitOfWork>(sp => new FakeModuleUow()); // match
        sc.AddScoped<IUnitOfWork>(sp => new OtherUow());      // não match
        using var sp = sc.BuildServiceProvider();

        var routing = new RoutingUnitOfWork(sp);
        var repo = routing.GetRepository<FakeAggregate, Guid>();

        repo.Should().NotBeNull();
    }

    [Fact]
    public void GetRepository_WithNoMatch_ShouldThrow()
    {
        var sc = new ServiceCollection();
        sc.AddScoped<IUnitOfWork>(sp => new OtherUow());
        using var sp = sc.BuildServiceProvider();

        var routing = new RoutingUnitOfWork(sp);
        Action act = () => routing.GetRepository<FakeAggregate, Guid>();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*No Unit of Work (DbContext) found*");
    }

    private sealed class AnotherFakeModuleUow : IUnitOfWork, IRepository<FakeAggregate, Guid>
    {
        public Task<FakeAggregate?> TryFindAsync(Guid key, CancellationToken ct) => Task.FromResult<FakeAggregate?>(null);
        public void Add(FakeAggregate aggregate) { }
        public void Delete(FakeAggregate aggregate) { }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
        public IRepository<TAgg, TKey> GetRepository<TAgg, TKey>() => throw new NotSupportedException();
    }

    [Fact]
    public void GetRepository_WithAmbiguousMatch_ShouldThrow()
    {
        var sc = new ServiceCollection();
        sc.AddScoped<IUnitOfWork>(sp => new FakeModuleUow());
        sc.AddScoped<IUnitOfWork>(sp => new AnotherFakeModuleUow());
        using var sp = sc.BuildServiceProvider();

        var routing = new RoutingUnitOfWork(sp);
        Action act = () => routing.GetRepository<FakeAggregate, Guid>();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Ambiguous Unit of Work routing*");
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleUows_ShouldSumChanges()
    {
        var sc = new ServiceCollection();
        sc.AddScoped<IUnitOfWork>(sp => new FakeModuleUow()); // returns 1
        sc.AddScoped<IUnitOfWork>(sp => new OtherUow());      // returns 2
        using var sp = sc.BuildServiceProvider();

        var routing = new RoutingUnitOfWork(sp);
        var total = await routing.SaveChangesAsync();

        total.Should().Be(3);
    }
}
