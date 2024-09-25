using MagicMonitor.Device.Remote.Services;
using MagicMonitor.Sensors.Athxx;
using MagicMonitor.Sensors.Bmxx80;
using MagicMonitor.Sensors.Ens160;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Runtime.Native;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;
using UnitsNet.Units;
using DeviceStatus = MagicMonitor.Sensors.Ens160.DeviceStatus;

namespace MagicMonitor.Device.Remote
{
    public class Program
    {

        private static GpioController gpioController = new GpioController();
        private static EnvironmentalSensorService EnvService;
        private static Ws2808 neo = new Ws2808(Gpio.IO08, 1);
        private static DeviceStatus ensDeviceStatus;
        private static OperatingMode ensMode;
        private static GpioPin fan;
        private static bool isFanOn = false;

        private static int iTempOut;
        private static int iTempOutMax;
        private static int iTempOutMin;
        private static int iHumidity;
        private static int iCo2Out;

        public static void Main()
        {
            fan = gpioController.OpenPin(Gpio.IO14, PinMode.Output);
            //fan.Write(PinValue.High);

            SetLedColor(Color.Black);
            Configuration.SetPinFunction(Gpio.IO06, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO07, DeviceFunction.I2C1_CLOCK);
            //Configuration.SetPinFunction(Gpio.IO08, DeviceFunction.PWM1);
            
            I2cConnectionSettings bme280Settings = new(1, Bme280.SecondaryI2cAddress, I2cBusSpeed.FastMode);
            using var bme280Device = I2cDevice.Create(bme280Settings);

            I2cConnectionSettings ens160Settings = new(1, Ens160.SecondaryI2cAddress, I2cBusSpeed.FastMode);
            using var ens160Device = I2cDevice.Create(ens160Settings);

            //using var aht20 = new Aht20(aht20Device);
            using var ens160 = new Ens160(ens160Device);
            using var bme280 = new Bme280(bme280Device);

            ensDeviceStatus = ens160.GetDeviceStatus();
            ensMode = ens160.CurrentOperatingMode;
            while (ensDeviceStatus != DeviceStatus.Normal || ensMode != OperatingMode.Standard)
            {
                Debug.WriteLine($"Waiting for Ens160 to enter Normal Status. Currently {ens160.GetDeviceStatusName(ensDeviceStatus)}");
                Debug.WriteLine($"Waiting for Ens160 to enter Standard Operation. Currently {ens160.GetOperatingModeName(ensMode)}");
                Thread.Sleep(TimeSpan.FromSeconds(15).Milliseconds);
                ensMode = ens160.CurrentOperatingMode;
                ensDeviceStatus = ens160.GetDeviceStatus();
                SetDeviceStatusRgb(ensDeviceStatus);
            }
            
            Debug.WriteLine($"Device Status: {ensDeviceStatus}");

            if (!SetupBluetoothServices(out var serviceProvider)) return;

            var initRead = bme280.Read();
            ens160.CurrentOperatingMode = OperatingMode.Idle;
            Thread.Sleep(100);

            ens160.SetCompensationValues(initRead.Temperature, initRead.Humidity);
            ens160.CurrentOperatingMode = OperatingMode.Standard;
            ens160.Read();

            WriteSensorDebug(bme280, ens160);

            #region Start Advertising
            //Once all the Characteristics/ Services have been created you need to advertise so
            // other devices can see it.Here we also say the device can be connected too and other
            // devices can see it with a specific device name.
            serviceProvider.StartAdvertising(new GattServiceProviderAdvertisingParameters()
            {
                IsConnectable = true,
                IsDiscoverable = true
            });
            #endregion 

            Thread.Sleep(TimeSpan.FromSeconds(15).Milliseconds);

            // Update values after 1 min. to simulate real sensors
            while (true)
            {
                ens160.SetCompensationValues(initRead.Temperature, initRead.Humidity);
                WriteSensorDebug(bme280, ens160);

                //float t1 = 23.4F;
                //float t3 = 7.5F;

                Thread.Sleep(15000);
                // Move temperatures up
                //while (t1 < 120)
                //{
                //    t1 += 1.3F;
                //    t3 += 2.1F;

                //    EnvService.UpdateValue(iTempOut, t1);
                //    EnvService.UpdateValue(iTempOutMin, t3);
                //    Thread.Sleep(5000);
                //}

                //// Move temperatures down
                //while (t1 > -50F)
                //{
                //    t1 -= 1.3F;
                //    t3 -= 2.1F;

                //    EnvService.UpdateValue(iTempOut, t1);
                //    EnvService.UpdateValue(iTempOutMin, t3);
                //    Thread.Sleep(5000);
                //}
            }
        }

        private static void SetLedColor(Color color)
        {
            var pixel = neo.Image;
            pixel.SetPixel(0, 0, color.G, color.R, color.B);
            neo.Update();
        }

