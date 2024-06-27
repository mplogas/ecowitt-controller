namespace Ecowitt.Controller.Model;

public class Gateway
{
    // important properties
    public string IpAddress { get; set; }
    public string Name { get; set; }
    public DateTime TimestampUtc { get; set; }
    public List<Subdevice> Subdevices { get; set; } = new();
    
    // system properties
    public string? Model { get; set; } 
    public string? PASSKEY { get; set; } 
    public string? StationType { get; set; } 
    public int? Runtime { get; set; }
    public DateTime? DateUtc { get; set; }
    public string? Freq { get; set; }

    // sensors
    public double? TempIndoor { get; set; }
    public int? HumidityIndoor { get; set; }
    public double? BaromRelativeIndoor { get; set; }
    public double? BaromAbsoluteIndoor { get; set; }
    public double? Temp { get; set; }
    public int? Humidity { get; set; }
    public int? WindDir { get; set; }
    public double? WindSpeed { get; set; }
    public double? WindGust { get; set; }
    public double? MaxDailyGust { get; set; }
    public double? SolarRadiation { get; set; }
    public int? Uv { get; set; }
    public double? RainRate { get; set; }
    public double? EventRain { get; set; }
    public double? HourlyRain { get; set; }
    public double? DailyRain { get; set; }
    public double? WeeklyRain { get; set; }
    public double? MonthlyRain { get; set; }
    public double? YearlyRain { get; set; }
    public double? TotalRain { get; set; }
    public double? Temp1 { get; set; }
    public int? Humidity1 { get; set; }
    public double? Temp2 { get; set; }
    public int? Humidity2 { get; set; }
    public double? Temp3 { get; set; }
    public int? Humidity3 { get; set; }
    public double? Temp4 { get; set; }
    public int? Humidity4 { get; set; }
    public double? Temp5 { get; set; }
    public int? Humidity5 { get; set; }
    public double? Temp6 { get; set; }
    public int? Humidity6 { get; set; }
    public double? Temp7 { get; set; }
    public int? Humidity7 { get; set; }
    public double? Temp8 { get; set; }
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
    public double? TfCo2 { get; set; }
    public int? HumiCo2 { get; set; }
    public double? Pm25Co2 { get; set; }
    public double? Pm2524hCo2 { get; set; }
    public double? Pm10Co2 { get; set; }
    public double? Pm1024hCo2 { get; set; }
    public int? Co2 { get; set; }
    public int? Co224h { get; set; }
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
    public int? Co2Batt { get; set; }
    public double? LeafBatt1 { get; set; }
}