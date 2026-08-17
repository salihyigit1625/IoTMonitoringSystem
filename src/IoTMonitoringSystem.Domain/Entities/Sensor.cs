namespace IoTMonitoringSystem.Domain.Entities;

public class Sensor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SensorMeasurement> Measurements { get; set; } = new List<SensorMeasurement>();
}