        private static void WriteSensorDebug(Bme280 bme280, Ens160 ens160, Aht20? aht20 = null)
        {
            fan.Write(isFanOn ? PinValue.Low : PinValue.High);
            isFanOn = !isFanOn;
            Debug.WriteLine($"Fan {(isFanOn ? "On" : "Off")}");

            Debug.WriteLine();
            var bme280Read = bme280.Read();

            Debug.WriteLine("--------------BME280 Sensor-------------------");
            Debug.WriteLine($"Temperature: {bme280Read.Temperature.DegreesFahrenheit}F / {bme280Read.Temperature.DegreesCelsius}C");
            Debug.WriteLine($"Pressure: {bme280Read.Pressure.Hectopascals}hPa");
            Debug.WriteLine($"Relative humidity: {bme280Read.Humidity.Percent}%");
            Debug.WriteLine();

            if (aht20 != null)
            {
                Debug.WriteLine("---------------AHT20 Sensor-------------------");
                Debug.WriteLine($"Temperature: {aht20.GetTemperature().As(TemperatureUnit.DegreeFahrenheit)}F");
                Debug.WriteLine($"Relative humidity: {aht20.GetHumidity().Percent}%");
                Debug.WriteLine();
            }

            ensDeviceStatus = ens160.GetDeviceStatus();
            SetDeviceStatusRgb(ensDeviceStatus);

            ens160.SetCompensationValues(bme280Read.Temperature, bme280Read.Humidity);
            ens160.Read();
            Debug.WriteLine("--------------ENS160 Sensor-------------------");
            Debug.WriteLine($"Temp Used In Calculations: {ens160.Temperature.DegreesFahrenheit}F - {ens160.Temperature.DegreesCelsius}C - {ens160.Temperature.Kelvins}");
            Debug.WriteLine($"Humidty Used In Calculations: {ens160.RelativeHumidity.Percent}%");
            Debug.WriteLine($"Current OP Mode: {ens160.GetOperatingModeName(ens160.CurrentOperatingMode)}");
            Debug.WriteLine($"Current Status: {ens160.GetDeviceStatusName(ensDeviceStatus)}");
            Debug.WriteLine($"CO2 Concentration: {ens160.Co2Concentration.PartsPerMillion}ppm");
            Debug.WriteLine($"TVOC Concentration: {ens160.TvocConcentration.PartsPerMillion}ppm");
            //Debug.WriteLine($"Ethanol Concentration: {ens160.EthanolConcentration.PartsPerBillion}ppb");
            Debug.WriteLine($"AirQualityIndex: {ens160.AirQualityIndex}");
            Debug.WriteLine("---------------------------------------------");
            Debug.WriteLine();

            // Update sensor values, these would need to be updated every time sensors are read. 
            EnvService.UpdateValue(iTempOut, (float)bme280Read.Temperature.DegreesFahrenheit);
            EnvService.UpdateValue(iTempOutMax, (float) bme280Read.Temperature.DegreesFahrenheit);
            EnvService.UpdateValue(iTempOutMin, (float) bme280Read.Temperature.DegreesFahrenheit);
            EnvService.UpdateValue(iHumidity, (float) bme280Read.Humidity.Percent);
            EnvService.UpdateValue(iCo2Out, (float) ens160.Co2Concentration.PartsPerMillion);
        }

        private static void SetDeviceStatusRgb(DeviceStatus status)
        {
            if (status == DeviceStatus.InitialStartup)
                SetLedColor(Color.Yellow);

            if (status == DeviceStatus.Invalid)
                SetLedColor(Color.Crimson);

            if (status == DeviceStatus.Normal)
                SetLedColor(Color.DarkBlue);

            if (status == DeviceStatus.WarmUp)
                SetLedColor(Color.DeepPink);
        }

