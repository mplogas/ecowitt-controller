namespace Ecowitt.Controller.Model.Api;

public class GatewayApiData
{
    public string PASSKEY { get; set; } = string.Empty;
    public string StationType { get; set; } = string.Empty;
    public int Runtime { get; set; }
    public DateTime DateUtc { get; set; }
    public string Freq { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    //PASSKEY, stationtype, runtime, dateutc, tempinf, humidityin, baromrelin, baromabsin, tf_co2, humi_co2, pm25_co2, pm25_24h_co2, pm10_co2, pm10_24h_co2, co2, co2_24h, co2_batt, freq, model
    
    // Request form keys: PASSKEY, stationtype, runtime, heap, dateutc, tempinf, humidityin, baromrelin, baromabsin, tempf, humidity, winddir, windspeedmph, windgustmph, maxdailygust, solarradiation,
    // uv, rrain_piezo, erain_piezo, hrain_piezo, drain_piezo, wrain_piezo, mrain_piezo, yrain_piezo, ws90cap_volt, ws90_ver, soilmoisture1..16, soilad1..16, soilbatt1..16,
    // lightning_num, lightning, lightning_time, wh57batt, wh90batt, freq, model, interval

    public string Payload { get; set; } = string.Empty;
}