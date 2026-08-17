using IoTMonitoringSystem.Application.DTOs;

namespace IoTMonitoringSystem.Application.Interfaces;

public interface ISensorService
{
    Task<IEnumerable<SensorResponseDto>> GetAllAsync(bool? isActive = null);
    Task<SensorResponseDto?> GetByIdAsync(int id);
    Task<SensorResponseDto> CreateAsync(SensorCreateDto dto);
    Task<bool> UpdateAsync(int id, SensorUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<MeasurementResponseDto?> GetLatestMeasurementAsync(int sensorId);
}