        private static bool SetupBluetoothServices(out GattServiceProvider serviceProvider)
        {
            // Define used custom Uuid 
            Guid serviceUuid = new("A7EEDF2C-DA87-4CB5-A9C5-5151C78B0057");
            Guid readStaticCharUuid = new("A7EEDF2C-DA89-4CB5-A9C5-5151C78B0057");

            // BluetoothLEServer is a singleton object so gets its instance. The Object is created when you first access it
            // and can be disposed to free up memory.
            BluetoothLEServer server = BluetoothLEServer.Instance;

            // Give device a name
            server.DeviceName = $"MM-REMOTE-";


            //The GattServiceProvider is used to create and advertise the primary service definition.
            //An extra device information service will be automatically created.
            GattServiceProviderResult result = GattServiceProvider.Create(serviceUuid);
            if (result.Error != BluetoothError.Success)
            {
                serviceProvider = null;
                return false;
            }

            serviceProvider = result.ServiceProvider;

            // Get created Primary service from provider
            GattLocalService service = serviceProvider.Service;

            #region Static read characteristic
            // Now we add an characteristic to service
            // If the read value is not going to change then you can just use a Static value
            DataWriter sw = new();
            sw.WriteString("This is Bluetooth sample 3");

            GattLocalCharacteristicResult characteristicResult = service.CreateCharacteristic(readStaticCharUuid,
                new GattLocalCharacteristicParameters()
                {
                    CharacteristicProperties = GattCharacteristicProperties.Read,
                    UserDescription = "My Static Characteristic",
                    StaticValue = sw.DetachBuffer()
                });
            ;

            if (characteristicResult.Error != BluetoothError.Success)
            {
                // An error occurred.
                return false;
            }
            #endregion

            // Add standard Bluetooth Sig services to the provider. These are an example of standard services
            // that can be reused/updated for other applications. Based on standards but simplified. 

            // === Device Information Service ===
            // https://www.bluetooth.com/specifications/specs/device-information-service-1-1/
            // The Device Information Service is created automatically when you create the initial primary service.
            // The default version just has a Manufacturer name of "nanoFramework and model or "Esp32"
            // You can add your own service which will replace the default one.
            // To make it easy we have included some standard services classes to this sample
            DeviceInformationServiceService DifService = new(
                "Southern Harmony",
                "MM-R1",
                null, // no serial number
                "v1.0",
                SystemInfo.Version.ToString(),
                "");

            // === Battery Service ===
            // https://www.bluetooth.com/specifications/specs/battery-service-1-0/
            // Battery service exposes the current battery level percentage
            BatteryService BatService = new();

            // Update the Battery service the current battery level regularly. In this case 94%
            BatService.BatteryLevel = 100;

            // === Current Time Service ===
            // https://www.bluetooth.com/specifications/specs/current-time-service-1-1/
            // The Current Time Service exposes the device current date/time and also 
            // optionally allows the date time to be updated. You can call the Notify method to inform
            // any connected devices of changed in date/time.  Any subscribed clients will be automatically 
            // be notified every 60 seconds.
            CurrentTimeService CtService = new(true);

            // === Environmental Sensor Service ===
            // https://www.bluetooth.com/specifications/specs/environmental-sensing-service-1-0/
            // This service exposes measurement data from an environmental sensors.
            EnvService = new();

            // Add sensors to service, return index so sensor can be updated later.
            iTempOut = EnvService.AddSensor(EnvironmentalSensorService.SensorType.Temperature, "Temp");
            iTempOutMax = EnvService.AddSensor(EnvironmentalSensorService.SensorType.Temperature, "Max Temp", EnvironmentalSensorService.Sampling.Maximum);
            iTempOutMin = EnvService.AddSensor(EnvironmentalSensorService.SensorType.Temperature, "Min Temp", EnvironmentalSensorService.Sampling.Minimum);
            iHumidity = EnvService.AddSensor(EnvironmentalSensorService.SensorType.Humidity, "Humidty");
            iCo2Out = EnvService.AddSensor(EnvironmentalSensorService.SensorType.Co2, "CO2");
            
            return true;
        }

        public static void ScanI2cBus(I2cConnectionSettings deviceConnectionSettings)
        {
            Debug.WriteLine("Hello from I2C Scanner!");
            SpanByte span = new byte[1];
            bool isDevice;
            // On a normal bus, not all those ranges are supported but scanning anyway
            for (int i = 0; i <= 0xFF; i++)
            {
                isDevice = false;
                I2cDevice i2c = new(deviceConnectionSettings);
                // What we write is not important
                var res = i2c.WriteByte(0x07);
                // A successfull write will be: 0x10 Write: 1, transferred: 1
                // A non successful one: 0x0F Write: 4, transferred: 0
                Debug.Write($"0x{i:X2} Write: {StatusName(res.Status)}, transferred: {res.BytesTransferred}");
                isDevice = res.Status == I2cTransferStatus.FullTransfer;

                // What we read doesn't matter, reading only 1 element is what's needed
                res = i2c.Read(span);
                // A successfull write will be: Read: 1, transferred: 1
                // A non successfull one: Read: 2, transferred: 0
                Debug.WriteLine($", Read: {StatusName(res.Status)}, transferred: {res.BytesTransferred}");

                // For most devices, success should be when you can write and read
                // Now, this can be adjusted with just read or write depending on the
                // device you are looking for
                isDevice &= res.Status == I2cTransferStatus.FullTransfer;
                Debug.WriteLine($"0x{i:X2} - {(isDevice ? "Present" : "Absent")}");

                // Just force to dispose so we can use the next one
                i2c.Dispose();
            }
        }

        private static string StatusName(I2cTransferStatus value)
        {
            switch (value)
            {
                case I2cTransferStatus.UnknownError:
                    return "Unknown Error";
                case I2cTransferStatus.FullTransfer:
                    return "FullTransfer";
                case I2cTransferStatus.ClockStretchTimeout:
                    return "ClockStretchTimeout";
                case I2cTransferStatus.PartialTransfer:
                    return "PartialTransfer";
                case I2cTransferStatus.SlaveAddressNotAcknowledged:
                    return "SlaveAddressNotAcknowledged";
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
