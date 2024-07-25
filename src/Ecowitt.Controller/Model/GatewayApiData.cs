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


    public double? TempInf { get; set; }
    public int? HumidityIn { get; set; }
    public double? BaromRelIn { get; set; }
    public double? BaromAbsIn { get; set; }
    public double? TempF { get; set; }
    public int? Humidity { get; set; }
    public int? WindDir { get; set; }
    public double? WindSpeedMph { get; set; }
    public double? WindGustMph { get; set; }
    public double? MaxDailyGust { get; set; }
    public double? SolarRadiation { get; set; }
    public int? Uv { get; set; }
    public double? RainRateIn { get; set; }
    public double? EventRainIn { get; set; }
    public double? HourlyRainIn { get; set; }
    public double? DailyRainIn { get; set; }
    public double? WeeklyRainIn { get; set; }
    public double? MonthlyRainIn { get; set; }
    public double? YearlyRainIn { get; set; }
    public double? TotalRainIn { get; set; }
    public double? Temp1F { get; set; }
    public int? Humidity1 { get; set; }
    public double? Temp2F { get; set; }
    public int? Humidity2 { get; set; }
    public double? Temp3F { get; set; }
    public int? Humidity3 { get; set; }
    public double? Temp4F { get; set; }
    public int? Humidity4 { get; set; }
    public int? Humidity5 { get; set; }
    public double? Temp5F { get; set; }
    public double? Temp6F { get; set; }
    public int? Humidity6 { get; set; }
    public double? Temp7F { get; set; }
    public int? Humidity7 { get; set; }
    public double? Temp8F { get; set; }
    public int? Humidity8 { get; set; }
    public int? SoilMoisture1 { get; set; }
    public int? SoilAd1 { get; set; }
    public int? SoilMoisture2 { get; set; }
    public int? SoilAd2 { get; set; }
    public int? SoilMoisture3 { get; set; }
    public int? SoilAd3 { get; set; }
    public int? SoilMoisture4 { get; set; }
    public int? SoilAd4 { get; set; }
    public int? SoilMoisture5 { get; set; }
    public int? SoilAd5 { get; set; }
    public int? SoilMoisture6 { get; set; }
    public int? SoilAd6 { get; set; }
    public int? SoilMoisture7 { get; set; }
    public int? SoilAd7 { get; set; }
    public int? SoilMoisture8 { get; set; }
    public int? SoilAd8 { get; set; }
    public double? Pm25Ch1 { get; set; }
    public double? Pm25Avg24hCh1 { get; set; }
    public double? Pm25Ch2 { get; set; }
    public double? Pm25Avg24hCh2 { get; set; }
    public double? Pm25Ch3 { get; set; }
    public double? Pm25Avg24hCh3 { get; set; }
    public double? Pm25Ch4 { get; set; }
    public double? Pm25Avg24hCh4 { get; set; }
    public double? Tf_Co2 { get; set; }
    public int? Humi_Co2 { get; set; }
    public double? Pm25_Co2 { get; set; }
    public double? Pm25_24h_Co2 { get; set; }
    public double? Pm10_Co2 { get; set; }
    public double? Pm10_24h_Co2 { get; set; }
    public int? Co2 { get; set; }
    public int? Co2_24h { get; set; }
    public int? LightningNum { get; set; }
    public string? Lightning { get; set; } // Assuming this might be a string representation of lightning data
    public string? LightningTime { get; set; } // Assuming this might be a string representation of time
    public int? LeakCh2 { get; set; }
    public double? TfCh1 { get; set; }
    public double? TfCh2 { get; set; }
    public int? LeafWetnessCh1 { get; set; }
    public double? ConsoleBatt { get; set; }
    public int? Wh65batt { get; set; }
    public double? Wh80batt { get; set; }
    public int? Wh26batt { get; set; }
    public int? Batt1 { get; set; }
    public int? Batt2 { get; set; }
    public int? Batt3 { get; set; }
    public int? Batt4 { get; set; }
    public int? Batt5 { get; set; }
    public int? Batt6 { get; set; }
    public int? Batt7 { get; set; }
    public int? Batt8 { get; set; }
    public double? SoilBatt1 { get; set; }
    public double? SoilBatt2 { get; set; }
    public double? SoilBatt3 { get; set; }
    public double? SoilBatt4 { get; set; }
    public double? SoilBatt5 { get; set; }
    public double? SoilBatt6 { get; set; }
    public double? SoilBatt7 { get; set; }
    public double? SoilBatt8 { get; set; }
    public int? Pm25Batt1 { get; set; }
    public int? Pm25Batt2 { get; set; }
    public int? Pm25Batt3 { get; set; }
    public int? Pm25Batt4 { get; set; }
    public int? Wh57Batt { get; set; }
    public int? LeakBatt2 { get; set; }
    public double? TfBatt1 { get; set; }
    public double? TfBatt2 { get; set; }
    public int? Co2_Batt { get; set; }
    public double? LeafBatt1 { get; set; }
}