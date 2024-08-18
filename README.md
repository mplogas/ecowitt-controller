## Supported Devices  

### Gateways  
- **GW2000**: The GW2000 is a displayless console/gateway available since April 2022. It offers an extended range of functions, including a browser interface (WebUI) for display and configuration. It supports both Ethernet and WLAN connections, though it's recommended not to use both interfaces simultaneously to avoid blocking the web interface. The GW2000 firmware supports various sensors, including the WFC01 IoT sensor.  
- **GW1200**: This gateway supports bidirectional communication necessary for controlling intelligent components like switches and water valves. It supports a WebUI and also supports the WS View Plus app for configuration .  
- **GW1x00**: The GW1100 is similar to the GW1200 but does not support a WebUI and no subdevices. It is configured and displayed via the WS View Plus app. The GW1100 includes a temperature/humidity sensor for indoor measurements and supports local data display through applications like PWT (Personal Weather Tablet).
- **Other Compatible Gateways**: WN1980, WS3800, and WS39x0 also support IoT sensors and bidirectional communication .  
  
### Subdevices  
- **AC1100 Smart Plug**: The AC1100 is a switchable socket that can be controlled manually, time-controlled, or based on measured values from the weather station. It requires an IoT-enabled console (e.g., GW2000, GW1200) and the Ecowitt app for automatic operation. It supports different regions with corresponding plugs and maximum wattages .  
- **WFC01 Intelligent Water Timer (WittFlow)**: The WFC01 is a timer-controlled or sensor-measurement-dependent water valve. It features a built-in liquid flow sensor and a temperature sensor for the liquid. The valve can be controlled via the Ecowitt app, and it supports various operating modes, including manual, plan, and smart modes . The device is waterproof and dustproof to IP66 standards and built from corrosion-resistant materials .  
  
### Compatibility  
- **Weather Stations**: Fine Offset and its clones (Ecowitt, Froggit, Ambient Weather...) are generally supported if your gateway/weather station supports custom weather station uploads in Ecowitt format.  
- **Web API Devices**: If your device offers a web API (e.g., GW2000, GW1200), this tool offers bidirectional communication, allowing you to control actors like the AC1100 smart plug or the WFC01 water valve .  
## Why  
Ecowitt offers great and reliable weather stations that can be run entirely locally. As a Home Assistant user, I used Ecowitt2Mqtt to collect my weather data. Unfortunately, it doesn't support subdevices (yet), and my Python skills aren't sufficient to extend it. However, I have experience with .NET, IoT, and I enjoy challenges. This project was created to fill that gap and provide a solution for controlling Ecowitt subdevices from Home Assistant. Additionally, the decoupled architecture can serve as a blueprint for future similar projects. 
## Features  
- **Multi-Gateway, Multi-Subdevices Support:** The system supports multiple gateways and subdevices, allowing for extensive scalability and flexibility .  
- **Automatic Discovery:** Newly added sensors and subdevices are automatically detected. This feature can be turned off to allow for manual gateway definitions .  
- **Bidirectional Communication:** Enables data retrieval such as battery state and sensor status from subdevices, and also allows for control commands to be sent .  
- **MQTT-Based Integration:** Weather data is exposed via MQTT, facilitating integration with other IoT systems .  
- **Home Assistant Compatibility:** Supports device discovery messages to the Home Assistant topic, enabling seamless integration with Home Assistant through MQTT .  
- **Highly Configurable:** Offers extensive configuration options including discovery settings, polling schedules, HTTP timeouts, and alternative discovery topics .  
- **Metric and Freedom Units Support:** Transforms data points to metric units by default, with the option to turn this off if imperial units are preferred .  
- **Dynamic Property Discovery:** The system flexibly discovers and publishes new sensor properties by default, ensuring up-to-date data availability . 
## Roadmap  
- **InfluxDB Storage:** Store states directly in InfluxDB without going through Home Assistant first. This will enable more efficient data storage and retrieval, allowing for advanced data analysis and visualization using tools like Grafana.  
- **Calculated Properties:** Implement calculated properties such as dewpoint, wind direction (NESW), windchill, etc. These calculated metrics will provide more comprehensive weather data insights, enhancing the utility of the collected data for various applications.  
## Getting started
To begin, you will need a running MQTT broker along with its IP address or hostname and port number. While TLS and credentials are optional, they are highly recommended for enhanced security.  
## Ecowitt Controller (this tool)  
If you intend to run the Ecowitt Controller in a Docker container, you will need to create a new `.json` file named `appsettings.json`. For a minimal setup, you only need to specify the IP address of your MQTT broker. All other settings will be automatically populated with default values.  
### Minimal Configuration  
 ``` json
{
  "mqtt": {
    "host": ""    
  }
}
``` 

