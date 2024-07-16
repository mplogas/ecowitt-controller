### setting up a gateway
topic `homeassistant/sensor/gw2000/config`

payload: 
```json
{
  "device": {
    "hw_version": "gw2000_v1.2.3",
    "identifiers": [
      "gw2000_testgateway"
    ],
    "manufacturer": "Ecowitt",
    "model": "GW2000",
    "name": "Testgateway",
    "sw_version": "1.2.3"
  },
  "name": "Availability",
  "availability_topic": "ecowitt/gw2000/availability",
  "object_id": "ecowitt_gw2000_availability_state",
  "state_topic": "ecowitt/gw2000/state",
  "unique_id": "ecowitt_gw2000_availability_state",
  "origin": {
    "name": "Ecowitt Controller",
    "sw": "0.0.1",
    "url": "https://github.com/mplogas/ecowitt-controller"
  }
}
```

### setting up a sensor for the gateway
topic `homeassistant/sensor/gw2000/uv/config`

payload: 
```json
{
  "device": {
    "hw_version": "gw2000_v1.2.3",
    "identifiers": [
      "gw2000_testgateway"
    ],
    "manufacturer": "Ecowitt",
    "model": "GW2000",
    "name": "Testgateway",
    "sw_version": "1.2.3"
  },
  "name": "uv",
  "retain": false,
  "availability_topic": "ecowitt/gw2000/availability",
  "object_id": "ecowitt_gw2000_uv_state",
  "state_topic": "ecowitt/gw2000/state",
  "value_template": "{{ value_json.uv }}",
  "unique_id": "ecowitt_gateway_1234567890_uv_state",
  "icon": "mdi:weather-sunny",
  "qos": 1,
  "state_class": "measurement",
  "unit_of_measurement": "UV index",
  "origin": {
    "name": "Ecowitt Controller",
    "sw": "0.0.1",
    "url": "https://github.com/mplogas/ecowitt-controller"
  }
}
```
### subdevice
topic `homeassistant/sensor/wfc01/config`

payload: 
```json
{
  "device": {
    "hw_version": "wfc01_v1.0.0",
    "identifiers": [
      "wfc01_012345"
    ],
    "manufacturer": "Ecowitt",
    "model": "WFC01",
    "name": "WittFlow 001",
    "sw_version": "1.0.1",
    "via_device": "gw2000_testgateway"
  },
  "name": "availability",
  "retain": false,
  "availability": [
    {
        "topic": "ecowitt/gw2000/availability",
        "payload_available": "online",
        "payload_not_available": "offline",
        "value_template": "{{ value_json.state }}"
    },
    {
        "topic": "ecowitt/wfc01/availability",
        "payload_available": "online",
        "payload_not_available": "offline",
        "value_template": "{{ value_json.state }}"
    }
    ], 
  "availability_mode": "all",
  "state_topic": "ecowitt/wfc01/state",
  "object_id": "ecowitt_wfc01_availability_state",
  "unique_id": "ecowitt_wfc01_availability_state",
  "qos": 1,
  "origin": {
    "name": "Ecowitt Controller",
    "sw": "0.0.1",
    "url": "https://github.com/mplogas/ecowitt-controller"
  }
}
```

### subdevice toggle
topic `homeassistant/switch/wfc01/config`

payload: 
```json
{
  "device": {
    "hw_version": "wfc01_v1.0.0",
    "identifiers": [
      "wfc01_012345"
    ],
    "manufacturer": "Ecowitt",
    "model": "WFC01",
    "name": "WittFlow 001",
    "sw_version": "1.0.1",
    "via_device": "gw2000_testgateway"
  },
  "name": "toggle",
  "retain": false,
  "availability": [
    {
        "topic": "ecowitt/gw2000/availability",
        "payload_available": "online",
        "payload_not_available": "offline",
        "value_template": "{{ value_json.state }}"
    },
    {
        "topic": "ecowitt/wfc01/availability",
        "payload_available": "online",
        "payload_not_available": "offline",
        "value_template": "{{ value_json.state }}"
    }
    ], 
  "availability_mode": "all",
  "command_topic": "ecowitt/wfc01/cmd",
  "state_topic": "ecowitt/wfc01/state",
  "object_id": "ecowitt_wfc01_toggle_state",
  "unique_id": "ecowitt_wfc01_toggle_state",
  "qos": 1,
  "icon": "mdi:water",
  "origin": {
    "name": "Ecowitt Controller",
    "sw": "0.0.1",
     "url": "https://github.com/mplogas/ecowitt-controller"
  }
}
```