using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using System;
using System.Collections;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Bmxx80;

using System.Device.Spi;
using System.Device.I2c;
using System.Diagnostics;
using nanoFramework.UI;
using System.Device.Gpio;
using System.Drawing;
using Iot.Device.Ws28xx.Esp32;

namespace MagicMonitor.Device.Master
{
    public class Program
    {
        //public static void Main()
        //{
        //    const int backLightPin = Gpio.IO06;
        //    const int chipSelect = Gpio.IO07;
        //    const int dataCommand = Gpio.IO18;
        //    const int reset = -1; //Gpio.IO01;
        //    const int screenWidth = 320;
        //    const int screenHeight = 240;
        //    const int delayBetween = 3000;

        //    var gpioController = new GpioController();

        //    Debug.WriteLine("Setting up Display SPI Bus 2");

        //    try
        //    {
        //        Configuration.SetPinFunction(Gpio.IO03, DeviceFunction.SPI2_MISO);
        //        Configuration.SetPinFunction(Gpio.IO08, DeviceFunction.SPI2_MOSI);

        //        Configuration.SetPinFunction(47, DeviceFunction.SPI2_CLOCK);

        //        var displaySpiConfig = new SpiConfiguration(2, chipSelect, dataCommand, reset, -1);

        //        var graphicDriver = Ili9341.GraphicDriver;

        //        var screenConfig = new ScreenConfiguration(1, 1, screenWidth, screenHeight, graphicDriver);

        //        var bufferSize = (uint) (DisplayControl.ScreenWidth * DisplayControl.ScreenHeight * 3 / 8);

        //        DisplayControl.Initialize(displaySpiConfig, screenConfig, bufferSize);

        //        gpioController.OpenPin(backLightPin, PinMode.Output);
        //        gpioController.Write(backLightPin, PinValue.High);

        //        Debug.WriteLine($"init screen initialized");

        //        Debug.WriteLine($"Buffer Size is {bufferSize}, IsFullScreenBufferAvailable: {DisplayControl.IsFullScreenBufferAvailable}");
        //        Debug.WriteLine("Fullscreen Bitmap");
        //        Bitmap fullScreenBitmap = DisplayControl.FullScreen;

        //        fullScreenBitmap.Clear();
        //        while (true)
        //        {
        //            Debug.WriteLine("Getting Display Font SegoeUIRegular12");
        //            var displayFont = Resource.GetFont(Resource.FontResources.segoeuiregular12);
        //            Debug.WriteLine("Running WritePoint");
        //            WritePoint wrtPoint = new WritePoint();
        //            Thread.Sleep(delayBetween);

        //            Debug.WriteLine("Running ColorGradient");
        //            ColourGradient colourGradient = new ColourGradient(fullScreenBitmap);
        //            Thread.Sleep(delayBetween);

        //            Debug.WriteLine("Running BouncingBalls");
        //            BouncingBalls bb = new BouncingBalls(fullScreenBitmap);
        //            Thread.Sleep(delayBetween);
        //            //fullScreenBitmap.DrawText("This is text", displayFont, Color.NavajoWhite, 0, 1);

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine($"Exception: {e.Message}");
        //    }

        //    Thread.Sleep(Timeout.Infinite);
        //}

        // Devices found by watcher
        private static readonly Hashtable s_foundDevices = new();
        private static Ws2808 neo = new Ws2808(Gpio.IO38, 1);

        // Devices to collect from. Added when connected
        private static readonly Hashtable s_dataDevices = new();
        private static GpioController gpioController = new GpioController();


