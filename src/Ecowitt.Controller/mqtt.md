# MQTT

## MQTT topic considerations

#### Requirements
- easily accessible for HA Discovery
- Gateways, Subdevices and Sensors should be easily accessible through topic filter, eg. `ecowitt/+/subdevices/#` or `ecowitt/+/sensors/#` or  `ecowitt/+/subdevices/+/sensors/#`
- sensor topics should be filtered with `<base>/sensors/+/temperature/data` or `<base>/sensors/+/humidity/data`
- sensor payload should be simple json, eg. `{"value": 23.4, "unit": "°C"}`
- state topic should be `ecowitt/+/state` `ecowitt/+/subdevices/+/state` 
- state payload should be `{"state": "online"}` or `{"state": "offline"}`

#### Topic Templates
- `ecowitt/<gw_id>/state` - Gateway state
- `ecowitt/<gw_id>/subdevices/<subdevice_id>/state` - Subdevice state
- `ecowitt/<gw_id>/sensors/<sensor_id>/data` - Gateway Sensor data
- `ecowitt/<gw_id>/subdevices/<subdevice_id>/sensors/<sensor_id>/<sensor_type>/data` - Subdevice Sensor data

