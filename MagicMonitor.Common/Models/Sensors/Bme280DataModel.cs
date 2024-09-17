using System;

namespace MagicMonitor.Common.Models.Sensors
{
    public class Bme280DataModel
    {
        public Bme280DataModel()
        {

        }

        public double BarPressure { get; set; }
        public double TemperatureFahrenheit { get; set; }
        public double HumidityPercent { get; set; }
    }
}
