using IoTMonitoringSystem.Application.DTOs;

namespace IoTMonitoringSystem.Application.Interfaces;

public interface IStatisticsService
{
    Task<SensorStatisticsDto?> GetStatisticsAsync(int sensorId, DateTime? from = null, DateTime? to = null);
}