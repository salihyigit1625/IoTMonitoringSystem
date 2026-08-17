namespace IoTMonitoringSystem.Application.DTOs;

public record SensorStatisticsDto(
    int SensorId,
    string SensorName,
    DateTime? From,
    DateTime? To,
    int TotalMeasurements,
    double AvgTemperature,
    double MinTemperature,
    double MaxTemperature,
    double AvgHumidity,
    double MinHumidity,
    double MaxHumidity,
    double AvgPressure,
    double MinPressure,
    double MaxPressure
);