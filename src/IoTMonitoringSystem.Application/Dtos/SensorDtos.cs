namespace IoTMonitoringSystem.Application.DTOs;

public record SensorCreateDto(
    string Name,
    string Type,
    string Location,
    bool IsActive = true
);

public record SensorUpdateDto(
    string Name,
    string Type,
    string Location,
    bool IsActive
);

public record SensorResponseDto(
    int Id,
    string Name,
    string Type,
    string Location,
    bool IsActive,
    DateTime CreatedAt
);