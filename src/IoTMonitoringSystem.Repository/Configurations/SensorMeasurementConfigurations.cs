using IoTMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTMonitoringSystem.Repository.Configurations;

public class SensorMeasurementConfiguration : IEntityTypeConfiguration<SensorMeasurement>
{
    public void Configure(EntityTypeBuilder<SensorMeasurement> builder)
    {
        builder.ToTable("SensorMeasurements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Temperature).IsRequired();
        builder.Property(m => m.Humidity).IsRequired();
        builder.Property(m => m.Pressure).IsRequired();
        builder.Property(m => m.MeasurementDate).IsRequired();

        builder.HasOne(m => m.Sensor)
            .WithMany(s => s.Measurements)
            .HasForeignKey(m => m.SensorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.SensorId, m.MeasurementDate })
            .IsDescending(false, true)
            .HasDatabaseName("IX_SensorMeasurements_SensorId_MeasurementDate");
    }
}