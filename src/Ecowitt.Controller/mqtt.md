# MQTT

## MQTT topic considerations

#### Requirements
- easily accessible for HA Discovery
- Gateways, Subdevices and Sensors should be easily accessible through topic filter, eg. `ecowitt/+/subdevices/#` or `ecowitt/+/sensors/#` or  `ecowitt/+/subdevices/+/sensors/#`
- sensor topics should be filtered with `<base>/sensors/+/temperature` or `<base>/sensors/+/humidity`
- sensor payload should be simple json, eg. `{"value": 23.4, "unit": "°C"}`
- state payload should be `{"state": "online"}` or `{"state": "offline"}` and part of the payload to gateways or subdevices

#### Topic Templates
- `ecowitt/<gw_id>` - Gateway hw_info & state
- `ecowitt/<gw_id>/subdevices/<subdevice_id>` - Subdevice hw_info & state
- `ecowitt/<gw_id>/sensors/<sensor_id>/<sensor_type>` - Gateway Sensor data
- `ecowitt/<gw_id>/subdevices/<subdevice_id>/sensors/<sensor_id>/<sensor_type>` - Subdevice Sensor data

