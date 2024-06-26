# Ecowitt Controller

## Overview

  
The Ecowitt Controller is a project designed to extend the capabilities of Ecowitt Weatherstations. While existing implementations primarily focus on reading data sent by the base station, this project introduces the ability to both read data from subdevices and control them. This includes new controllable devices such as valves (WFC01) and sockets (AC1100), providing a more comprehensive and interactive weather monitoring and control solution for your home automation.

## Features

### MQTT Integration

- **Control Subdevices**: Easily control connected subdevices through MQTT.
- **Receive Sensor Data**: Collect sensor data from both the main station and its subdevices.


### Auto-Discovery

- **Seamless Integration**: Simply point the custom weather service of your weather station to the integrated endpoint, and the tool will automatically discover connected subdevices.


### High Configurability

- **Flexible Settings**: Turn off auto-discovery, configure MQTT output, or set up retries according to your needs. The tool is designed to be highly customizable.


### Compatibility

- **Supported Devices**: Tested with GW1000 and GW2000, any Ecowitt Gateway should work, but only GW1200 and GW2000 support subdevices
- **Easy Setup**: Utilize the Ecowitt custom weather station endpoint for a straightforward setup process.


## Implementation

  
This project is implemented in C#/.NET 8, leveraging ASP.NET Core Web API as the endpoint for the Ecowitt custom weather station. Key components include:

- **ASP.NET Core Web API**: Serves as the endpoint for receiving data from the weather station.
- **[MQTTnet](https://github.com/dotnet/MQTTnet)**: Provides MQTT background services for communication.
- **Polling Background Service**: Regularly fetches updates from subdevices.
- **[SlimMessageBus](https://github.com/zarusz/SlimMessageBus)**: An in-memory message bus that ties all components together for efficient communication.


## Getting Started

### Prerequisites

- .NET 8 SDK
- An Ecowitt Weatherstation (controllable subdevices optional)
- MQTT broker


### Installation

*docker container is the recommended way to run it, Dockerfile is part of the project and image will be made available soon*

1. Clone the repository:


- `git clone https://github.com/yourusername/ecowitt-weatherstation-controller.git`

- Navigate to the project directory:

- `cd ecowitt-controller`

- Build the project:

- `dotnet build`

- Run the application:


1. `dotnet run`


### Configuration

Configure your Ecowitt Weatherstation to point to the provided endpoint. Adjust settings in the `appsettings.json` file to customize auto-discovery, MQTT output, and other parameters.

```
{  
  "AllowedHosts": "*",  
  "mqtt": {  
    "host": "",  
    "user": "",  //optional
    "password": "",    //optional
    "port": 1883,    //optional
    "topic": "ecowitt",    //optional
    "clientId": "ecowitt-controller",   //optional  
    "reconnect": true,    //optional
    "reconnectAttemps": 5    //optional
  },  
  "ecowitt": {  
    "pollingInterval": 30,
    "autodiscovery": true,    //optional
    "retries": 2,    //optional
    "gateways": [    //entire gateway block is optional in autodiscovery-mode
      {        
        "name": "subdevice1"
        "ip": "127.0.0.1",  
        "username": "",  
        "password": ""  
      }  
	 ]  
    },      
    "controller": {  
    "precision": 2,  
    "unit": "metric"  
  }  
}
```

## Contributing

  
We welcome contributions from the community! Please read our [CONTRIBUTING.md](https://oai.azure.com/portal/4b29ef036c1d4f3cbcb407bd0e6fe5a2/CONTRIBUTING.md) file for guidelines on how to get involved.

## License

  
This project is licensed under the MIT License. See the [LICENSE](https://oai.azure.com/portal/4b29ef036c1d4f3cbcb407bd0e6fe5a2/LICENSE) file for more details.

## Contact

##  
For any questions or feedback, please open an issue on GitHub.  
