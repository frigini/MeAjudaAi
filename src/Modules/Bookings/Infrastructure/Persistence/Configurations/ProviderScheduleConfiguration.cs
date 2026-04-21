using MeAjudaAi.Modules.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.Configurations;

public class ProviderScheduleConfiguration : IEntityTypeConfiguration<ProviderSchedule>
{
    public void Configure(EntityTypeBuilder<ProviderSchedule> builder)
    {
        builder.ToTable("provider_schedules");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(ps => ps.ProviderId)
            .IsRequired()
            .HasColumnName("provider_id");

        // Coleção de Value Objects
        builder.OwnsMany(ps => ps.Availabilities, availability =>
        {
            availability.ToTable("provider_availabilities");
            
            availability.WithOwner().HasForeignKey("provider_schedule_id");
            availability.Property<Guid>("id");
            availability.HasKey("id");

            availability.Property(a => a.DayOfWeek)
                .IsRequired()
                .HasColumnName("day_of_week")
                .HasConversion<string>();

            // Índice único para garantir apenas uma configuração por dia da semana para o mesmo schedule
            // Usamos o nome da propriedade CLR, não o nome da coluna
            availability.HasIndex("DayOfWeek", "provider_schedule_id").IsUnique();

            // Slots dentro de cada Availability (Coleção aninhada)
            availability.OwnsMany(a => a.Slots, slot =>
            {
                slot.ToTable("provider_availability_slots");
                
                slot.WithOwner().HasForeignKey("availability_id");
                slot.Property<Guid>("id");
                slot.HasKey("id");

                slot.Property(s => s.Start)
                    .IsRequired()
                    .HasColumnName("start_time")
                    .HasColumnType("timestamptz");

                slot.Property(s => s.End)
                    .IsRequired()
                    .HasColumnName("end_time")
                    .HasColumnType("timestamptz");
            });
        });

        builder.Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(ps => ps.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(ps => ps.ProviderId).IsUnique();
    }
}
