// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace MagicMonitor.Sensors.Bmxx80.ReadResult
{
    /// <summary>
    /// Contains a measurement result of a Bme280 sensor.
    /// </summary>
    public class Bme280ReadResult : Bmp280ReadResult
    {
        /// <summary>
        /// Gets the collected humidity measurement.
        /// </summary>
        public RelativeHumidity Humidity { get; }

        /// <summary>
        /// Gets a value indicating whether last humidity value read was successful.
        /// </summary>
        public bool HumidityIsValid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bme280ReadResult" /> class.
        /// </summary>
        /// <param name="temperature">The <see cref="Temperature"/> measurement.</param>
        /// <param name="temperatureIsValid">Last temperature value read was successful.</param>
        /// <param name="pressure">The <see cref="Pressure"/> measurement.</param>
        /// <param name="pressureIsValid">Last pressure value read was successful.</param>
        /// <param name="humidity">The humidity measurement.</param>
        /// <param name="humidityIsValid">Last humidity value read was successful.</param>
        public Bme280ReadResult(Temperature temperature, bool temperatureIsValid, Pressure pressure, bool pressureIsValid, RelativeHumidity humidity, bool humidityIsValid)
            : base(temperature, temperatureIsValid, pressure, pressureIsValid)
        {
            Humidity = humidity;

            HumidityIsValid = humidityIsValid;
        }
    }
}
