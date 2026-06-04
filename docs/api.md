# HTTP API

The controller supports two ingestion modes per gateway for weather data:

- **Push** (`/data/report` — see [Inbound](#inbound--weather-station-data-push-mode) below) — gateway-initiated, fixed imperial source, controller converts to user-preferred display units. Default. Works with any Ecowitt-protocol station.
- **Poll** ([`get_livedata_info`](#outbound--gateway-livedata-polling-poll-mode)) — controller-initiated on `liveDataInterval`. Gateway returns values in its WSView display unit; controller passes the unit through to HA verbatim. Requires GW1200/GW2000/GW3000.

Mode is selected per gateway via the `ingestMode` config field. Push payloads from `Poll`-mode gateway IPs are silently discarded (200 OK is still returned to avoid retry storms).

## Inbound — Weather Station Data (Push mode)

The controller exposes an HTTP endpoint that Ecowitt gateways post weather data to. This is the legacy/default ingestion path.

### `GET /data/report`

Health check / connectivity test.

**Response:** `200 OK`
```json
{ "status": "ok" }
```

### `POST /data/report`

Receives weather station data from Ecowitt gateways.

**Content-Type:** `application/x-www-form-urlencoded`

The gateway sends all sensor readings as form fields. The controller captures the sender's IP address automatically and serializes the form key/value pairs into a JSON payload for internal processing.

**Known form fields (non-exhaustive):**
| Field | Description |
|-------|-------------|
| `PASSKEY` | Gateway passkey identifier |
| `stationtype` | Station type string (e.g. `GW2000A_V3.1.3`) |
| `runtime` | Gateway uptime in seconds |
| `dateutc` | Gateway timestamp |
| `freq` | Radio frequency |
| `model` | Gateway model (e.g. `GW2000A`) |
| `tempinf` | Indoor temperature (°F) |
| `tempf` | Outdoor temperature (°F) |
| `humidity` | Outdoor humidity (%) |
| `humidityin` | Indoor humidity (%) |
| `baromrelin` | Relative barometric pressure (inHg) |
| `baromabsin` | Absolute barometric pressure (inHg) |
| `winddir` | Wind direction (°) |
| `windspeedmph` | Wind speed (mph) |
| `windgustmph` | Wind gust (mph) |
| `maxdailygust` | Max daily gust (mph) |
| `solarradiation` | Solar radiation (W/m²) |
| `uv` | UV index |
| `rrain_piezo` | Rain rate piezo (in/hr) |
| `drain_piezo` | Daily rain piezo (in) |
| `wrain_piezo` | Weekly rain piezo (in) |
| `mrain_piezo` | Monthly rain piezo (in) |
| `yrain_piezo` | Yearly rain piezo (in) |
| `soilmoisture1`–`8` | Soil moisture channels (%) |
| `soilad1`–`8` | Soil admittance channels (mS) |
| `lightning_num` | Lightning strike count |
| `lightning` | Lightning distance |
| `lightning_time` | Last lightning strike time |
| `tempf1`–`8` | Extra temperature channels (°F) |
| `humidity1`–`8` | Extra humidity channels (%) |
| `tf_co2` | CO2 sensor temperature (°F) |
| `humi_co2` | CO2 sensor humidity (%) |
| `pm25_co2` | CO2 sensor PM2.5 (µg/m³) |
| `co2` | CO2 concentration (ppm) |
| `wh65batt`, `wh80batt`, `wh90batt`, `wh57batt`, `co2_batt`, etc. | Battery states |
| `ws90cap_volt` | WS90 capacitor voltage |

All imperial values are automatically converted to metric when `controller.unit` is set to `"metric"` (default). This conversion is **Push-mode specific** — Poll mode uses unit passthrough (see below).

**Response:** `200 OK`, `400 Bad Request`, or `500 Internal Server Error`

## Outbound — Gateway Livedata Polling (Poll mode)

When a gateway is configured with `ingestMode: Poll`, the controller polls its local `get_livedata_info` API on `ecowitt.liveDataInterval` (default 5 seconds). Only available on GW1200/GW2000/GW3000.

### `GET http://<gateway-ip>/get_livedata_info`

Retrieves all current sensor readings from the gateway.

**Response structure (top-level keys depend on connected hardware):**
```jsonc
{
  "common_list": [          // outdoor weather aggregate, ID-keyed
    { "id": "0x02", "val": "12.9", "unit": "C" },     // outdoor temp
    { "id": "0x0B", "val": "0.6 m/s" },               // wind speed
    { "id": "0x19", "val": "21.60 km/h" }             // max gust (unit varies by gateway setting)
  ],
  "piezoRain": [            // WS90 rain bucket; WS90 battery on last array element
    { "id": "0x0E", "val": "0.0 mm/Hr" },
    { "id": "0x13", "val": "120.3 mm", "battery": "3", "voltage": "2.72", "ws90cap_volt": "2.3" }
  ],
  "wh25": [                 // indoor T/H/baro
    { "intemp": "23.5", "unit": "C", "inhumi": "49%", "abs": "1004.0 hPa", "rel": "1004.0 hPa" }
  ],
  "lightning": [            // WH57
    { "distance": "8 km", "count": "0", "battery": "5", "date": "...", "timestamp": "..." }
  ],
  "co2": [                  // AQIN: PM/CO2/T/H/battery
    { "temp": "23.9", "unit": "C", "humidity": "52%", "PM25": "4.0", "CO2": "974", "battery": "6" }
  ],
  "ch_soil": [              // WH51 channels 1-16
    { "channel": "1", "name": "CH1 Soil", "battery": "5", "voltage": "1.60", "humidity": "54%" }
  ],
  "ch_temp": [              // WN30/WN36 channels
    { "channel": "1", "temp": "14.0", "unit": "C", "battery": "5", "voltage": "1.46" }
  ],
  "debug": [ /* gateway internals — not mapped to sensors */ ]
}
```

**Value format and unit passthrough:** Values come either as `"<number> <unit>"` (e.g. `"21.60 km/h"`, `"22.3 mm"`, `"8 km"`) or as a bare number with the unit in a sibling field (e.g. `wh25.unit = "C"`). The unit suffix reflects whatever the user has configured in WSView for that quantity — Ecowitt's API respects the gateway display setting. The controller parses both forms and passes the unit string straight through to HA's `unit_of_measurement`; no source-side conversion is performed. A small `CanonicalizeUnit` map handles the gateway unit strings that HA's `device_class` validation rejects: `"C"`/`"F"` → `"°C"`/`"°F"` (temperature), `"W/m2"` → `"W/m²"` (irradiance), and `"mm/Hr"`/`"in/Hr"` → `"mm/h"`/`"in/h"` (precipitation_intensity). Everything else passes through verbatim.

**Sensor ID mapping** (`common_list`):

| Livedata ID | Sensor (HA property name) | Source unit (gateway-dependent) |
|---|---|---|
| `0x02` | `tempf` Outdoor Temperature | `°C` / `°F` |
| `0x03` | `dewpoint` Dew Point | `°C` / `°F` |
| `0x07` | `humidity` Outdoor Humidity | `%` |
| `0x0A` | `winddir` Wind Direction | `°` |
| `0x0B` | `windspeedmph` Wind Speed | `m/s` / `km/h` / `mph` |
| `0x0C` | `windgustmph` Wind Gust | `m/s` / `km/h` / `mph` |
| `0x19` | `maxdailygust` Max Daily Gust | `m/s` / `km/h` / `mph` |
| `0x15` | `solarradiation` Solar Radiation | `W/m²` |
| `0x17` | `uv` UV Index | (dimensionless) |
| `0x6D` | `windrun` Wind Run | (unitless in API; no HA device_class assigned) |
| `4` | `feelslike` Feels Like | `°C` / `°F` |
| `5` | `vpd` Vapor Pressure Deficit | `kPa` |

Unknown IDs are logged at debug level and skipped — future firmware additions don't break ingestion.

**Response:** `200 OK` with JSON body. `404` / empty body / parse failure are logged as warnings and the cycle skips silently.

## Outbound — Gateway Subdevice Polling

The controller polls Ecowitt gateways via their local HTTP API to retrieve subdevice data. This runs as a background service on the configured `ecowitt.pollingInterval`.

### `GET http://<gateway-ip>/get_iot_device_list`

Retrieves the list of connected IoT subdevices (AC1100, WFC01, WFC02).

**Response structure:**
```json
{
  "command": [
    {
      "id": 12345,
      "model": 1,
      "ver": 15,
      "rfnet_state": 1,
      "battery": 4,
      "signal": 3
    }
  ]
}
```

| Field | Description |
|-------|-------------|
| `id` | Unique subdevice identifier |
| `model` | Subdevice model: `1` = WFC01, `2` = AC1100, `3` = WFC02 |
| `ver` | Firmware version |
| `rfnet_state` | RF network state (`1` = available) |
| `battery` | Battery level |
| `signal` | Signal strength |

### `POST http://<gateway-ip>/parse_quick_cmd_iot`

Reads detailed subdevice data or sends commands.

**Read device request:**
```json
{
  "command": [
    { "cmd": "read_device", "id": 12345, "model": 1 }
  ]
}
```

**Response structure (varies by model):**
```json
{
  "command": [
    {
      "devicename": "WFC01",
      "nickname": "Garden Valve",
      "water_status": "0",
      "water_running": "0",
      "water_total": "1234",
      "flow_velocity": "0",
      "water_temp": "65.3",
      "wfc01batt": "4",
      "gw_rssi": "-45",
      ...
    }
  ]
}
```

Sensor properties are mapped through `SensorBuilder` (see `Model/Mapping/SensorBuilder.cs`) and vary per subdevice model.

**Quick run command:**
```json
{
  "command": [
    {
      "cmd": "quick_run",
      "id": 12345,
      "model": 1,
      "val": 20,
      "val_type": 1,
      "position": 100,
      "always_on": 1
    }
  ]
}
```

`val_type`: `0` = Seconds, `1` = Minutes, `2` = Hours, `3` = Liters. `always_on = 1` makes the device run continuously until stopped (HA bare-ON without duration uses this default).

**Quick stop command:**
```json
{
  "command": [
    { "cmd": "quick_stop", "id": 12345, "model": 1 }
  ]
}
```

Commands originate from MQTT (HA switches publish to `<base>/<gw>/subdevices/<id>/cmd/homeassistant` with `ON`/`OFF`, or direct JSON to `<base>/<gw>/subdevices/<id>/cmd`). Dispatcher resolves the gateway and publishes `SubdeviceCommandDispatch` on the bus; `HttpPublishingService` consumes it and issues the HTTP call. All HTTP traffic to a gateway — subdevice polling, livedata polling, command dispatch — serializes through a per-gateway `SemaphoreSlim` to avoid concurrent requests against the same device.