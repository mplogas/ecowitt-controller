# HTTP API

## Inbound — Weather Station Data

The controller exposes an HTTP endpoint that Ecowitt gateways post weather data to.

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

All imperial values are automatically converted to metric when `controller.unit` is set to `"metric"` (default).

**Response:** `200 OK`, `400 Bad Request`, or `500 Internal Server Error`

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

**Quick run command (not yet active):**
```json
{
  "command": [
    {
      "cmd": "quick_run",
      "id": 12345,
      "model": 1,
      "val": 20,
      "val_type": 1,
      "on_type": 0,
      "off_type": 0,
      "always_on": 1,
      "on_time": 0,
      "off_time": 0
    }
  ]
}
```

**Quick stop command (not yet active):**
```json
{
  "command": [
    { "cmd": "quick_stop", "id": 12345, "model": 1 }
  ]
}
```

> Note: Subdevice command sending is currently commented out in the Dispatcher. The infrastructure is in place but not wired up.