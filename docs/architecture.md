# Architecture

Ecowitt Controller runs three background services connected by an in-memory message bus (SlimMessageBus). Weather data arrives via HTTP from Ecowitt gateways (either push or poll, configurable per gateway), subdevice state and commands flow through a gateway HTTP API, and everything is published over MQTT with optional Home Assistant discovery.

## Ingestion Modes

Each gateway is configured (via `ecowitt.gateways[].ingestMode`) as either:

- **`Push`** (default) — gateway initiates uploads via `POST /data/report`. Imperial source units; controller converts to `controller.unit`.
- **`Poll`** — controller polls `get_livedata_info` on `ecowitt.liveDataInterval`. Whatever unit the gateway is configured to display in WSView is the unit that lands in HA (passthrough). Only available on GW1200/GW2000/GW3000.

Both paths terminate at `Dispatcher.OnHandle(GatewayApiData)` and `Dispatcher.OnHandle(GatewayLiveData)` respectively, both produce `Device` objects via their own mapper extension, both feed the same `DeviceStore` and downstream MQTT publish pipeline. The two paths coexist — a deployment can have one gateway in Push mode and another in Poll mode.

## Data Flow

### Push ingestion

```mermaid
flowchart LR
    GW[Ecowitt Gateway] -->|HTTP POST /data/report| DC[DataController]
    DC -->|GatewayApiData| BUS[(Message Bus)]
    BUS --> DISP[Dispatcher]
    DISP -->|DeviceData / DeviceDataFull| BUS
    BUS --> MQTT[MqttService]
    MQTT -->|Publish| BROKER[MQTT Broker]
    BROKER --> HA[Home Assistant]
```

### Poll ingestion (livedata)

```mermaid
flowchart LR
    DISP[Dispatcher] -->|HttpConfig| BUS[(Message Bus)]
    BUS --> HTTP[HttpPublishingService]
    HTTP -->|GET /get_livedata_info| GW[Ecowitt Gateway]
    GW -->|JSON response| HTTP
    HTTP -->|GatewayLiveData| BUS
    BUS --> DISP
    DISP -->|DeviceData / DeviceDataFull| BUS
    BUS --> MQTT[MqttService]
```

## Subdevice Polling

```mermaid
flowchart LR
    DISP[Dispatcher] -->|HttpConfig| BUS[(Message Bus)]
    BUS --> HTTP[HttpPublishingService]
    HTTP -->|HTTP API| GW[Ecowitt Gateway]
    GW -->|Subdevice data| HTTP
    HTTP -->|SubdeviceApiAggregate| BUS
    BUS --> DISP
    DISP -->|SubdeviceData / SubdeviceDataFull| BUS
    BUS --> MQTT[MqttService]
```

## Subdevice Command Dispatch

Commands originate from MQTT (HA switches or direct JSON) and flow back to the gateway over the same `HttpPublishingService` HTTP path. Dispatcher resolves the gateway and publishes `SubdeviceCommandDispatch` rather than calling the HTTP service directly — every cross-service interaction in the system goes through the bus.

```mermaid
flowchart LR
    HA[Home Assistant] -->|MQTT cmd| BROKER[MQTT Broker]
    BROKER --> MQTT[MqttService]
    MQTT -->|SubdeviceApiCommand| BUS[(Message Bus)]
    BUS --> DISP[Dispatcher]
    DISP -->|SubdeviceCommandDispatch| BUS
    BUS --> HTTP[HttpPublishingService]
    HTTP -->|POST /parse_quick_cmd_iot| GW[Ecowitt Gateway]
```

### Run modes (duration / volume)

The valve/plug run-mode entities (`select` + `number` + `button`) feed a separate
staging path. `MqttService` translates each entity change into a `SubdeviceRunConfig`
message; `Dispatcher` merges it into the subdevice's `StagedRunConfig` (held in
`DeviceStore`) and, on the Start button, validates mode applicability + value
bounds, assembles a `SubdeviceApiCommand`, and routes it through the **same shared
guard** as the direct path before dispatch. Capability gating (Duration universal;
Volume only for flow-capable valves) and the unit→gateway conversion
(minutes→seconds `val_type:0`, liters→deciliters `val_type:3`) are enforced
controller-side so the device self-closes at the target — the direct-MQTT path
cannot bypass them.

```mermaid
flowchart LR
    HA[HA select / number / button] -->|MQTT cmd/#| MQTT[MqttService]
    MQTT -->|SubdeviceRunConfig| BUS[(Message Bus)]
    BUS --> DISP[Dispatcher]
    DISP -->|merge -> DeviceStore staged config| DISP
    DISP -->|on Start: guard + SubdeviceCommandDispatch| BUS
    BUS --> HTTP[HttpPublishingService]
```

## Per-gateway Concurrency

All HTTP traffic to a given gateway — subdevice polling, livedata polling (when in Poll mode), and command dispatch — runs through `HttpPublishingService` and serializes through a `ConcurrentDictionary<string, SemaphoreSlim>` keyed by gateway IP. At most one HTTP request per gateway is in flight at any moment. Different gateways still get full parallelism. A separate 350ms `Task.Delay` between subdevice payload calls handles rate-limiting (a separate contract from concurrency).

