using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;

public partial class BookingsDbContext : BaseDbContext, IUnitOfWork
{
    public BookingsDbContext(DbContextOptions<BookingsDbContext> options) 
        : base(options)
    {
    }

    public BookingsDbContext(
        DbContextOptions<BookingsDbContext> options, 
        IDomainEventProcessor domainEventProcessor) 
        : base(options, domainEventProcessor)
    {
    }

    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<ProviderSchedule> ProviderSchedules { get; set; } = null!;

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"BookingsDbContext does not support repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name}. " +
            $"Supported: Booking(BookingId), ProviderSchedule(ProviderScheduleId).");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Bookings);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}