using System.Collections.Concurrent;
using IoTMonitoringSystem.Application.DTOs;
using IoTMonitoringSystem.Application.Interfaces;

namespace IoTMonitoringSystem.WorkerService;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly Random _random = new();
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<int, SensorSimState> _sensorStates = new();

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Telemetry Worker Service başlatıldı. Periyot: {Interval} sn", _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sensorService = scope.ServiceProvider.GetRequiredService<ISensorService>();
                var measurementService = scope.ServiceProvider.GetRequiredService<IMeasurementService>();

                var activeSensors = (await sensorService.GetAllAsync(isActive: true)).ToList();

                if (activeSensors.Count != 0)
                {
                    var measurementBatch = new List<MeasurementCreateDto>();

                    foreach (var sensor in activeSensors)
                    {
                        var state = _sensorStates.GetOrAdd(sensor.Id, _ => new SensorSimState
                        {
                            Temperature = 24.5,
                            Humidity = 50.0,
                            Pressure = 1013.25
                        });

                        state.Temperature = Math.Clamp(Math.Round(state.Temperature + ((_random.NextDouble() * 0.6) - 0.3), 2), 15.0, 38.0);
                        state.Humidity = Math.Clamp(Math.Round(state.Humidity + ((_random.NextDouble() * 1.0) - 0.5), 2), 30.0, 85.0);
                        state.Pressure = Math.Clamp(Math.Round(state.Pressure + ((_random.NextDouble() * 0.4) - 0.2), 2), 990.0, 1030.0);

                        measurementBatch.Add(new MeasurementCreateDto(
                            SensorId: sensor.Id,
                            Temperature: state.Temperature,
                            Humidity: state.Humidity,
                            Pressure: state.Pressure,
                            MeasurementDate: DateTime.UtcNow
                        ));

                        _logger.LogInformation(
                            "Kademeli Simülasyon -> [Sensör: {Id} - {Name}] Sıcaklık: {Temp}°C | Nem: {Humidity}% | Basınç: {Pressure} hPa",
                            sensor.Id, sensor.Name, state.Temperature, state.Humidity, state.Pressure
                        );
                    }

                    await measurementService.CreateBatchAsync(measurementBatch);
                }
                else
                {
                    _logger.LogWarning("Aktif sensör bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetri simülasyonu sırasında hata oluştu!");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("IoT Telemetry Worker Service durduruluyor...");
    }

    private sealed class SensorSimState
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
    }
}