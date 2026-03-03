# Ecowitt Controller

A .NET 10 bridge that connects Ecowitt weather stations and IoT subdevices to MQTT, with native Home Assistant auto-discovery.

## Features

- Multi-gateway, multi-subdevice support
- Automatic discovery of new sensors and subdevices
- Bidirectional communication with subdevices (AC1100, WFC01, WFC02)
- Home Assistant MQTT discovery (devices, sensors, switches)
- Metric/imperial unit conversion
- Change-detection filtering to reduce MQTT noise

## Supported Devices

**Gateways:** GW3000, GW2000, GW1200, GW1100, WN1980, WS3800, WS39x0

**Subdevices** (require IoT-capable gateway like GW2000/GW1200):
- **AC1100** — Smart plug with power monitoring
- **WFC01** — Water timer with flow sensor and temperature
- **WFC02** — Water valve with optional flow sensor

**Weather Stations:** Any Fine Offset compatible station (Ecowitt, Froggit, Ambient Weather, ...) that supports custom Ecowitt protocol uploads.

## Getting Started

**Prerequisites:** A running MQTT broker (e.g. Mosquitto).

### Configuration

Create an `appsettings.json`. Minimal setup — just point it at your MQTT broker:

```json
{
  "mqtt": {
    "host": "192.168.1.50"
  }
}
```

Full configuration with all options and their defaults:

```json
{
  "Serilog": {
    "MinimumLevel": "Warning"
  },
  "mqtt": {
    "host": "",
    "user": "",
    "password": "",
    "port": 1883,
    "basetopic": "ecowitt",
    "clientId": "ecowitt-controller",
    "reconnect": true,
    "reconnectAttempts": 2
  },
  "ecowitt": {
    "pollingInterval": 30,
    "autodiscovery": true,
    "calculateValues": true,
    "retries": 2,
    "gateways": [
      {
        "name": "weatherstation_01",
        "ip": "192.168.1.101"
      }
    ]
  },
  "controller": {
    "precision": 2,
    "unit": "metric",
    "homeassistantdiscovery": true
  }
}
```

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `mqtt` | `host` | — | MQTT broker address (required) |
| `mqtt` | `port` | `1883` | MQTT broker port |
| `mqtt` | `basetopic` | `ecowitt` | Root MQTT topic prefix |
| `mqtt` | `reconnect` | `true` | Auto-reconnect on disconnect |
| `mqtt` | `reconnectAttempts` | `2` | Reconnect retry count |
| `ecowitt` | `pollingInterval` | `30` | Subdevice polling interval (seconds) |
| `ecowitt` | `autodiscovery` | `false` | Auto-discover gateways from incoming data |
| `ecowitt` | `calculateValues` | `true` | Generate calculated sensor values |
| `ecowitt` | `gateways` | `[]` | Manual gateway definitions (name, ip, credentials) |
| `controller` | `precision` | `2` | Decimal places for floating-point values |
| `controller` | `unit` | `metric` | `metric` or `imperial` |
| `controller` | `homeassistantdiscovery` | `true` | Publish HA MQTT discovery messages |

### Run with Docker

```bash
docker run -d --name ecowitt-controller \
  -v /path/to/appsettings.json:/config/appsettings.json:ro \
  -p 8080:8080 \
  mplogas/ecowitt-controller:latest
```

### Run from Source

```bash
cd src
dotnet run --project Ecowitt.Controller/Ecowitt.Controller.csproj -c Release
```

### Configure Your Weather Station

1. Open your gateway's WebUI or the WS View Plus app
2. Go to weather services and enable the **Customized** upload
3. Set protocol to **Ecowitt**, enter the controller's IP, path `/data/report`, port `8080`
4. Set the posting interval (e.g. 30 seconds)

### Home Assistant

With `homeassistantdiscovery` enabled (default), devices and sensors appear automatically in HA via MQTT discovery. Make sure your HA instance is connected to the same MQTT broker.

## Documentation

- [HTTP API](docs/api.md) — Inbound weather data endpoint and outbound gateway polling
- [MQTT Topics](docs/mqtt.md) — Topic structure, payloads, and Home Assistant discovery

## Tech Stack

- **ASP.NET Core Web API** — HTTP endpoint for Ecowitt weather data
- **[MQTTnet](https://github.com/dotnet/MQTTnet)** — MQTT client
- **[SlimMessageBus](https://github.com/zarusz/SlimMessageBus)** — In-memory message bus connecting the services
- **[Serilog](https://serilog.net/)** — Structured logging
- **[Polly](https://github.com/App-vNext/Polly)** — HTTP retry policies

## Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT — see [LICENSE](LICENSE).