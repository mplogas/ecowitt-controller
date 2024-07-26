namespace Ecowitt.Controller.Model;

public class GatewayApiData
{
    public string PASSKEY { get; set; }
    public string StationType { get; set; }
    public int Runtime { get; set; }
    public DateTime DateUtc { get; set; }
    public string Freq { get; set; }
    public string Model { get; set; }
    public string? IpAddress { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    //PASSKEY, stationtype, runtime, dateutc, tempinf, humidityin, baromrelin, baromabsin, tf_co2, humi_co2, pm25_co2, pm25_24h_co2, pm10_co2, pm10_24h_co2, co2, co2_24h, co2_batt, freq, model
    
    // Request form keys: PASSKEY, stationtype, runtime, heap, dateutc, tempinf, humidityin, baromrelin, baromabsin, tempf, humidity, winddir, windspeedmph, windgustmph, maxdailygust, solarradiation,
    // uv, rrain_piezo, erain_piezo, hrain_piezo, drain_piezo, wrain_piezo, mrain_piezo, yrain_piezo, ws90cap_volt, ws90_ver, soilmoisture1, soilad1, soilmoisture2, soilad2, soilmoisture3, soilad3, soilmoisture4,
    // soilad4, soilmoisture5, soilad5, soilmoisture6, soilad6, soilmoisture7, soilad7, soilmoisture8, soilad8, lightning_num, lightning, lightning_time, soilbatt1, soilbatt2, soilbatt3, soilbatt4, soilbatt5, soilbatt6,
    // soilbatt7, soilbatt8, wh57batt, wh90batt, freq, model, interval

    public string Payload { get; set; } = string.Empty;
}