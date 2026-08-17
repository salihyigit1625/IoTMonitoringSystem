using IoTMonitoringSystem.Application.DTOs;
using IoTMonitoringSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IoTMonitoringSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;
    private readonly IMeasurementService _measurementService;
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<SensorsController> _logger;

    public SensorsController(
        ISensorService sensorService,
        IMeasurementService measurementService,
        IStatisticsService statisticsService,
        ILogger<SensorsController> logger)
    {
        _sensorService = sensorService;
        _measurementService = measurementService;
        _statisticsService = statisticsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var sensors = await _sensorService.GetAllAsync(isActive);
        return Ok(sensors);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sensor = await _sensorService.GetByIdAsync(id);
        if (sensor is null)
            return NotFound(new { message = $"Sensör (Id: {id}) bulunamadı." });

        return Ok(sensor);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SensorCreateDto dto)
    {
        var created = await _sensorService.CreateAsync(dto);
        _logger.LogInformation("Yeni sensör oluşturuldu: {Name} (Id: {Id})", created.Name, created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SensorUpdateDto dto)
    {
        var updated = await _sensorService.UpdateAsync(id, dto);
        if (!updated)
            return NotFound(new { message = $"Güncellenecek sensör (Id: {id}) bulunamadı." });

        _logger.LogInformation("Sensör güncellendi: Id {Id}", id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _sensorService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Silinecek sensör (Id: {id}) bulunamadı." });

        _logger.LogInformation("Sensör silindi: Id {Id}", id);
        return NoContent();
    }

    [HttpGet("{id:int}/latest")]
    public async Task<IActionResult> GetLatest(int id)
    {
        var latest = await _sensorService.GetLatestMeasurementAsync(id);
        if (latest is null)
            return NotFound(new { message = $"Sensöre (Id: {id}) ait ölçüm verisi bulunamadı." });

        return Ok(latest);
    }

    [HttpGet("{id:int}/measurements")]
    public async Task<IActionResult> GetMeasurements(
        int id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 100)
    {
        var measurements = await _measurementService.GetMeasurementsBySensorAsync(id, from, to, limit);
        return Ok(measurements);
    }

    [HttpGet("{id:int}/statistics")]
    public async Task<IActionResult> GetStatistics(
        int id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var stats = await _statisticsService.GetStatisticsAsync(id, from, to);
        if (stats is null)
            return NotFound(new { message = $"Sensör (Id: {id}) bulunamadı." });

        return Ok(stats);
    }
    
    [HttpPatch("{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var sensor = await _sensorService.GetByIdAsync(id);
        if (sensor is null)
            return NotFound(new { message = $"Sensör (Id: {id}) bulunamadı." });

        var updateDto = new SensorUpdateDto(
            sensor.Name,
            sensor.Type,
            sensor.Location,
            !sensor.IsActive
        );

        await _sensorService.UpdateAsync(id, updateDto);
        _logger.LogInformation("Sensör aktiflik durumu değiştirildi: Id {Id}, Yeni Durum: {IsActive}", id, !sensor.IsActive);
        return Ok(new { message = $"Sensör durumu {(updateDto.IsActive ? "Aktif" : "Pasif")} yapıldı.", isActive = updateDto.IsActive });
    }
    
}