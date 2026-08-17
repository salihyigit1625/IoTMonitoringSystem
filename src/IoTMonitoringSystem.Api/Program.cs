using IoTMonitoringSystem.Application.Extensions;
using IoTMonitoringSystem.Repository.Context;
using IoTMonitoringSystem.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/api-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("IoT Monitoring API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // DbContext
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Application Servisleri
    builder.Services.AddApplicationServices();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Swagger - Her ortamda (Docker dahil) çalışacak şekilde ve root (/) rotasında
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoT Monitoring API v1");
        c.RoutePrefix = string.Empty; // http://localhost:5000 doğrudan Swagger'ı açar
    });
    
    app.UseMiddleware<IoTMonitoringSystem.Api.Middlewares.ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging();
    app.UseAuthorization();
    app.MapControllers();

    // Otomatik Migration & Seed
    await app.Services.ApplyMigrationsAndSeedAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API beklenmeyen bir hata nedeniyle durduruldu!");
}
finally
{
    Log.CloseAndFlush();
}