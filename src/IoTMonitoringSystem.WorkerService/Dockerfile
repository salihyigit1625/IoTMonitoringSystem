FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/IoTMonitoringSystem.WorkerService/IoTMonitoringSystem.WorkerService.csproj", "src/IoTMonitoringSystem.WorkerService/"]
COPY ["src/IoTMonitoringSystem.Application/IoTMonitoringSystem.Application.csproj", "src/IoTMonitoringSystem.Application/"]
COPY ["src/IoTMonitoringSystem.Domain/IoTMonitoringSystem.Domain.csproj", "src/IoTMonitoringSystem.Domain/"]
COPY ["src/IoTMonitoringSystem.Repository/IoTMonitoringSystem.Repository.csproj", "src/IoTMonitoringSystem.Repository/"]

RUN dotnet restore "src/IoTMonitoringSystem.WorkerService/IoTMonitoringSystem.WorkerService.csproj"

COPY . .
WORKDIR "/src/src/IoTMonitoringSystem.WorkerService"
RUN dotnet build "./IoTMonitoringSystem.WorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IoTMonitoringSystem.WorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTMonitoringSystem.WorkerService.dll"]