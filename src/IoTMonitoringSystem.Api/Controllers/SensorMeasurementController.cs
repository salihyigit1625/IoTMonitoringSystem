using IoTMonitoringSystem.Application.DTOs;
using IoTMonitoringSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IoTMonitoringSystem.Api.Controllers;

[ApiController]
[Route("api/sensor-measurements")]
public class SensorMeasurementsController : ControllerBase
{
    private readonly IMeasurementService _measurementService;
    private readonly ILogger<SensorMeasurementsController> _logger;

    public SensorMeasurementsController(
        IMeasurementService measurementService,
        ILogger<SensorMeasurementsController> logger)
    {
        _measurementService = measurementService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MeasurementCreateDto dto)
    {
        var created = await _measurementService.CreateAsync(dto);
        _logger.LogInformation("Manuel ölçüm eklendi: Sensör {SensorId}, Sıcaklık: {Temp}°C", created.SensorId, created.Temperature);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? sensorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var measurements = await _measurementService.GetAllAsync(sensorId, from, to, page, pageSize);
        return Ok(measurements);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var measurement = await _measurementService.GetByIdAsync(id);
        if (measurement is null)
            return NotFound(new { message = $"Ölçüm kaydı (Id: {id}) bulunamadı." });

        return Ok(measurement);
    }
}