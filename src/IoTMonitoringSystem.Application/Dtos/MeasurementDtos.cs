namespace IoTMonitoringSystem.Application.DTOs;

public record MeasurementCreateDto(
    int SensorId,
    double Temperature,
    double Humidity,
    double Pressure,
    DateTime? MeasurementDate = null
);

public record MeasurementResponseDto(
    long Id,
    int SensorId,
    double Temperature,
    double Humidity,
    double Pressure,
    DateTime MeasurementDate
);