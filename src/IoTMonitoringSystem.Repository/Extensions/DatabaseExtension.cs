using IoTMonitoringSystem.Domain.Entities;
using IoTMonitoringSystem.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IoTMonitoringSystem.Repository.Extensions;

public static class DatabaseExtension
{
    public static async Task ApplyMigrationsAndSeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Veritabanı migration kontrolü yapılıyor...");
            await context.Database.MigrateAsync();

            if (!await context.Sensors.AnyAsync())
            {
                logger.LogInformation("Varsayılan sensörler ekleniyor...");
                var defaultSensors = new List<Sensor>
                {
                    new() { Name = "Sensor-001", Type = "Temperature/Humidity/Pressure", Location = "Bursa Depo - Bölüm A", IsActive = true },
                    new() { Name = "Sensor-002", Type = "Temperature/Humidity/Pressure", Location = "Bursa Üretim Hattı 1", IsActive = true },
                    new() { Name = "Sensor-003", Type = "Temperature/Humidity/Pressure", Location = "Sunucu Odası", IsActive = false }
                };

                await context.Sensors.AddRangeAsync(defaultSensors);
                await context.SaveChangesAsync();
                logger.LogInformation("Varsayılan sensörler başarıyla eklendi.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabanı migration/seed sırasında hata oluştu.");
            throw;
        }
    }
}