        public static void Main()
        {

            Configuration.SetPinFunction(Gpio.IO06, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO07, DeviceFunction.I2C1_CLOCK);

            const int busId = 1;

            I2cConnectionSettings i2cSettings = new(busId, Bmp280.SecondaryI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            

            // set higher sampling
            //i2CBmp280.TemperatureSampling = Sampling.LowPower;
            //i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

            //// Perform a synchronous measurement
            //var readResult = i2CBmp280.Read();

            //// Print out the measured data
            //Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius:0.#}\u00B0C");
            //Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals:0.##}hPa");

            //Console.WriteLine("Sample Client/Central 2 : Collect data from Environmental sensors");
            //Console.WriteLine("Searching for Environmental Sensors");

            // Create a watcher
            BluetoothLEAdvertisementWatcher watcher = new()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            watcher.Received += Watcher_Received;

            while (true)
            {
                Console.WriteLine("Starting BluetoothLEAdvertisementWatcher");
                watcher.Start();

                // Run until we have found some devices to connect to
                while (s_foundDevices.Count == 0)
                {
                    Debug.WriteLine("Scanning for devices");
                    Thread.Sleep(10000);
                }

                Console.WriteLine("Stopping BluetoothLEAdvertisementWatcher");

                // We can't connect if watch running so stop it.
                watcher.Stop();

                Console.WriteLine($"Devices found {s_foundDevices.Count}");
                Console.WriteLine("Connecting and Reading Sensors");

                foreach (DictionaryEntry entry in s_foundDevices)
                {
                    BluetoothLEDevice device = entry.Value as BluetoothLEDevice;

                    // Connect and register notify events
                    if (ConnectAndRegister(device))
                    {
                        if (s_dataDevices.Contains(device.BluetoothAddress))
                        {
                            s_dataDevices.Remove(device.BluetoothAddress);
                        }
                        s_dataDevices.Add(device.BluetoothAddress, device);
                    }
                }
                s_foundDevices.Clear();
            }
        }

        /// <summary>
        /// Check for device with correct Service UUID in advert and not already found
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool IsValidDevice(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (args.Advertisement.ServiceUuids.Length > 0 &&
                args.Advertisement.ServiceUuids[0].Equals(new Guid("A7EEDF2C-DA87-4CB5-A9C5-5151C78B0057")))
            {
                if (!s_foundDevices.Contains(args.BluetoothAddress))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            SetLedColor(Color.Blue);
            // Print information about received advertisement
            // You don't receive all information in 1 event and it can be split across 2 events
            // AdvertisementTypes 0 and 4
            Console.WriteLine($"Received advertisement address:{args.BluetoothAddress:X}/{args.BluetoothAddressType} Name:{args.Advertisement.LocalName}  Advert type:{args.AdvertisementType}  Services:{args.Advertisement.ServiceUuids.Length}");

            if (args.Advertisement.ServiceUuids.Length > 0)
            {
                
                Console.WriteLine($"Advert Service UUID {args.Advertisement.ServiceUuids[0]}");
            }

            // Look for advert with our primary service UUID from Bluetooth Sample 3
            if (IsValidDevice(args))
            {
                Console.WriteLine($"Found an Environmental test sensor :{args.BluetoothAddress:X}");

                // Add it to list as a BluetoothLEDevice
                s_foundDevices.Add(args.BluetoothAddress, BluetoothLEDevice.FromBluetoothAddress(args.BluetoothAddress, args.BluetoothAddressType));
            }

            SetLedColor(Color.Black);
        }

        /// <summary>
        /// Connect and set-up Temperature Characteristics for value 
        /// changed notifications.
        /// </summary>
        /// <param name="device">Bluetooth device</param>
        /// <returns>True if device connected</returns>
        private static bool ConnectAndRegister(BluetoothLEDevice device)
        {
            bool result = false;
            SetLedColor(Color.Blue);
            GattDeviceServicesResult sr = device.GetGattServicesForUuid(GattServiceUuids.EnvironmentalSensing);
            if (sr.Status == GattCommunicationStatus.Success)
            {
                // Connected and services read
                result = true;

                // Pick up all temperature characteristics
                foreach (GattDeviceService service in sr.Services)
                {
                    Console.WriteLine($"Service UUID {service.Uuid}");

                    GattCharacteristicsResult cr = service.GetCharacteristicsForUuid(GattCharacteristicUuids.Temperature);
                    GattCharacteristicsResult co2 = service.GetCharacteristicsForUuid(Utilities.CreateUuidFromShortCode((ushort) 11216));
                    
                    if (cr.Status == GattCommunicationStatus.Success)
                    {
                        //Temperature characteristics found now read value and 
                        //set up notify for value changed
                        foreach (GattCharacteristic gc in cr.Characteristics)
                        {
                            // Read current temperature
                            GattReadResult rr = gc.ReadValue();
                            if (rr.Status == GattCommunicationStatus.Success)
                            {
                                // Read current value and output
                                OutputTemp(gc, ReadTempValue(rr.Value));

                                // Set up a notify value changed event
                                gc.ValueChanged += TempValueChanged;
                                // and configure CCCD for Notify
                                gc.WriteClientCharacteristicConfigurationDescriptorWithResult(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            }
                        }
                    }
                    if (co2.Status == GattCommunicationStatus.Success)
                    {
                        //Temperature characteristics found now read value and 
                        //set up notify for value changed
                        foreach (GattCharacteristic gc in cr.Characteristics)
                        {
                            // Read current temperature
                            GattReadResult rr = gc.ReadValue();
                            if (rr.Status == GattCommunicationStatus.Success)
                            {
                                // Read current value and output
                                OutputCo2(gc, ReadTempValue(rr.Value));

                                // Set up a notify value changed event
                                gc.ValueChanged += TempValueChanged;
                                // and configure CCCD for Notify
                                gc.WriteClientCharacteristicConfigurationDescriptorWithResult(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            }
                        }
                    }
                }
            }
            
            SetLedColor(Color.Black);
            return result;
        }

        private static void Device_ConnectionStatusChanged(object sender, EventArgs e)
        {
            BluetoothLEDevice dev = (BluetoothLEDevice) sender;
            if (dev.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                Console.WriteLine($"Device {dev.BluetoothAddress:X} disconnected");
                Thread.Sleep(125);

                // Remove device. We get picked up again once advert seen.
                s_dataDevices.Remove(dev.BluetoothAddress);
                dev.Dispose();
            }
        }

        private static float ReadTempValue(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            return (float) rdr.ReadInt16() / 100;
        }

        private static void OutputTemp(GattCharacteristic gc, float value)
        {
            Console.WriteLine($"New value => Device:{gc.Service.Device.BluetoothAddress:X} Sensor:{gc.UserDescription,-20}  Current temp:{value}");
        }
        private static void OutputCo2(GattCharacteristic gc, float value)
        {
            Console.WriteLine($"New value => Device:{gc.Service.Device.BluetoothAddress:X} Sensor:{gc.UserDescription,-20}  Current value:{value}");
        }

        private static void TempValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedEventArgs)
        {
            OutputTemp(sender,
                ReadTempValue(valueChangedEventArgs.CharacteristicValue));
        }

        private static void SetLedColor(Color color)
        {
            var pixel = neo.Image;
            pixel.SetPixel(0, 0, color.G, color.R, color.B);
            neo.Update();
        }
    }
}