For a more detailed configuration, including MQTT credentials and polling intervals, see the full configuration example below. This example also enables debug logging.  
### Full Config

``` json
{
  "Serilog": {
    "MinimumLevel": "Debug"
  },
  "mqtt": {
    "host": "",
    "user": "",
    "password": "",
    "port": 1883,
    "basetopic": "ecowitt",
    "clientId": "ecowitt-controller",
    "reconnect": true,
    "reconnectAttemps": 5    
  },
  "ecowitt": {
    "pollingInterval": 30, //seconds
    "autodiscovery": true,
    "retries": 2
  },
  "controller": {
    "precision": 2, //math.round to x digits
    "unit": "metric", // or "imperial"
    "publishingInterval": 10, //seconds
    "homeassistantdiscovery": true
  }
}

```
### Run as Docker
To run Ecowitt Controller in a Docker container, mount your config file and forward port 8080:

`docker run -d --name ecowitt-controller -v path/to/appsettings.json:/config/appsettings.json:ro -p 8080:8080 mplogas/ecowitt-controller:latest`  
### Run on Bare-Metal
To run Ecowitt Controller on bare-metal, clone the repository and modify the `appsettings.json`. Then, change into the `src` directory and run:

`dotnet run -C Release Ecowitt.Controller.csproj`  
### Configure Your Weather Station
To configure your Ecowitt weather station to send data to the Ecowitt Controller, follow these steps:  

1. **Access the WebUI or WS View Plus App:**
    - For the GW2000, you can use the built-in WebUI available at the device's IP address .
    - Alternatively, use the WS View Plus app to configure the station.  
        
2. **Set Up Custom Server:**
    - Navigate to the weather services configuration page.
    - Enable the 'customized' data posting option.
    - Choose the Ecowitt protocol.
    - Enter the IP address or hostname of your Ecowitt-Controller instance.
    - Specify the path as `/data/report`.
    - Set the port number (default is 8080).
    - Define the posting interval (e.g., 30 seconds).  
        
3. **Save and Apply Settings:**
    - Save the configuration and ensure the weather station starts sending data to the specified Ecowitt-Controller instance.  
### Home Assistant
Ensure your Home Assistant has permission set up to listen for MQTT messages from the Ecowitt Controller topic. The Home Assistant Discovery should automatically pick up new devices and sensors if `homeassistantdiscovery` is enabled in your `appsettings.json`.
## Implementation      

This project is developed using C# and .NET 8, utilizing ASP.NET Core Web API to create an endpoint for the Ecowitt custom weather station. The main components of the implementation include:      
- **ASP.NET Core Web API**: Acts as the endpoint to receive data from the weather station.   
- **[MQTTnet](https://github.com/dotnet/MQTTnet)**: Facilitates MQTT background services for communication with the weather station.   
- **Polling Background Service**: Periodically retrieves updates from subdevices.   
- **[SlimMessageBus](https://github.com/zarusz/SlimMessageBus)**: An in-memory message bus that integrates all components for efficient communication.`
## Contributing
We welcome contributions from the community! Please read our [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines on how to get involved.
## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
## Contact
For any questions or feedback, please open an issue on GitHub.  