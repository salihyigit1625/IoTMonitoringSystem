using System.Reflection;
using IoTMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoTMonitoringSystem.Repository.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorMeasurement> SensorMeasurements => Set<SensorMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}