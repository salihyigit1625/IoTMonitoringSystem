using IoTMonitoringSystem.Application.DTOs;
using IoTMonitoringSystem.Application.Interfaces;
using IoTMonitoringSystem.Domain.Entities;
using IoTMonitoringSystem.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace IoTMonitoringSystem.Application.Services;

public class MeasurementService : IMeasurementService
{
    private readonly AppDbContext _context;

    public MeasurementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MeasurementResponseDto> CreateAsync(MeasurementCreateDto dto)
    {
        var measurementDate = dto.MeasurementDate ?? DateTime.UtcNow;

        var entity = new SensorMeasurement
        {
            SensorId = dto.SensorId,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            Pressure = dto.Pressure,
            MeasurementDate = measurementDate.Kind == DateTimeKind.Utc 
                ? measurementDate 
                : DateTime.SpecifyKind(measurementDate, DateTimeKind.Utc)
        };

        await _context.SensorMeasurements.AddAsync(entity);
        await _context.SaveChangesAsync();

        return new MeasurementResponseDto(
            entity.Id,
            entity.SensorId,
            entity.Temperature,
            entity.Humidity,
            entity.Pressure,
            entity.MeasurementDate
        );
    }

    public async Task CreateBatchAsync(IEnumerable<MeasurementCreateDto> dtos)
    {
        var entities = dtos.Select(dto =>
        {
            var measurementDate = dto.MeasurementDate ?? DateTime.UtcNow;
            return new SensorMeasurement
            {
                SensorId = dto.SensorId,
                Temperature = dto.Temperature,
                Humidity = dto.Humidity,
                Pressure = dto.Pressure,
                MeasurementDate = measurementDate.Kind == DateTimeKind.Utc 
                    ? measurementDate 
                    : DateTime.SpecifyKind(measurementDate, DateTimeKind.Utc)
            };
        }).ToList();

        if (entities.Count != 0)
        {
            await _context.SensorMeasurements.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<MeasurementResponseDto?> GetByIdAsync(long id)
    {
        var measurement = await _context.SensorMeasurements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return measurement is null ? null : new MeasurementResponseDto(
            measurement.Id,
            measurement.SensorId,
            measurement.Temperature,
            measurement.Humidity,
            measurement.Pressure,
            measurement.MeasurementDate
        );
    }

    public async Task<IEnumerable<MeasurementResponseDto>> GetAllAsync(
        int? sensorId, 
        DateTime? from, 
        DateTime? to, 
        int page = 1, 
        int pageSize = 50)
    {
        var query = _context.SensorMeasurements.AsNoTracking().AsQueryable();

        if (sensorId.HasValue)
            query = query.Where(x => x.SensorId == sensorId.Value);

        if (from.HasValue)
        {
            var utcFrom = from.Value.Kind == DateTimeKind.Utc 
                ? from.Value 
                : DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
            query = query.Where(x => x.MeasurementDate >= utcFrom);
        }

        if (to.HasValue)
        {
            var utcTo = to.Value.Kind == DateTimeKind.Utc 
                ? to.Value 
                : DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
            query = query.Where(x => x.MeasurementDate <= utcTo);
        }

        return await query
            .OrderByDescending(x => x.MeasurementDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MeasurementResponseDto(
                m.Id,
                m.SensorId,
                m.Temperature,
                m.Humidity,
                m.Pressure,
                m.MeasurementDate
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<MeasurementResponseDto>> GetMeasurementsBySensorAsync(
        int sensorId, 
        DateTime? from, 
        DateTime? to, 
        int limit = 100)
    {
        var query = _context.SensorMeasurements
            .AsNoTracking()
            .Where(x => x.SensorId == sensorId);

        if (from.HasValue)
        {
            var utcFrom = from.Value.Kind == DateTimeKind.Utc 
                ? from.Value 
                : DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
            query = query.Where(x => x.MeasurementDate >= utcFrom);
        }

        if (to.HasValue)
        {
            var utcTo = to.Value.Kind == DateTimeKind.Utc 
                ? to.Value 
                : DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
            query = query.Where(x => x.MeasurementDate <= utcTo);
        }

        return await query
            .OrderByDescending(x => x.MeasurementDate)
            .Take(limit)
            .Select(m => new MeasurementResponseDto(
                m.Id,
                m.SensorId,
                m.Temperature,
                m.Humidity,
                m.Pressure,
                m.MeasurementDate
            ))
            .ToListAsync();
    }
}