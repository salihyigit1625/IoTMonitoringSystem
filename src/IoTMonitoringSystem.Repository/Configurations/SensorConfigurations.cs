using IoTMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTMonitoringSystem.Repository.Configurations;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("Sensors");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Location)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Sensors_IsActive");
        
        builder.HasMany(s => s.Measurements)
            .WithOne(m => m.Sensor)
            .HasForeignKey(m => m.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}