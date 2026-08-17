using IoTMonitoringSystem.Application.DTOs;

namespace IoTMonitoringSystem.Application.Interfaces;

public interface IMeasurementService
{
    Task<MeasurementResponseDto> CreateAsync(MeasurementCreateDto dto);
    Task CreateBatchAsync(IEnumerable<MeasurementCreateDto> dtos);
    Task<MeasurementResponseDto?> GetByIdAsync(long id);
    Task<IEnumerable<MeasurementResponseDto>> GetAllAsync(int? sensorId, DateTime? from, DateTime? to, int page = 1, int pageSize = 50);
    Task<IEnumerable<MeasurementResponseDto>> GetMeasurementsBySensorAsync(int sensorId, DateTime? from, DateTime? to, int limit = 100);
}