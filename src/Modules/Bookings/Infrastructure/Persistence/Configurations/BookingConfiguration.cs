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

        builder.OwnsOne(b => b.TimeSlot, timeSlot =>
        {
            timeSlot.Property(ts => ts.Start)
                .IsRequired()
                .HasColumnName("start_time")
                .HasColumnType("timestamptz");
            
            timeSlot.Property(ts => ts.End)
                .IsRequired()
                .HasColumnName("end_time")
                .HasColumnType("timestamptz");
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

        // Índices otimizados para busca de sobreposição e listagem
        // Para indexar campos de owned types, devemos usar o builder.OwnsOne e configurar o index dentro dele ou referenciar as propriedades via string se já configuradas.
        // A forma mais robusta é usar as propriedades mapeadas.
        builder.HasIndex(b => new { b.ProviderId, b.Status });
        
        builder.HasIndex(b => b.ClientId);
        builder.HasIndex(b => b.Status);
    }
}
