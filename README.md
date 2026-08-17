# IoT Sensor Monitoring & Real-time Telemetry Platform

Endüstriyel IoT sensörlerinden gelen telemetri verisini toplayan, kalıcı hale getiren ve gerçek zamanlı olarak izleyip alarm üreten, konteynerleştirilmiş bir .NET tabanlı izleme platformu.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Grafana](https://img.shields.io/badge/Grafana-Unified%20Alerting-F46800?logo=grafana&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-Code--First-512BD4)
![Serilog](https://img.shields.io/badge/Logging-Serilog-gray)

---

## Sistem Mimarisi ve Veri Akışı

Platform, veri üretimi ile veri sunumunu birbirinden bağımsız iki servise (`Worker` ve `API`) ayırır. Bu sayede telemetri üretim yükü, sorgu/okuma yükünü etkilemez.

```
┌──────────────────┐        ┌──────────────────┐
│   IoT Worker      │        │  Manual Ingestion │
│ (Random Walk Sim.)│        │  (Swagger / curl)  │
└─────────┬─────────┘        └─────────┬─────────┘
          │  HTTP POST                  │  HTTP POST
          ▼                             ▼
        ┌───────────────────────────────────┐
        │            iot-api                 │
        │  (Clean Architecture / REST API)   │
        └────────────────┬────────────────────┘
                          │ EF Core (Code-First)
                          ▼
                 ┌──────────────────┐
                 │   iot-postgres    │
                 │  (PostgreSQL 16)  │
                 └────────┬───────────┘
                          │ Provisioned Datasource
                          ▼
                 ┌──────────────────┐
                 │    iot-grafana     │
                 │ Dashboard + Alert  │
                 └────────┬───────────┘
                          │ Threshold > 40°C
                          ▼
                  🔔 Unified Alerting
                     (Firing State)
```

**Akış özeti:** Worker Service, her sensör için ataletli (kademeli) bir simülasyon üretir → API'ye POST eder → API, EF Core üzerinden PostgreSQL'e yazar → Grafana, aynı veritabanını datasource olarak okuyup dashboard'da görselleştirir ve eşik aşımında alarm tetikler.

---

## Teknik Özellikler & Mühendislik Kararları

### Katmanlı Mimari (Separation of Concerns)
Proje, test edilebilirliği ve bakım maliyetini düşürmek amacıyla dört katmana ayrılmıştır:

| Katman | Sorumluluk |
|---|---|
| **API** | HTTP kontratları, request/response modelleri, middleware, Serilog konfigürasyonu |
| **Application** | İş kuralları, servisler, DTO mapping, validasyon |
| **Domain** | Entity'ler, value object'ler, framework'ten bağımsız çekirdek model |
| **Infrastructure / Repository** | EF Core `DbContext`, migration'lar, repository implementasyonları |

Domain katmanı hiçbir dış bağımlılık içermez; bu sayede iş kuralları veritabanı veya framework değişikliklerinden izole kalır.

### Gerçekçi Simülasyon Motoru (Random Walk)
Worker Service, veriyi `Random.Next()` ile salt rastgele üretmek yerine **kademeli değişim (random walk)** algoritması kullanır:

```
yeniDeğer = mevcutDeğer + rastgeleAdım(-Δ, +Δ)
yeniDeğer = Clamp(yeniDeğer, minFizikselSınır, maxFizikselSınır)
```

Bu yaklaşım, gerçek bir sıcaklık/nem sensöründe bir okumadan diğerine ani sıçramalar yaşanmayacağı fiziksel gerçeğini simüle eder — sensörün **ataletini (inertia)** taklit eder. Sonuç olarak Grafana grafiklerinde gürültülü/anlamsız çizgiler yerine, gerçek zaman serisi verisine benzeyen organik dalgalanmalar görülür.

### Grafana Auto-Provisioning
Grafana konteyneri, UI üzerinden manuel "Add Datasource" veya "Import Dashboard" adımı gerektirmeden, dosya sisteminden otomatik olarak ayağa kalkar:

```
grafana/provisioning/
├── datasources/
│   └── postgres-datasource.yml   # Bağlantı bilgileri otomatik yüklenir
└── dashboards/
    ├── dashboard-provider.yml    # JSON dashboard'ların taranacağı dizin
    └── iot-dashboard.json        # Panel tanımları
```

Öne çıkan sorgu teknikleri:
- **`partitionByValues`** — tek bir SQL sorgusundan dönen çoklu sensör verisini, zaman serisi panelinde her sensör için ayrı bir seri olarak böler.
- **`${sensor:sqlstring}`** — dashboard üstündeki template variable'ı, SQL injection'a karşı güvenli şekilde `WHERE` koşuluna enjekte eder ve kullanıcının tek bir sensöre filtre uygulamasını sağlar.

---

## API Referansı

| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/sensors` | Yeni sensör tanımı oluşturur |
| `GET` | `/api/sensors` | Kayıtlı tüm sensörleri listeler |
| `GET` | `/api/sensors/{id}` | Belirli bir sensörün detayını getirir |
| `PUT` | `/api/sensors/{id}` | Sensör bilgisini günceller |
| `DELETE` | `/api/sensors/{id}` | Sensörü siler |
| `POST` | `/api/telemetry` | Yeni telemetri okuması kaydeder (ingestion endpoint) |
| `GET` | `/api/telemetry/latest` | Sensör başına en güncel okumayı döner |
| `GET` | `/api/telemetry/statistics` | Belirtilen `startDate`–`endDate` aralığında min/max/avg istatistiği hesaplar |

---

## Hızlı Başlangıç (Quickstart)

Tek komutla tüm servisleri (veritabanı, API, worker, Grafana) ayağa kaldırın:

```bash
docker compose up -d
```

Migration'lar API başlangıcında otomatik uygulanır; ek bir manuel adım gerekmez.

### Erişim Bilgileri

| Servis | Adres | Varsayılan Kimlik Bilgisi |
|---|---|---|
| **API (Swagger)** | `http://localhost:5000/swagger` | — |
| **Grafana** | `http://localhost:3000` | `admin` / `admin` |
| **PostgreSQL** | `localhost:5432` | `postgres` / `postgres` (bkz. `docker-compose.yml`) |

### Konteyner Servisleri

| Container | Rol |
|---|---|
| `iot-postgres` | Veri kalıcılığı |
| `iot-api` | REST API |
| `iot-worker` | Sürekli telemetri üretimi |
| `iot-grafana` | Dashboard & alerting |

---

## İzleme & Alarm Senaryosu

Grafana Unified Alerting üzerinde tanımlı kural: **sıcaklık okuması 40°C eşiğini aştığında** kural `Pending` durumundan `Firing` durumuna geçer.

**Alarmı manuel tetiklemek için:**

```bash
curl -X POST http://localhost:5000/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "sensorId": 1,
    "temperature": 45.5,
    "humidity": 40,
    "timestamp": "2026-08-17T12:00:00Z"
  }'
```

**Doğrulama adımları:**
1. `http://localhost:3000` → **Alerting → Alert rules** yolunu açın.
2. İlgili kuralın durumunun `Firing` olarak işaretlendiğini gözlemleyin.
3. Dashboard panelinde ilgili sensörün zaman serisinde eşik çizgisinin (40°C) aşıldığı anı görsel olarak doğrulayın.

Worker Service'in ürettiği random walk verisi doğal olarak bu eşiği zaman zaman aşabileceğinden, alarmın gerçek koşullarda da (manuel müdahale olmadan) tetiklendiği gözlemlenebilir.
