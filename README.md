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
    "reconnectAttempts": 2,
    "useMqtt311": false
  },
  "ecowitt": {
    "pollingInterval": 30,
    "calculateValues": true,
    "retries": 2,
    "gateways": [
      {
        "name": "weatherstation_01",
        "ip": "192.168.1.101",
        "subdevices": true
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
| `mqtt` | `useMqtt311` | `false` | Use MQTT 3.1.1 instead of MQTT 5.0 |
| `ecowitt` | `pollingInterval` | `30` | Subdevice polling interval (seconds) |
| `ecowitt` | `liveDataInterval` | `5` | Gateway livedata polling interval (seconds), used when a gateway is in `Poll` mode |
| `ecowitt` | `calculateValues` | `true` | Generate calculated sensor values |
| `ecowitt` | `gateways` | `[]` | Gateway definitions (name, ip, credentials, subdevices) |
| `ecowitt.gateways[]` | `subdevices` | `false` | Enable subdevice polling (only GW1200, GW2000, GW3000) |
| `ecowitt.gateways[]` | `ingestMode` | `Push` | `Push` (accept legacy `/data/report` HTTP push, default) or `Poll` (poll `get_livedata_info` on `liveDataInterval`; only GW1200/2000/3000). When `Poll`, push payloads from this gateway IP are dropped. |
| `controller` | `precision` | `2` | Decimal places for floating-point values |
| `controller` | `unit` | `metric` | `metric` or `imperial` |
| `controller` | `homeassistantdiscovery` | `true` | Publish HA MQTT discovery messages |

### Run with Docker

Multi-architecture images (amd64, arm64) are available on [Docker Hub](https://hub.docker.com/r/mplogas/ecowitt-controller/tags). The container expects its configuration at `/config/appsettings.json`.

```bash
docker run -d --name ecowitt-controller \
  -v /path/to/config:/config \
  -p 8080:8080 \
  --restart always \
  mplogas/ecowitt-controller:latest
```

Place your `appsettings.json` in the host directory you bind-mount to `/config`.

#### Docker Compose

```yaml
services:
  ecowitt-controller:
    image: mplogas/ecowitt-controller:latest
    container_name: ecowitt-controller
    restart: always
    volumes:
      - type: bind
        source: /path/to/config
        target: /config
    ports:
      - "8080:8080"
```

Port `8080` must be reachable by your Ecowitt gateway for weather data uploads. If your gateway is on a specific VLAN or subnet, bind to the appropriate interface IP (e.g. `192.168.1.10:8080:8080`).

### Home Assistant Add-on

For Home Assistant OS / Supervised installations, Ecowitt Controller is available as a native add-on. This handles configuration, networking, and MQTT broker discovery automatically.

1. Add the add-on repository to your HA instance: `https://github.com/mplogas/ha-addon-ecowitt-controller`
2. Install **Ecowitt Controller** from the add-on store
3. Configure your gateways in the add-on settings
4. Start the add-on

The add-on runs on the host network and auto-discovers the Mosquitto broker if the official Mosquitto add-on is installed. See the [add-on repository](https://github.com/mplogas/ha-addon-ecowitt-controller) for full documentation.

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

### Home Assistant Integration

With `homeassistantdiscovery` enabled (default), devices and sensors appear automatically in HA via MQTT discovery. Make sure your HA instance is connected to the same MQTT broker.

> **Tip:** Running Home Assistant OS or Supervised? Use the [HA add-on](#home-assistant-add-on) instead for a simpler setup.

> **Note:** Gateways and their sensors only appear after the first data push from the Ecowitt device. This depends on the **Upload Interval** configured in your gateway's WebUI or the WS View Plus app (step 4 above). Subdevices are picked up on the next polling cycle after their parent gateway has reported in, so expect an additional delay of up to one `pollingInterval`.

## Documentation

- [Architecture](docs/architecture.md) — System design, data flow, and message bus topology
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