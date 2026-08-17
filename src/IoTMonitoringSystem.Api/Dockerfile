FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/IoTMonitoringSystem.Api/IoTMonitoringSystem.Api.csproj", "src/IoTMonitoringSystem.Api/"]
COPY ["src/IoTMonitoringSystem.Application/IoTMonitoringSystem.Application.csproj", "src/IoTMonitoringSystem.Application/"]
COPY ["src/IoTMonitoringSystem.Domain/IoTMonitoringSystem.Domain.csproj", "src/IoTMonitoringSystem.Domain/"]
COPY ["src/IoTMonitoringSystem.Repository/IoTMonitoringSystem.Repository.csproj", "src/IoTMonitoringSystem.Repository/"]

RUN dotnet restore "src/IoTMonitoringSystem.Api/IoTMonitoringSystem.Api.csproj"

COPY . .
WORKDIR "/src/src/IoTMonitoringSystem.Api"
RUN dotnet build "./IoTMonitoringSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IoTMonitoringSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTMonitoringSystem.Api.dll"]