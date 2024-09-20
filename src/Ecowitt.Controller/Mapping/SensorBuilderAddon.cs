using System.Transactions;
using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Mapping
{
    public partial class SensorBuilder
    {
        public static void CalculateWFC01Addons(ref Model.Subdevice subdevice)
        {
            if (subdevice.Model == SubdeviceModel.WFC01)
            {
                var waterTotal = subdevice.Sensors.FirstOrDefault(s =>
                    s.Name.Equals("water_total", StringComparison.InvariantCultureIgnoreCase));
                var waterHappen = subdevice.Sensors.FirstOrDefault(s =>
                    s.Name.Equals("happen_water", StringComparison.InvariantCultureIgnoreCase));

                if (waterTotal != null && waterHappen != null)
                {
                    waterHappen.Value = (double)waterTotal.Value - (double)waterHappen.Value;
                }
            }
        }

        public static void CalculateGatewayAddons(ref Model.Gateway gateway, bool isMetric)
        {
            var tempin = gateway.Sensors.FirstOrDefault(s => s.Name.Equals("tempinf", StringComparison.InvariantCultureIgnoreCase));
            var humidityin = gateway.Sensors.FirstOrDefault(s => s.Name.Equals("humidityin", StringComparison.InvariantCultureIgnoreCase));

            if (tempin != null && humidityin != null)
            {
                var dewPoint = BuildTemperatureSensor("dewpointin", "Indoor Dewpoint",
                    isMetric
                        ? CalculateDewPointMetric((double)tempin.Value, (double)humidityin.Value).ToString()
                        : CalculateDewPointImperial((double)tempin.Value, (double)humidityin.Value).ToString(), isMetric, isMetricdot);
                if (dewPoint != null) gateway.Sensors.Add(dewPoint);
            }

            var temp = gateway.Sensors.FirstOrDefault(s => s.Name.Equals("tempf", StringComparison.InvariantCultureIgnoreCase));
            var humidity = gateway.Sensors.FirstOrDefault(s => s.Name.Equals("humidity", StringComparison.InvariantCultureIgnoreCase));
            if (temp != null && humidity != null)
            {
                var dewPoint = BuildTemperatureSensor("dewpoint", "Outdoor Dewpoint",
                    isMetric
                        ? CalculateDewPointMetric((double)temp.Value, (double)humidity.Value).ToString()
                        : CalculateDewPointImperial((double)temp.Value, (double)humidity.Value).ToString(), isMetric, isMetric);
                var heatIndex = BuildTemperatureSensor("heatindex", "Heat Index",
                    isMetric
                        ? CalculateHeatIndexMetric((double)temp.Value, (double)humidity.Value).ToString()
                        : CalculateHeatIndexImperial((double)temp.Value, (double)humidity.Value).ToString(), isMetric, isMetric);
                
                if (dewPoint != null) gateway.Sensors.Add(dewPoint);
                if (heatIndex != null) gateway.Sensors.Add(heatIndex);
            }

            var windspeed = gateway.Sensors.FirstOrDefault(s => s.Name.Equals("windspeedmph", StringComparison.InvariantCultureIgnoreCase));
            if (temp != null && windspeed != null)
            {
                var windChill = BuildTemperatureSensor("windchill", "Wind Chill",
                    isMetric
                        ? CalculateWindChillMetric((double)temp.Value, (double)windspeed.Value).ToString()
                        : CalculateWindChillImperial((double)temp.Value, (double)windspeed.Value).ToString(), isMetric, isMetric);
                if (windChill != null) gateway.Sensors.Add(windChill);
            }

            var winddirection = gateway.Sensors.FirstOrDefault(s => s.Name.Equals("winddir", StringComparison.InvariantCultureIgnoreCase));
            if (winddirection != null)
            {
                var compass = BuildStringSensor("winddir-comp", "Wind Direction (Compass)",
                    CalculateWindDirection((int)winddirection.Value).ToString());
                gateway.Sensors.Add(compass);
            }

            var sensorsToAdd = new List<ISensor>();
            var pm25 = gateway.Sensors.Where(s => s.Name.StartsWith("pm25_avg_24h") || s.Name.StartsWith("pm25_24h"));
            sensorsToAdd.AddRange(pm25.Select(sensor => BuildStringSensor($"{sensor.Name}-aqi", $"{sensor.Alias} AQI", CalculatePm25Aqi24h((double)sensor.Value))));

            var pm10 = gateway.Sensors.Where(s => s.Name.StartsWith("pm10_avg_24h") || s.Name.StartsWith("pm10_24h"));
            sensorsToAdd.AddRange(pm10.Select(sensor => BuildStringSensor($"{sensor.Name}-aqi", $"{sensor.Alias} AQI", CalculatePm10Aqi24h((double)sensor.Value))));

            gateway.Sensors.AddRange(sensorsToAdd);
        }

        // shout out to wikipedia for the formulas! <3
        private static double CalculateDewPointMetric(double tempC, double relativeHumidity)
        {
            return tempC - ((100 - relativeHumidity) / 5);
        }

        private static double CalculateDewPointImperial(double tempF, double relativeHumidity)
        {
            return tempF - ((100 - relativeHumidity) * 0.36);
        }

        private static double CalculateHeatIndexMetric(double tempC, double relativeHumidity)
        {
            return -8.78469475556 + 1.61139411 * tempC + 2.33854883889 * relativeHumidity
                   - 0.14611605 * tempC * relativeHumidity - 0.012308094 * Math.Pow(tempC, 2)
                   - 0.0164248277778 * Math.Pow(relativeHumidity, 2) + 0.002211732 * Math.Pow(tempC, 2) * relativeHumidity
                   + 0.00072546 * tempC * Math.Pow(relativeHumidity, 2) - 0.000003582 * Math.Pow(tempC, 2) * Math.Pow(relativeHumidity, 2);
        }

        private static double CalculateHeatIndexImperial(double tempF, double relativeHumidity)
        {
            return -42.379 + 2.04901523 * tempF + 10.14333127 * relativeHumidity
                - 0.22475541 * tempF * relativeHumidity - 0.00683783 * Math.Pow(tempF, 2)
                - 0.05481717 * Math.Pow(relativeHumidity, 2) + 0.00122874 * Math.Pow(tempF, 2) * relativeHumidity
                + 0.00085282 * tempF * Math.Pow(relativeHumidity, 2) - 0.00000199 * Math.Pow(tempF, 2) * Math.Pow(relativeHumidity, 2);
        }

        private static double CalculateWindChillMetric(double tempC, double windSpeedKmh)
        {
            return 13.12 + 0.6215 * tempC - 11.37 * Math.Pow(windSpeedKmh, 0.16) + 0.3965 * tempC * Math.Pow(windSpeedKmh, 0.16);
        }

        private static double CalculateWindChillImperial(double tempF, double windSpeedMph)
        {
            return 35.74 + 0.6215 * tempF - 35.75 * Math.Pow(windSpeedMph, 0.16) + 0.4275 * tempF * Math.Pow(windSpeedMph, 0.16);
        }

        private static string CalculateWindDirection(int windDirection)
        {
            return windDirection switch
            {
                //>= 348.75 => "N",
                //>= 326.25 => "NNW",
                //>= 303.75 => "NW",
                //>= 281.25 => "WNW",
                //>= 258.75 => "W",
                //>= 236.25 => "WSW",
                //>= 213.75 => "SW",
                //>= 191.25 => "SSW",
                //>= 168.75 => "S",
                //>= 146.25 => "SSE",
                //>= 123.75 => "SE",
                //>= 101.25 => "ESE",
                //>= 78.75 => "E",
                //>= 56.25 => "ENE",
                //>= 33.75 => "NE",
                //>= 11.25 => "NNE",
                //_ => "N"
                >= 349 => "N",
                >= 326 => "NNW",
                >= 304 => "NW",
                >= 281 => "WNW",
                >= 259 => "W",
                >= 236 => "WSW",
                >= 214 => "SW",
                >= 191 => "SSW",
                >= 169 => "S",
                >= 146 => "SSE",
                >= 124 => "SE",
                >= 101 => "ESE",
                >= 79 => "E",
                >= 56 => "ENE",
                >= 34 => "NE",
                >= 11 => "NNE",
                _ => "N"
            };
        }

        // according to wikipedia, each region/country has its own AQI calculation and each pollutant has its own AQI calculation
        // this is the default Ecowitt (seemingly the US one) implementation
        private static string CalculatePm25Aqi24h(double particles24h)
        {
            return particles24h switch
            {
                >= 225.4 => "Hazardous",
                >= 125.3 => "Very Unhealthy",
                >= 55.3 => "Unhealthy",
                >= 35.3 => "Unhealthy for Sensitive Groups",
                >= 9.0 => "Moderate",
                >= 0 => "Good",
                _ => "Unknown"
            };
        }

        private static string CalculatePm10Aqi24h(double particles24h)
        {
            return particles24h switch
            {
                >= 424 => "Hazardous",
                >= 354 => "Very Unhealthy",
                >= 254 => "Unhealthy",
                >= 154 => "Unhealthy for Sensitive Groups",
                >= 54 => "Moderate",
                >= 0 => "Good",
                _ => "Unknown"
            };
        }

        //private static string CalculateAqiColor(string aqi)
        //{
        //    return aqi switch
        //    {
        //        >= "Hazardous" => "#7E0023", // maroon
        //        >= "Very Unhealthy" => "#99004C", //bordeaux red
        //        >= "Unhealthy" => "#FF0000", // red
        //        >= "Unhealthy for Sensitive Groups" => "#FFA500", // orange
        //        >= "Moderate" => "#FFFF00",  // yellow
        //        >= "Good" => "#00ff00", // green
        //        _ => "#cfcfd4" // grey
        //    };
        //}
    }
}
