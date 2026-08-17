using IoTMonitoringSystem.Application.DTOs;
using IoTMonitoringSystem.Application.Interfaces;
using IoTMonitoringSystem.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace IoTMonitoringSystem.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SensorStatisticsDto?> GetStatisticsAsync(int sensorId, DateTime? from, DateTime? to)
    {
        var sensor = await _context.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sensorId);

        if (sensor is null)
            return null;

        var query = _context.SensorMeasurements
            .AsNoTracking()
            .Where(m => m.SensorId == sensorId);

        if (from.HasValue)
        {
            var utcFrom = from.Value.Kind == DateTimeKind.Utc 
                ? from.Value 
                : DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
            query = query.Where(m => m.MeasurementDate >= utcFrom);
        }

        if (to.HasValue)
        {
            var utcTo = to.Value.Kind == DateTimeKind.Utc 
                ? to.Value 
                : DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
            query = query.Where(m => m.MeasurementDate <= utcTo);
        }

        var stats = await query
            .GroupBy(m => m.SensorId)
            .Select(g => new
            {
                Count = g.Count(),
                AvgTemp = g.Average(m => m.Temperature),
                MinTemp = g.Min(m => m.Temperature),
                MaxTemp = g.Max(m => m.Temperature),
                AvgHum = g.Average(m => m.Humidity),
                MinHum = g.Min(m => m.Humidity),
                MaxHum = g.Max(m => m.Humidity),
                AvgPress = g.Average(m => m.Pressure),
                MinPress = g.Min(m => m.Pressure),
                MaxPress = g.Max(m => m.Pressure)
            })
            .FirstOrDefaultAsync();

        if (stats is null || stats.Count == 0)
        {
            return new SensorStatisticsDto(
                SensorId: sensor.Id,
                SensorName: sensor.Name,
                From: from,
                To: to,
                TotalMeasurements: 0,
                AvgTemperature: 0,
                MinTemperature: 0,
                MaxTemperature: 0,
                AvgHumidity: 0,
                MinHumidity: 0,
                MaxHumidity: 0,
                AvgPressure: 0,
                MinPressure: 0,
                MaxPressure: 0
            );
        }

        return new SensorStatisticsDto(
            SensorId: sensor.Id,
            SensorName: sensor.Name,
            From: from,
            To: to,
            TotalMeasurements: stats.Count,
            AvgTemperature: Math.Round(stats.AvgTemp, 2),
            MinTemperature: stats.MinTemp,
            MaxTemperature: stats.MaxTemp,
            AvgHumidity: Math.Round(stats.AvgHum, 2),
            MinHumidity: stats.MinHum,
            MaxHumidity: stats.MaxHum,
            AvgPressure: Math.Round(stats.AvgPress, 2),
            MinPressure: stats.MinPress,
            MaxPressure: stats.MaxPress
        );
    }
}