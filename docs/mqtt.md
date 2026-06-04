# MQTT Topics

All topics are prefixed with the configured base topic (default: `ecowitt`). Names are sanitized to lowercase with spaces replaced by hyphens.

## Published Topics

### Heartbeat
- **`<base>/heartbeat`** — Published every 30 seconds while MQTT is connected.
  ```json
  { "service": "2024-01-15T12:00:00Z" }
  ```

### Gateway
- **`<base>/<gw_name>`** — Gateway info, published on first discovery and full updates.
  ```json
  {
    "ip": "192.168.1.101",
    "name": "weatherstation-01",
    "model": "GW2000A",
    "passkey": "...",
    "stationType": "GW2000A_V3.1.3",
    "runtime": 123456,
    "state": "online",
    "freq": "868M"
  }
  ```

- **`<base>/<gw_name>/availability`** — Gateway availability. Payload is plain text `online` or `offline` (offline if no data received for 5 minutes).

### Gateway Sensors
- **`<base>/<gw_name>/sensors/<sensor_alias>`** — Sensor data for measurement sensors.
  ```json
  { "name": "tempinf", "alias": "Indoor Temperature", "value": 22.5, "unit": "°C" }
  ```

- **`<base>/<gw_name>/diag/<sensor_alias>`** — Sensor data for diagnostic sensors (batteries, RSSI, etc.). Same payload format as above.

### Subdevices
- **`<base>/<gw_name>/subdevices/<subdevice_id>`** — Subdevice info.
  ```json
  {
    "id": 12345,
    "model": 1,
    "devicename": "WFC01",
    "nickname": "Garden Valve",
    "state": "online",
    "ver": 15
  }
  ```

- **`<base>/<gw_name>/subdevices/<subdevice_id>/availability`** — Subdevice availability (`online`/`offline`).

### Subdevice Sensors
- **`<base>/<gw_name>/subdevices/<subdevice_id>/sensors/<sensor_alias>`** — Measurement sensors.
- **`<base>/<gw_name>/subdevices/<subdevice_id>/diag/<sensor_alias>`** — Diagnostic sensors.

Same payload format as gateway sensors.

## Subscribed Topics

### Home Assistant Status
- **`homeassistant/status`** — Listens for `online`/`offline`. On `online`, all gateways and subdevices are re-emitted so HA picks them up after restart.

### Subdevice Commands (direct MQTT)
- **`<base>/+/subdevices/+/cmd`** — Accepts JSON commands for subdevice control.
  ```jsonc
  { "cmd": "Start", "id": 12345, "duration": 20, "unit": "Minutes" }
  // cmd: Start | Stop. unit: Seconds | Minutes | Hours | Liters (or 0|1|2|3).
  // duration is in the chosen unit; the controller normalizes time to seconds
  // and volume to deciliters for the gateway. Omit duration/unit for an
  // always-on Start.
  ```

### Subdevice Commands (Home Assistant)
- **`<base>/+/subdevices/+/cmd/homeassistant`** — Simplified command topic for HA switches. Payload is plain text `ON` or `OFF` (always-on run / stop).

### Subdevice Run Modes (Home Assistant)
Controllable subdevices (WFC01/WFC02/AC1100) expose duration/volume run controls in addition to the on/off switch. The controller **stages** the selected mode and value, then fires the run when the Start button is pressed — the device self-closes at the duration/volume target.

- **`<base>/+/subdevices/+/cmd/mode`** — selected run mode. Payload: `Duration` or `Volume`.
- **`<base>/+/subdevices/+/cmd/set/duration`** — run duration in minutes (plain integer).
- **`<base>/+/subdevices/+/cmd/set/volume`** — run volume in liters (plain integer; flow-capable valves only).
- **`<base>/+/subdevices/+/cmd/start`** — start a run using the currently staged mode + value.

All subdevice command topics (the four above plus `cmd` and `cmd/homeassistant`) are covered by a single `<base>/+/subdevices/+/cmd/#` subscription. A run mode the device doesn't support (e.g. `Volume` on a flow-less valve) is rejected; a `Start` with a missing/invalid value falls back to a safe default (3 min / 5 L).

