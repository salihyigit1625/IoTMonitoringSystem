using IoTMonitoringSystem.Application.Interfaces;
using IoTMonitoringSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IoTMonitoringSystem.Application.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<IMeasurementService, MeasurementService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        return services;
    }
}