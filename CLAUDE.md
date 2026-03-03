# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ecowitt Controller is a .NET 8 / ASP.NET Core application that bridges Ecowitt weather stations and IoT subdevices (AC1100 smart plugs, WFC01/WFC02 water valves) to MQTT, with Home Assistant auto-discovery support. It receives weather data via HTTP POST from Ecowitt gateways and polls subdevices via their HTTP API, then publishes everything over MQTT.

## Build & Run Commands

All commands run from `src/`:

```bash
# Build
dotnet build Ecowitt.Controller.sln

# Run
dotnet run --project Ecowitt.Controller/Ecowitt.Controller.csproj

# Run release
dotnet run -c Release --project Ecowitt.Controller/Ecowitt.Controller.csproj

# Run tests (NUnit)
dotnet test EcoWitt.Controller.Tests/EcoWitt.Controller.Tests.csproj

# Run a single test
dotnet test EcoWitt.Controller.Tests/EcoWitt.Controller.Tests.csproj --filter "FullyQualifiedName~TestMethodName"

# Docker build (from src/)
docker build -t ecowitt-controller .
```

Note: the test project casing is `EcoWitt.Controller.Tests` (capital W) while the main project is `Ecowitt.Controller`.

## Architecture

The system uses three BackgroundServices communicating through an **in-memory message bus** (SlimMessageBus):

### Data Flow

1. **DataController** (`Controller/DataController.cs`) — ASP.NET endpoint at `POST /data/report` receives form-encoded weather data from Ecowitt gateways, publishes `GatewayApiData` onto the bus.

2. **Dispatcher** (`Service/Orchestrator/Dispatcher*.cs`) — Central orchestrator (partial class split across files). Consumes messages from both HTTP and MQTT services, manages the `DeviceStore`, performs change detection via `DeNoiserHelper`, and emits data/discovery messages. The `ConsumerHttp` partial handles gateway and subdevice API data; the `ConsumerMqtt` partial handles MQTT lifecycle and Home Assistant status events.

3. **MqttService** (`Service/Mqtt/MqttService*.cs`) — Partial class split across files for consumer, publisher, discovery, and events. Connects to MQTT broker, publishes sensor data and Home Assistant discovery payloads, subscribes to HA status topic for re-emission on HA restart.

4. **HttpPublishingService** (`Service/Http/HttpPublishingService*.cs`) — Polls Ecowitt gateway HTTP APIs for subdevice data on a configurable interval, publishes `SubdeviceApiAggregate` onto the bus.

### Message Bus Topics (SlimMessageBus)

Messages are defined in `Model/Message/` (Config, Data, Event subdirs). The bus wiring is in `Program.cs`. Key flows:
- Dispatcher → MqttService: `MqttConfig`, `DeviceData`, `DeviceDataFull`, `SubdeviceData`, `SubdeviceDataFull`, `HomeAssistantDiscoveryEvent`
- MqttService → Dispatcher: `MqttServiceEvent`, `MqttConnectionEvent`, `HomeAssistantStatusEvent`
- Dispatcher → HttpPublishingService: `HttpConfig`
- HttpPublishingService → Dispatcher: `SubdeviceApiAggregate`, `HttpServiceEvent`

### Key Model Layer

- **Device/Subdevice/Sensor** (`Model/Device.cs`, `Model/Subdevice.cs`, `Model/Sensor.cs`) — Domain model. `ISensor` has typed value accessors and change tracking via hash comparison.
- **SensorBuilder** (`Model/Mapping/SensorBuilder*.cs`) — Partial class that maps raw Ecowitt property names (e.g., `tempinf`, `baromrelin`) to typed `Sensor` objects with unit conversion (imperial→metric). This is the main mapping to extend when adding new sensor types.
- **ApiDataExtension** (`Model/Mapping/ApiDataExtension.cs`) — Extension methods that convert API DTOs to domain objects.
- **DiscoveryBuilder** (`Model/Discovery/`) — Builds Home Assistant MQTT discovery payloads.
- **DeNoiserHelper** (`Service/Orchestrator/DeNoiser.cs`) — Filters insignificant sensor value changes using configurable tolerances per sensor type.
- **DeviceStore** (`Service/Orchestrator/DeviceStore.cs`) — Thread-safe in-memory store (`ConcurrentDictionary`) for gateway/subdevice state.

### Configuration

Three option classes bound from `appsettings.json` sections in `Model/Configuration/`:
- `ecowitt` → `EcowittOptions` (gateways, polling interval)
- `mqtt` → `MqttOptions` (broker connection)
- `controller` → `ControllerOptions` (units, precision, publishing interval, HA discovery toggle)

Config is loaded from `/config/appsettings.json` (Docker) or the content root (bare-metal).

## Conventions

- Partial classes are used extensively for service separation (consumers, publishers, events in separate files).
- Logging uses Serilog with structured logging templates (not string interpolation).
- HTTP retry policy uses Polly with decorrelated jitter backoff.
- The Ecowitt gateway HTTP API endpoints used: `get_iot_device_list`, `parse_quick_cmd_iot`.
- SensorType/SensorState/SensorCategory enums align with Home Assistant device classes.