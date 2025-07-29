using Ecowitt.Controller.Model;
using Serilog;

/* all currently available sensors. their naming scheme is not consistent, so we have to map them manually
PASSKEY
stationtype
runtime
dateutc
tempinf
humidityin 
baromrelin 
baromabsin 
tempf  
humidity   
winddir
windspeedmph
windgustmph
maxdailygust
solarradiation
uv     
rainratein 
eventrainin
hourlyrainin
dailyrainin
weeklyrainin
monthlyrainin 
yearlyrainin
totalrainin
srain_piezo
rrain_piezo
erain_piezo
hrain_piezo
drain_piezo
wrain_piezo
mrain_piezo
yrain_piezo
ws90cap_volt
ws90_ver   
tempf1,….,,tempf8             
humidity1,….,humidity8        
soilmoisture1,….., soilmoisture8  
soilad1,…., soilad8           
pm25_ch1, …., pm25_ch4        
pm25_avg_24h_ch1,…..,pm25_avg_24h_ch4
tf_co2                        
humi_co2                      
pm1_co2                       
pm1_24h_co2                   
pm25_co2                      
pm25_24h_co2                  
pm4_co2                       
pm4_24h_co2                   
pm10_co2                      
pm10_24h_co2                  
co2                           
co2_24h                       
lightning_num                 
lightning                     
lightning_time                
leak_ch1 …., leak_ch4         
tf_ch1, …., tf_ch8            
leafwetness_ch1,…, leafwetness_ch8
console_batt                  
wh65batt                      
wh80batt                      
wh26batt                      
batt1,….,batt8                
soilbatt1,…,soilbatt8         
pm25batt1, …, pm25batt4       
wh57batt                      
leakbatt1, …,leakbatt4        
tf_batt1, …, tf_batt8 
co2_batt          
leaf_batt1, …, leaf_batt8
wh90batt          
freq              
model             
interval          
ac_status         
warning           
always_on         
val_type          
val               
run_time          
rssi              
gw_rssi           
timeutc           
publish_time      
ac_action         
ac_running        
plan_status       
elect_total       
happen_elect      
realtime_power    
ac_voltage        
ac_current        
water_status      
water_action      
water_running     
water_total       
happen_water      
flow_velocity     
water_temp        
wfc01batt         
*/

