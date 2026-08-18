# IoT Sensor Monitoring & Real-time Telemetry Platform

.NET tabanlı, konteynerize edilmiş IoT telemetri toplama ve izleme platformu. Sensör verisi arka planda simüle edilir, PostgreSQL'e yazılır, Grafana üzerinden görselleştirilip eşik bazlı alarma bağlanır.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Grafana](https://img.shields.io/badge/Grafana-Unified%20Alerting-F46800?logo=grafana&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

---

## Mimari

```
Worker (random walk sim.) ─┐
                            ├─▶ API (Clean Arch) ─▶ PostgreSQL ─▶ Grafana ─▶ Alert (>40°C)
Manual Ingestion (curl) ───┘
```

- **API**: Katmanlı mimari — `API / Application / Domain / Infrastructure`. Domain katmanı framework bağımsız.
- **Worker**: Ayrı süreç, sürekli çalışan background service. Yazma yükünü API'den izole eder.
- **Grafana**: PostgreSQL'e doğrudan bağlanır, API'ye bağımlı değil.

## Simülasyon Motoru

Rastgele değer yerine random walk:

```
value = clamp(value + step(-Δ, +Δ), min, max)
```

Ani sıçrama yok, sensör ataletini taklit eder. Amaç: Grafana'da gerçek zaman serisine benzeyen, gürültüsüz veri.

## Grafana Provisioning

Datasource ve dashboard, UI'dan bağımsız olarak dosya sisteminden yüklenir:

```
grafana/provisioning/
├── datasources/postgres-datasource.yml
└── dashboards/{dashboard-provider.yml, iot-dashboard.json}
```

- `partitionByValues` → tek sorgudan dönen çoklu sensör verisini ayrı time series olarak böler.
- `${sensor:sqlstring}` → dashboard variable'ını SQL injection riski olmadan `WHERE` koşuluna basar.

## API

| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/sensors` | Sensör oluştur |
| `GET` | `/api/sensors` | Sensörleri listele |
| `GET` | `/api/sensors/{id}` | Sensör detayı |
| `PUT` | `/api/sensors/{id}` | Sensör güncelle |
| `DELETE` | `/api/sensors/{id}` | Sensör sil |
| `POST` | `/api/telemetry` | Telemetri kaydet (ingestion) |
| `GET` | `/api/telemetry/latest` | Sensör başına son okuma |
| `GET` | `/api/telemetry/statistics` | `startDate`–`endDate` aralığında min/max/avg |

## Quickstart

```bash
docker compose up -d
```

Migration'lar API başlangıcında otomatik uygulanır.

| Servis | Adres | Auth |
|---|---|---|
| API / Swagger | `localhost:5000/swagger` | — |
| Grafana | `localhost:3000` | `admin` / `admin` |
| PostgreSQL | `localhost:5432` | `postgres` / `postgres` |

Container'lar: `iot-postgres`, `iot-api`, `iot-worker`, `iot-grafana`

## Alarm Testi

Grafana Unified Alerting kuralı: `temperature > 40°C` → `Firing`

```bash
curl -X POST http://localhost:5000/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{"sensorId":1,"temperature":45.5,"humidity":40,"timestamp":"2026-08-17T12:00:00Z"}'
```

`Grafana → Alerting → Alert rules` üzerinden `Firing` durumunu doğrula.