## Home Assistant Discovery

When `controller.homeassistantdiscovery` is enabled, discovery configs are published as retained messages under the `homeassistant/` prefix.

### Discovery Topic Pattern
- **`homeassistant/sensor/<device_name>/config`** — Gateway & subdevice availability entities
- **`homeassistant/sensor/<device_name>_<sensor_name>/config`** — Sensor entities
- **`homeassistant/binary_sensor/<device_name>_<sensor_name>/config`** — Binary sensor entities (rain state, running state, etc.)
- **`homeassistant/switch/<subdevice_nickname>/config`** — Switch entities for controllable subdevices
- **`homeassistant/number/<subdevice_nickname>_<param>/config`** — Run Duration / Run Volume inputs (`entity_category: config`)
- **`homeassistant/select/<subdevice_nickname>/config`** — Run Mode selector (published only when 2+ modes apply)
- **`homeassistant/button/<subdevice_nickname>/config`** — Start Run button

### Discovery Payload Structure

**Gateway device:**
```json
{
  "device": {
    "identifiers": ["ec_weatherstation-01"],
    "name": "weatherstation-01",
    "model": "GW2000A",
    "manufacturer": "Ecowitt",
    "hw_version": "GW2000A",
    "sw_version": "GW2000A_V3.1.3"
  },
  "origin": {
    "name": "Ecowitt Controller",
    "sw": "v2.0.0",
    "url": "https://github.com/mplogas/ecowitt-controller"
  },
  "name": "Availability",
  "unique_id": "ec_weatherstation-01_availability",
  "object_id": "ec_weatherstation-01_availability",
  "availability_topic": "<base>/weatherstation-01/availability",
  "state_topic": "<base>/weatherstation-01/availability",
  "retain": false,
  "qos": 1
}
```

**Subdevice device** (linked to gateway via `via_device`):
```json
{
  "device": {
    "identifiers": ["ec_garden-valve"],
    "name": "Garden Valve",
    "model": "1",
    "manufacturer": "Ecowitt",
    "hw_version": "1",
    "sw_version": "15",
    "via_device": "ec_weatherstation-01"
  },
  ...
}
```

**Sensor entity:**
```json
{
  "device": { "..." },
  "origin": { "..." },
  "name": "Indoor Temperature",
  "unique_id": "ec_weatherstation-01_tempinf_temperature",
  "object_id": "ec_weatherstation-01_tempinf_temperature",
  "device_class": "temperature",
  "state_topic": "<base>/weatherstation-01/sensors/indoor-temperature",
  "value_template": "{{ value_json.value }}",
  "unit_of_measurement": "°C",
  "retain": false,
  "qos": 1
}
```

**Binary sensor entity:**
```json
{
  "value_template": "{% if (value_json.value == true) -%} ON {%- else -%} OFF {%- endif %}",
  ...
}
```

**Switch entity (subdevice toggle):**
```json
{
  "name": "switch",
  "state_topic": "<base>/<gw_name>/subdevices/<id>/diag/running",
  "command_topic": "<base>/<gw_name>/subdevices/<id>/cmd/homeassistant",
  "value_template": "{% if (value_json.value == true) -%} ON {%- else -%} OFF {%- endif %}",
  ...
}
```

### Identifier Format
- Device identifiers: `ec_<name>` (sanitized, lowercase, hyphens)
- Entity unique IDs: `ec_<device_name>_<type>` (e.g. `ec_weatherstation-01_availability`)
- Sensor entity IDs: `ec_<device_name>_<sensor_name>_<sensor_type>` (e.g. `ec_weatherstation-01_tempinf_temperature`)

### Device Class Mapping
Sensor types are mapped to Home Assistant device classes (see `DiscoveryBuilder.BuildDeviceCategory`). Examples: `temperature`, `humidity`, `pressure`, `battery`, `wind_speed`, `precipitation`, `voltage`, `current`, `power`, `signal_strength`, etc.

### Entity Categories
- **Diagnostic** sensors (batteries, RSSI, heap, runtime, etc.) are published with `"entity_category": "diagnostic"` so they appear under device diagnostics in HA rather than as primary entities.
