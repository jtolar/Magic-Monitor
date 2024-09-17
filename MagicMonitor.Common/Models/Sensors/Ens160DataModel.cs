namespace MagicMonitor.Common.Models.Sensors
{
    public class Ens160DataModel
    {
        public Ens160DataModel()
        {

        }

        public double CO2Concentration { get; set; }
        public double EthanolConcentration { get; set; }
        public double AirQualityIndex { get; set; }
        public double TemperatureFahrenheit { get; set; }
        public double HumidityPercent { get; set; }

    }
}