namespace Ecowitt.Controller.Mapping
{
    public partial class SensorBuilder
    {
        public static ISensor? BuildSensor(string propertyName, string propertyValue, bool isMetric = true)
        {
            switch (propertyName)
            {
                case "tempinf":
                    return BuildTemperatureSensor(propertyName, "Indoor Temperature", propertyValue, isMetric);
                case "tempf":
                    return BuildTemperatureSensor(propertyName, "Outdoor Temperature", propertyValue, isMetric);
                case "humidity":
                    return BuildHumiditySensor(propertyName, "Outdoor Humidity", propertyValue);
                case "humidityin":
                    return BuildHumiditySensor(propertyName, "Indoor Humidity", propertyValue);
                case "baromrelin":
                    return BuildPressureSensor(propertyName, "Relative Pressure", propertyValue, isMetric);
                case "baromabsin":
                    return BuildPressureSensor(propertyName, "Absolute Pressure", propertyValue, isMetric);
                case "windspeedmph":
                    return BuildWindSpeedSensor(propertyName, "Wind Speed", propertyValue, isMetric);
                case "windgustmph":
                    return BuildWindSpeedSensor(propertyName, "Wind Gust", propertyValue, isMetric);
                    //return BuildDoubleSensor(propertyName, "Wind Gust", propertyValue, "km/h", SensorType.WindSpeed);
                case "maxdailygust":
                    return BuildWindSpeedSensor(propertyName, "Max Daily Gust", propertyValue, isMetric);
                case "winddir":
                    return BuildIntSensor(propertyName, "Wind Direction", propertyValue, "°");
                case "solarradiation":
                    return BuildIntSensor(propertyName, "Solar Radiation", propertyValue, "W/m²", SensorType.Irradiance);
                case "uv":
                    return BuildIntSensor(propertyName, "UV Index", propertyValue);
                case "srain_piezo":
                    return BuildBinarySensor(propertyName, "Rain State", propertyValue, false);
                case "rainratein":
                case "rrain_piezo":
                    return BuildRainRateSensor(propertyName, "Rain Rate", propertyValue, isMetric);
                case "eventrainin":
                case "erain_piezo":
                    return BuildRainSensor(propertyName, "Event Rain", propertyValue, isMetric);
                case "hourlyrainin":
                case "hrain_piezo":
                    return BuildRainSensor(propertyName, "Hourly Rain", propertyValue, isMetric);
                case "dailyrainin":
                case "drain_piezo":
                    return BuildRainSensor(propertyName, "Daily Rain", propertyValue, isMetric);
                case "weeklyrainin":
                case "wrain_piezo":
                    return BuildRainSensor(propertyName, "Weekly Rain", propertyValue, isMetric);
                case "monthlyrainin":
                case "mrain_piezo":
                    return BuildRainSensor(propertyName, "Monthly Rain", propertyValue, isMetric);
                case "yearlyrainin":
                case "yrain_piezo":
                    return BuildRainSensor(propertyName, "Yearly Rain", propertyValue, isMetric);
                case "totalrainin":
                    return BuildRainSensor(propertyName, "Total Rain", propertyValue, isMetric);
                case "ws90cap_volt":
                    return BuildVoltageSensor(propertyName, "WS90 Capacitor Voltage", propertyValue, true);
                case "tempf1":
                case "tempf2":
                case "tempf3":
                case "tempf4":
                case "tempf5":
                case "tempf6":
                case "tempf7":
                case "tempf8":
                    var number = GetNumber(propertyName);
                    return BuildTemperatureSensor(propertyName, $"Temperature {number}", propertyValue, isMetric);
                case "humidity1":
                case "humidity2":
                case "humidity3":
                case "humidity4":
                case "humidity5":
                case "humidity6":
                case "humidity7":
                case "humidity8":
                    number = GetNumber(propertyName);
                    return BuildHumiditySensor(propertyName, $"Humidity {number}", propertyValue);
                case "soilmoisture1":
                case "soilmoisture2":
                case "soilmoisture3":
                case "soilmoisture4":
                case "soilmoisture5":
                case "soilmoisture6":
                case "soilmoisture7":
                case "soilmoisture8":
                    number = GetNumber(propertyName);
                    return BuildDoubleSensor(propertyName, $"Soil Moisture {number}", propertyValue, "%", SensorType.Moisture);
                case "soilad1":
                case "soilad2":
                case "soilad3":
                case "soilad4":
                case "soilad5":
                case "soilad6":
                case "soilad7":
                case "soilad8":
                    number = GetNumber(propertyName);
                    return BuildIntSensor(propertyName, $"Soil Admittance {number}", propertyValue, "mS");
                case "pm25_ch1":
                case "pm25_ch2":
                case "pm25_ch3":
                case "pm25_ch4":
                    number = GetNumber(propertyName);
                    return BuildParticleSensor(propertyName, $"PM2.5 Channel {number}", propertyValue, SensorType.Pm25, isMetric);
                case "pm25_avg_24h_ch1":
                case "pm25_avg_24h_ch2":
                case "pm25_avg_24h_ch3":
                case "pm25_avg_24h_ch4":
                    number = GetNumber(propertyName);
                    return BuildParticleSensor(propertyName, $"PM2.5 Channel {number} 24h Average", propertyValue, SensorType.Pm25, isTotal: true);
                case "tf_co2":
                    return BuildTemperatureSensor(propertyName, "CO2 Temperature", propertyValue, isMetric);
                case "humi_co2":
                    return BuildHumiditySensor(propertyName, "CO2 Humidity", propertyValue);
                case "pm1_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM1", propertyValue, SensorType.Pm1);
                case "pm1_24h_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM1 24h Average", propertyValue, SensorType.Pm1, isTotal: true);
                case "pm25_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM2.5", propertyValue, SensorType.Pm25);
                case "pm25_24h_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM2.5 24h Average", propertyValue, SensorType.Pm25, isTotal: true);
                case "pm4_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM4", propertyValue, SensorType.Pm1);
                case "pm4_24h_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM4 24h Average", propertyValue, SensorType.Pm1, isTotal: true);
                case "pm10_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM10", propertyValue, SensorType.Pm10);
                case "pm10_24h_co2":
                    return BuildParticleSensor(propertyName, "CO2 PM10 24h Average", propertyValue, SensorType.Pm10, isTotal: true);
                case "co2":
                    return BuildPPMSensor(propertyName, "CO2", propertyValue, SensorType.CarbonDioxide);
                case "co2_24h":
                    return BuildPPMSensor(propertyName, "CO2 24h Average", propertyValue, SensorType.CarbonDioxide, isTotal: true);
                case "lightning_num":
                    return BuildIntSensor(propertyName, "Lightning Strikes", propertyValue);
                case "lightning":
                    return BuildDistanceSensor(propertyName, "Lightning Distance", propertyValue);
                case "lightning_time":
                    return BuildDateTimeSensor(propertyName, "Lightning Time", propertyValue);
                case "leak_ch1":
                case "leak_ch2":
                case "leak_ch3":
                case "leak_ch4":
                    // maybe it's bool, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildIntSensor(propertyName, $"Leak Channel {number}", propertyValue);
                case "tf_ch1":
                case "tf_ch2":
                case "tf_ch3":
                case "tf_ch4":
                case "tf_ch5":
                case "tf_ch6":
                case "tf_ch7":
                case "tf_ch8":
                    number = GetNumber(propertyName);
                    return BuildTemperatureSensor(propertyName, $"Temperature {number}", propertyValue, isMetric);
                case "leafwetness_ch1":
                case "leafwetness_ch2":
                case "leafwetness_ch3":
                case "leafwetness_ch4":
                case "leafwetness_ch5":
                case "leafwetness_ch6":
                case "leafwetness_ch7":
                case "leafwetness_ch8":
                    // maybe it's double, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildIntSensor(propertyName, $"Leaf Wetness {number}", propertyValue, "%");
                case "console_batt":
                    // maybe it's voltage, I don't have this sensor
                    return BuildBatterySensor(propertyName, "Console Battery", propertyValue);
                case "wh65batt":
                    // maybe it's voltage, I don't have this sensor
                    return BuildBatterySensor(propertyName, "WH65 Battery", propertyValue);
                case "wh80batt":
                    // maybe it's voltage, I don't have this sensor
                    return BuildBatterySensor(propertyName, "WH80 Battery", propertyValue);
                case "wh26batt":
                    // maybe it's voltage, I don't have this sensor
                    return BuildBatterySensor(propertyName, "WH26 Battery", propertyValue);
                case "batt1":
                case "batt2":
                case "batt3":
                case "batt4":
                case "batt5":
                case "batt6":
                case "batt7":
                case "batt8":
                    // maybe it's voltage, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildBatterySensor(propertyName, $"Battery {number}", propertyValue);
                    //return BuildVoltageSensor(propertyName, $"Battery {number}", propertyValue, isDiag: true);
                case "soilbatt1":
                case "soilbatt2":
                case "soilbatt3":
                case "soilbatt4":
                case "soilbatt5":
                case "soilbatt6":
                case "soilbatt7":
                case "soilbatt8":
                    number = GetNumber(propertyName);
                    return BuildVoltageSensor(propertyName, $"Soil Battery {number}", propertyValue, isDiag: true);
                case "pm25batt1":
                case "pm25batt2":
                case "pm25batt3":
                case "pm25batt4":
                    // maybe it's voltage, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildBatterySensor(propertyName, $"PM2.5 Battery {number}", propertyValue);
                case "wh57batt":
                    return BuildBatterySensor(propertyName, "WH57 Battery", propertyValue, true);
                case "leakbatt1":
                case "leakbatt2":
                case "leakbatt3":
                case "leakbatt4":
                    // maybe it's voltage, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildBatterySensor(propertyName, $"Leak Battery {number}", propertyValue);
                case "tf_batt1":
                case "tf_batt2":
                case "tf_batt3":
                case "tf_batt4":
                case "tf_batt5":
                case "tf_batt6":
                case "tf_batt7":
                case "tf_batt8":
                    // maybe it's voltage, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildBatterySensor(propertyName, $"Temperature Battery {number}", propertyValue);
                case "co2_batt":
                    return BuildBatterySensor(propertyName, "CO2 Battery", propertyValue, true);
                case "leaf_batt1":
                case "leaf_batt2":
                case "leaf_batt3":
                case "leaf_batt4":
                case "leaf_batt5":
                case "leaf_batt6":
                case "leaf_batt7":
                case "leaf_batt8":
                    // maybe it's voltage, I don't have this sensor
                    number = GetNumber(propertyName);
                    return BuildBatterySensor(propertyName, $"Leaf Battery {number}", propertyValue);
                case "wh90batt":
                    return BuildVoltageSensor(propertyName, "WH90 Battery", propertyValue, true);
                case "ac_status":
                    return BuildBinarySensor(propertyName, "AC Status", propertyValue);
                case "ac_running":
                    return BuildBinarySensor(propertyName, "Running", propertyValue);
                case "warning":
                    return BuildBinarySensor(propertyName, "Warning", propertyValue);
                case "always_on":
                    return BuildBinarySensor(propertyName, "Always On", propertyValue);
                case "val_type":
                    return BuildIntSensor(propertyName, "ConfigValue Type", propertyValue, isDiag: true);
                case "val":
                    return BuildIntSensor(propertyName, "ConfigValue", propertyValue, isDiag: true);
                case "run_time":
                    return BuildIntSensor(propertyName, "Runtime", propertyValue, "s", isDiag: true);
                case "gw_rssi":
                    return BuildIntSensor(propertyName, "RSSI", propertyValue, "dBm", SensorType.SignalStrength, true);
                case "ac_action":
                    return BuildIntSensor(propertyName, "AC Action", propertyValue, isDiag: true);
                case "plan_status":
                    return BuildIntSensor(propertyName, "Plan Status", propertyValue, isDiag: true);
                case "elect_total":
                    return BuildConsumptionSensor(propertyName, "Total Consumption", propertyValue, true);
                case "happen_elect":
                    return BuildConsumptionSensor(propertyName, "Last Planned Consumption", propertyValue);
                case "realtime_power":
                    return BuildPowerSensor(propertyName, "Realtime Power", propertyValue);
                case "ac_voltage":
                    return BuildVoltageSensor(propertyName, "AC Voltage", propertyValue);
                case "ac_current":
                    return BuildCurrentSensor(propertyName, "AC Current", propertyValue);
                case "water_status":
                    return BuildBinarySensor(propertyName, "Water Status", propertyValue);
                case "water_action":
                    return BuildIntSensor(propertyName, "Water Action", propertyValue, isDiag: true);
                case "water_running":
                    return BuildBinarySensor(propertyName, "Running", propertyValue);
                case "water_total":
                    return BuildWaterConsumptionSensor(propertyName, "Total Water", propertyValue, isMetric, true);
                case "happen_water":
                    //new Sensor<double?>("Daily Consumption", isMetric ? (double?)device.water_total - (double?)device.happen_water : L2G(device.water_total) - L2G(device.happen_water), isMetric ? "L" : "gal", SensorType.Volume, SensorState.Measurement));
                    return BuildWaterConsumptionSensor(propertyName, "Last Planned Consumption", propertyValue, isMetric);
                case "flow_velocity":
                    return BuildWaterFlowSensor(propertyName, "Flow Velocity", propertyValue, isMetric);
                case "water_temp":
                    return BuildTemperatureSensor(propertyName, "Water Temperature", propertyValue, isMetric, true);
                case "wfc01batt":
                    return BuildBatterySensor(propertyName, "WFC01 Battery", propertyValue, true);
                case "heap":
                    return BuildIntSensor(propertyName, "Gateway Heap", propertyValue, "byte", isDiag: true);
                case "PASSKEY":
                case "stationtype":
                case "runtime":
                case "dateutc":
                case "freq":
                case "model":
                case "ws90_ver":
                case "interval":
                case "rssi":
                case "timeutc":
                case "publish_time":
                case "id":
                case "nickname":
                case "devicename":
                case "version":
                default:
                    Log.Information($"Ignored property {propertyName}.");
                    return null;
            }
        }

        
    }


}
