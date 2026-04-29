using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(b => b.ProviderId)
            .IsRequired()
            .HasColumnName("provider_id");

        builder.Property(b => b.ClientId)
            .IsRequired()
            .HasColumnName("client_id");

        builder.Property(b => b.ServiceId)
            .IsRequired()
            .HasColumnName("service_id");

        builder.Property(b => b.Date)
            .IsRequired()
            .HasColumnName("booking_date");

        builder.OwnsOne(b => b.TimeSlot, timeSlot =>
        {
            timeSlot.Property(ts => ts.Start)
                .IsRequired()
                .HasColumnName("start_time")
                .HasColumnType("time"); // Forçar 'time' para TimeOnly
            
            timeSlot.Property(ts => ts.End)
                .IsRequired()
                .HasColumnName("end_time")
                .HasColumnType("time");
        });

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(b => b.RejectionReason)
            .HasMaxLength(500)
            .HasColumnName("rejection_reason");

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500)
            .HasColumnName("cancellation_reason");

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(b => b.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Índice composto para busca de agendamentos ativos por prestador, data e status
        // Otimiza: GetActiveByProviderAndDateAsync (WHERE provider_id=X AND booking_date=X AND status=X)
        builder.HasIndex(b => new { b.ProviderId, b.Date, b.Status })
            .HasDatabaseName("ix_bookings_provider_date_status");

        // Índice composto para paginação por cliente com ordenação por data
        // Otimiza: GetByClientIdPagedAsync (WHERE client_id=X [date range] ORDER BY booking_date DESC)
        builder.HasIndex(b => new { b.ClientId, b.Date })
            .HasDatabaseName("ix_bookings_client_date");

        // Índice composto para paginação por prestador com ordenação por data
        // Otimiza: GetByProviderIdPagedAsync sem filtro de status (WHERE provider_id=X [date range] ORDER BY booking_date DESC)
        builder.HasIndex(b => new { b.ProviderId, b.Date })
            .HasDatabaseName("ix_bookings_provider_date");
    }
}
