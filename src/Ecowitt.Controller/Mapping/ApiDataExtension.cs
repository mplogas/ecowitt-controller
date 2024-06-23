using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Mapping;

public static class ApiDataExtension
{
    public static Gateway Map(this ApiData apiData, bool isMetric = true)
    {
        // TODO: check other apidata properties for freedom units
        return new Gateway
        {
            PASSKEY = apiData.PASSKEY,
            Model = apiData.Model,
            StationType = apiData.StationType,
            Runtime = apiData.Runtime,
            Freq = apiData.Freq,
            DateUtc = apiData.DateUtc,
            IpAddress = apiData.IpAddress ?? string.Empty,
            TimestampUtc = apiData.TimestampUtc,
            TempIndoor = !isMetric ? apiData.TempInf : F2C(apiData.TempInf),
            HumidityIndoor = apiData.HumidityIn,
            BaromRelativeIndoor = apiData.BaromRelIn,
            BaromAbsoluteIndoor = apiData.BaromAbsIn,
            Temp = !isMetric ? apiData.TempF : F2C(apiData.TempF),
            Humidity = apiData.Humidity,
            WindDir = apiData.WindDir,
            WindSpeed = !isMetric ? apiData.WindSpeedMph : M2K(apiData.WindSpeedMph),
            WindGust = !isMetric ? apiData.WindGustMph : M2K(apiData.WindGustMph),
            MaxDailyGust = !isMetric ? apiData.MaxDailyGust : M2K(apiData.MaxDailyGust),
            SolarRadiation = apiData.SolarRadiation,
            Uv = apiData.Uv,
            RainRate = !isMetric ? apiData.RainRateIn : I2M(apiData.RainRateIn),
            EventRain = !isMetric ? apiData.EventRainIn : I2M(apiData.EventRainIn),
            HourlyRain = !isMetric ? apiData.HourlyRainIn : I2M(apiData.HourlyRainIn),
            DailyRain = !isMetric ? apiData.DailyRainIn : I2M(apiData.DailyRainIn),
            WeeklyRain = !isMetric ? apiData.WeeklyRainIn : I2M(apiData.WeeklyRainIn),
            MonthlyRain = !isMetric ? apiData.MonthlyRainIn : I2M(apiData.MonthlyRainIn),
            YearlyRain = !isMetric ? apiData.YearlyRainIn : I2M(apiData.YearlyRainIn),
            TotalRain = !isMetric ? apiData.TotalRainIn : I2M(apiData.TotalRainIn),
            Temp1 = !isMetric ? apiData.Temp1F : F2C(apiData.Temp1F),
            Humidity1 = apiData.Humidity1,
            Temp2 = !isMetric ? apiData.Temp2F : F2C(apiData.Temp2F),
            Humidity2 = apiData.Humidity2,
            Temp3 = !isMetric ? apiData.Temp3F : F2C(apiData.Temp3F),
            Humidity3 = apiData.Humidity3,
            Temp4 = !isMetric ? apiData.Temp4F : F2C(apiData.Temp4F ),
            Humidity4 = apiData.Humidity4,
            Temp5 = !isMetric ? apiData.Temp5F : F2C(apiData.Temp5F),
            Humidity5 = apiData.Humidity5,
            Temp6 = !isMetric ? apiData.Temp6F : F2C(apiData.Temp6F),
            Humidity6 = apiData.Humidity6,
            Temp7 = !isMetric ? apiData.Temp7F : F2C(apiData.Temp7F),
            Humidity7 = apiData.Humidity7,
            Temp8 = !isMetric ? apiData.Temp8F : F2C(apiData.Temp8F),
            Humidity8 = apiData.Humidity8,
            SoilMoisture1 = apiData.SoilMoisture1,
            SoilAd1 = apiData.SoilAd1,
            SoilMoisture2 = apiData.SoilMoisture2,
            SoilAd2 = apiData.SoilAd2,
            SoilMoisture3 = apiData.SoilMoisture3,
            SoilAd3 = apiData.SoilAd3,
            SoilMoisture4 = apiData.SoilMoisture4,
            SoilAd4 = apiData.SoilAd4,
            SoilMoisture5 = apiData.SoilMoisture5,
            SoilAd5 = apiData.SoilAd5,
            SoilMoisture6 = apiData.SoilMoisture6,
            SoilAd6 = apiData.SoilAd6,
            SoilMoisture7 = apiData.SoilMoisture7,
            SoilAd7 = apiData.SoilAd7,
            SoilMoisture8 = apiData.SoilMoisture8,
            SoilAd8 = apiData.SoilAd8,
            Pm25Ch1 = apiData.Pm25Ch1,
            Pm25Avg24hCh1 = apiData.Pm25Avg24hCh1,
            Pm25Ch2 = apiData.Pm25Ch2,
            Pm25Avg24hCh2 = apiData.Pm25Avg24hCh2,
            Pm25Ch3 = apiData.Pm25Ch3,
            Pm25Avg24hCh3 = apiData.Pm25Avg24hCh3,
            Pm25Ch4 = apiData.Pm25Ch4,
            Pm25Avg24hCh4 = apiData.Pm25Avg24hCh4,
            TfCo2 = apiData.TfCo2,
            HumiCo2 = apiData.HumiCo2,
            Pm25Co2 = apiData.Pm25Co2,
            Pm2524hCo2 = apiData.Pm2524hCo2,
            Pm10Co2 = apiData.Pm10Co2,
            Pm1024hCo2 = apiData.Pm1024hCo2,
            Co2 = apiData.Co2,
            Co224h = apiData.Co224h,
            LightningNum = apiData.LightningNum,
            Lightning = apiData.Lightning,
            LightningTime = apiData.LightningTime,
            LeakCh2 = apiData.LeakCh2,
            TfCh1 = apiData.TfCh1,
            TfCh2 = apiData.TfCh2,
            LeafWetnessCh1 = apiData.LeafWetnessCh1,
            ConsoleBatt = apiData.ConsoleBatt,
            Wh65batt = apiData.Wh65batt,
            Wh80batt = apiData.Wh80batt,
            Wh26batt = apiData.Wh26batt,
            Batt1 = apiData.Batt1,
            Batt2 = apiData.Batt2,
            Batt3 = apiData.Batt3,
            Batt4 = apiData.Batt4,
            Batt5 = apiData.Batt5,
            Batt6 = apiData.Batt6,
            Batt7 = apiData.Batt7,
            Batt8 = apiData.Batt8,
            SoilBatt1 = apiData.SoilBatt1,
            SoilBatt2 = apiData.SoilBatt2,
            SoilBatt3 = apiData.SoilBatt3,
            SoilBatt4 = apiData.SoilBatt4,
            SoilBatt5 = apiData.SoilBatt5,
            SoilBatt6 = apiData.SoilBatt6,
            SoilBatt7 = apiData.SoilBatt7,
            SoilBatt8 = apiData.SoilBatt8,
            Pm25Batt1 = apiData.Pm25Batt1,
            Pm25Batt2 = apiData.Pm25Batt2,
            Pm25Batt3 = apiData.Pm25Batt3,
            Pm25Batt4 = apiData.Pm25Batt4,
            Wh57Batt = apiData.Wh57Batt,
            LeakBatt2 = apiData.LeakBatt2,
            TfBatt1 = apiData.TfBatt1,
            TfBatt2 = apiData.TfBatt2,
            Co2Batt = apiData.Co2Batt,
            LeafBatt1 = apiData.LeafBatt1
        };
    }
    
    private static double? F2C(double? fahrenheit)
    {
        return (fahrenheit - 32) * 5 / 9;
    }
    
    private static double? M2K(double? mph)
    {
        return mph * 1.60934;
    }
    
    private static double? I2M(double? inches)
    {
        return inches * 25.4;
    }
}