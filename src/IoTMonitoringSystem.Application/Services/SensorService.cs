using IoTMonitoringSystem.Application.DTOs;
using IoTMonitoringSystem.Application.Interfaces;
using IoTMonitoringSystem.Domain.Entities;
using IoTMonitoringSystem.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace IoTMonitoringSystem.Application.Services;

public class SensorService : ISensorService
{
    private readonly AppDbContext _context;

    public SensorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SensorResponseDto>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.Sensors.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query.Select(s => new SensorResponseDto(
            s.Id, s.Name, s.Type, s.Location, s.IsActive, s.CreatedAt
        )).ToListAsync();
    }

    public async Task<SensorResponseDto?> GetByIdAsync(int id)
    {
        var sensor = await _context.Sensors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return sensor is null ? null : new SensorResponseDto(
            sensor.Id, sensor.Name, sensor.Type, sensor.Location, sensor.IsActive, sensor.CreatedAt
        );
    }

    public async Task<SensorResponseDto> CreateAsync(SensorCreateDto dto)
    {
        var sensor = new Sensor
        {
            Name = dto.Name,
            Type = dto.Type,
            Location = dto.Location,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Sensors.AddAsync(sensor);
        await _context.SaveChangesAsync();

        return new SensorResponseDto(sensor.Id, sensor.Name, sensor.Type, sensor.Location, sensor.IsActive, sensor.CreatedAt);
    }

    public async Task<bool> UpdateAsync(int id, SensorUpdateDto dto)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor is null) return false;

        sensor.Name = dto.Name;
        sensor.Type = dto.Type;
        sensor.Location = dto.Location;
        sensor.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor is null) return false;

        _context.Sensors.Remove(sensor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MeasurementResponseDto?> GetLatestMeasurementAsync(int sensorId)
    {
        var latest = await _context.SensorMeasurements
            .AsNoTracking()
            .Where(m => m.SensorId == sensorId)
            .OrderByDescending(m => m.MeasurementDate)
            .FirstOrDefaultAsync();

        return latest is null ? null : new MeasurementResponseDto(
            latest.Id, latest.SensorId, latest.Temperature, latest.Humidity,
            latest.Pressure, latest.MeasurementDate
        );
    }
}