using System;

namespace Ecowitt.Controller.Model
{
    public class ApiData
    {
        public string PASSKEY { get; set; }
        public string StationType { get; set; }
        public int Runtime { get; set; }
        public DateTime DateUtc { get; set; }
        public double TempInf { get; set; }
        public int HumidityIn { get; set; }
        public double BaromRelIn { get; set; }
        public double BaromAbsIn { get; set; }
        public double TempF { get; set; }
        public int Humidity { get; set; }
        public int WindDir { get; set; }
        public double WindSpeedMph { get; set; }
        public double WindGustMph { get; set; }
        public double MaxDailyGust { get; set; }
        public double SolarRadiation { get; set; }
        public int Uv { get; set; }
        public double RainRateIn { get; set; }
        public double EventRainIn { get; set; }
        public double HourlyRainIn { get; set; }
        public double DailyRainIn { get; set; }
        public double WeeklyRainIn { get; set; }
        public double MonthlyRainIn { get; set; }
        public double YearlyRainIn { get; set; }
        public double TotalRainIn { get; set; }
        public double Temp1F { get; set; }
        public int Humidity1 { get; set; }
        public double Temp2F { get; set; }
        public int Humidity2 { get; set; }
        public double Temp3F { get; set; }
        public int Humidity3 { get; set; }
        public double Temp4F { get; set; }
        public int Humidity4 { get; set; }
        public double Temp5F { get; set; }
        public double Temp6F { get; set; }
        public int Humidity6 { get; set; }
        public double Temp7F { get; set; }
        public int Humidity7 { get; set; }
        public int SoilMoisture1 { get; set; }
        public int SoilAd1 { get; set; }
        public int SoilMoisture2 { get; set; }
        public int SoilAd2 { get; set; }
        public int SoilMoisture3 { get; set; }
        public int SoilAd3 { get; set; }
        public int SoilMoisture4 { get; set; }
        public int SoilAd4 { get; set; }
        public int SoilMoisture5 { get; set; }
        public int SoilAd5 { get; set; }
        public int SoilMoisture6 { get; set; }
        public int SoilAd6 { get; set; }
        public double Pm25Ch1 { get; set; }
        public double Pm25Avg24hCh1 { get; set; }
        public double Pm25Ch2 { get; set; }
        public double Pm25Avg24hCh2 { get; set; }
        public double TfCo2 { get; set; }
        public int HumiCo2 { get; set; }
        public double Pm25Co2 { get; set; }
        public double Pm2524hCo2 { get; set; }
        public double Pm10Co2 { get; set; }
        public double Pm1024hCo2 { get; set; }
        public int Co2 { get; set; }
        public int Co224h { get; set; }
        public int LightningNum { get; set; }
        public string Lightning { get; set; } // Assuming this might be a string representation of lightning data
        public string LightningTime { get; set; } // Assuming this might be a string representation of time
        public int LeakCh2 { get; set; }
        public double TfCh1 { get; set; }
        public double TfCh2 { get; set; }
        public int LeafWetnessCh1 { get; set; }
        public double ConsoleBatt { get; set; }
        public int Wh65batt { get; set; }
        public double Wh80batt { get; set; }
        public int Wh26batt { get; set; }
        public int Batt1 { get; set; }
        public int Batt2 { get; set; }
        public int Batt3 { get; set; }
        public int Batt4 { get; set; }
        public int Batt5 { get; set; }
        public int Batt6 { get; set; }
        public int Batt7 { get; set; }
        public double SoilBatt1 { get; set; }
        public double SoilBatt2 { get; set; }
        public double SoilBatt3 { get; set; }
        public double SoilBatt4 { get; set; }
        public double SoilBatt5 { get; set; }
        public double SoilBatt6 { get; set; }
        public int Pm25Batt1 { get; set; }
        public int Pm25Batt2 { get; set; }
        public int Wh57Batt { get; set; }
        public int LeakBatt2 { get; set; }
        public double TfBatt1 { get; set; }
        public double TfBatt2 { get; set; }
        public int Co2Batt { get; set; }
        public double LeafBatt1 { get; set; }
        public string Freq { get; set; }
        public string Model { get; set; }
        public int Interval { get; set; }
    }
}