## Message Bus Topology

### Dispatcher to MqttService

Configuration, device data, and discovery messages.

```mermaid
flowchart LR
    DISP[Dispatcher] --> CMQTT[config-mqtt] --> MQTT[MqttService]
    DISP --> DISC[home-assistant-discovery] --> MQTT
    DISP --> DREM[discovery-removal] --> MQTT
    DISP --> DD[device-data] --> MQTT
    DISP --> DDF[device-data-full] --> MQTT
    DISP --> SD[subdevice-data] --> MQTT
    DISP --> SDF[subdevice-data-full] --> MQTT
```

### MqttService to Dispatcher

Service lifecycle events, MQTT connection state, HA status, and subdevice commands received via MQTT.

```mermaid
flowchart LR
    MQTT[MqttService] --> MSE[mqtt-service-event] --> DISP[Dispatcher]
    MQTT --> MCE[mqtt-connection-event] --> DISP
    MQTT --> HAS[home-assistant-status] --> DISP
    MQTT --> SAC[subdevice-api-command] --> DISP
    MQTT --> SRC[subdevice-run-config] --> DISP
```

`subdevice-api-command` carries the HA switch (ON/OFF) and direct-MQTT commands.
`subdevice-run-config` carries the run-mode entities (select/number/button) as
partial staged-config updates plus a Start flag.

### Dispatcher to HttpPublishingService

HTTP polling configuration (subdevice + livedata hosts and intervals) and subdevice command dispatch.

```mermaid
flowchart LR
    DISP[Dispatcher] --> CHTTP[config-http] --> HTTP[HttpPublishingService]
    DISP --> SCD[subdevice-command-dispatch] --> HTTP
```

### HttpPublishingService and DataController to Dispatcher

Inbound weather data (push), polled livedata, polled subdevice data, and service lifecycle.

```mermaid
flowchart LR
    DC[DataController] --> GAD[gw-api-data] --> DISP[Dispatcher]
    HTTP[HttpPublishingService] --> GLD[gw-livedata] --> DISP
    HTTP --> SAG[subdevice-api-data] --> DISP
    HTTP --> HSE[http-service-event] --> DISP
```

## Service Responsibilities

### DataController
ASP.NET endpoint at `POST /data/report`. Receives form-encoded weather data from Push-mode gateways and publishes `GatewayApiData` onto the bus. For Poll-mode gateways, the request is accepted (200 OK) but the payload is dropped by Dispatcher at the bus boundary — no DataController logic needs to know about IngestMode.

### Dispatcher
Central orchestrator. Consumes messages from both HTTP and MQTT services, manages the `DeviceStore` (thread-safe in-memory state), performs change detection via `DeNoiserHelper`, and emits data and discovery messages. For Poll-mode gateways it also drops any incoming `GatewayApiData` push payload (the IngestMode filter lives here, not in DataController). Owns run-mode policy: stages `SubdeviceRunConfig` updates onto the subdevice, validates mode applicability + value bounds in a shared guard (so the direct-MQTT path can't bypass the HA-side constraints), and fires the assembled command on Start. Split across partial classes for HTTP consumers, MQTT consumers, and orchestration logic.

### MqttService
Connects to the MQTT broker, publishes sensor data and Home Assistant discovery payloads (including the run-mode `select`/`number`/`button` entities for controllable subdevices), subscribes to HA status topic for re-emission on HA restart, and forwards subdevice commands onto the bus — HA switch / direct JSON as `SubdeviceApiCommand`, and run-mode entity changes as `SubdeviceRunConfig`. A single `<base>/+/subdevices/+/cmd/#` subscription covers all subdevice command topics. Split across partial classes for consumer, publisher, discovery, and events.

### HttpPublishingService
Owns all outbound HTTP traffic to gateways. Runs two `PeriodicTimer` loops in parallel — one for subdevice polling (`get_iot_device_list` + `parse_quick_cmd_iot`), one for livedata polling (`get_livedata_info`) on Poll-mode gateways — plus a `SubdeviceCommandDispatch` bus consumer that issues start/stop commands. All three call sites acquire a per-gateway `SemaphoreSlim` to serialize requests to the same device.

## Domain Model

```mermaid
classDiagram
    class Device {
        +string IpAddress
        +string Name
        +DateTime TimestampUtc
        +List~Subdevice~ Subdevices
        +List~ISensor~ Sensors
    }

    class Subdevice {
        +int Id
        +string Nickname
        +SubdeviceModel Model
        +bool Availability
        +string GwIp
        +List~ISensor~ Sensors
    }

    class ISensor {
        <<interface>>
        +string Name
        +string Alias
        +object Value
        +SensorType SensorType
        +SensorCategory SensorCategory
    }

    Device "1" --> "*" Subdevice
    Device "1" --> "*" ISensor : Sensors
    Subdevice "1" --> "*" ISensor : Sensors
```
