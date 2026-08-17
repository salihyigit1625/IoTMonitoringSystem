namespace IoTMonitoringSystem.Domain.Entities;

public class SensorMeasurement
{
    public long Id { get; set; }
    public int SensorId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;

    public Sensor? Sensor { get; set; }
}