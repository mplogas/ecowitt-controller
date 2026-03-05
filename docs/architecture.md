# Architecture

Ecowitt Controller runs three background services connected by an in-memory message bus (SlimMessageBus). Weather data arrives via HTTP from Ecowitt gateways, subdevice state is polled from gateway APIs, and everything is published over MQTT with optional Home Assistant discovery.

## Data Flow

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
```

### Dispatcher to HttpPublishingService

Subdevice polling configuration.

```mermaid
flowchart LR
    DISP[Dispatcher] --> CHTTP[config-http] --> HTTP[HttpPublishingService]
```

### HttpPublishingService and DataController to Dispatcher

Inbound weather data and polled subdevice data.

```mermaid
flowchart LR
    DC[DataController] --> GAD[gw-api-data] --> DISP[Dispatcher]
    HTTP[HttpPublishingService] --> SAG[subdevice-api-data] --> DISP
    HTTP --> HSE[http-service-event] --> DISP
```

## Service Responsibilities

### DataController
ASP.NET endpoint at `POST /data/report`. Receives form-encoded weather data from Ecowitt gateways and publishes `GatewayApiData` onto the bus.

### Dispatcher
Central orchestrator. Consumes messages from both HTTP and MQTT services, manages the `DeviceStore` (thread-safe in-memory state), performs change detection via `DeNoiserHelper`, and emits data and discovery messages. Split across partial classes for HTTP consumers, MQTT consumers, and orchestration logic.

### MqttService
Connects to the MQTT broker, publishes sensor data and Home Assistant discovery payloads, subscribes to HA status topic for re-emission on HA restart, and handles subdevice commands from HA switches. Split across partial classes for consumer, publisher, discovery, and events.

### HttpPublishingService
Polls Ecowitt gateway HTTP APIs (`get_iot_device_list`, `parse_quick_cmd_iot`) for subdevice data on a configurable interval. Also sends subdevice commands (start/stop) back to gateways.

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
