using System;
using MagicMonitor.Bluetooth.CollectorService;
using MagicMonitor.Common.SoftWAP;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using MagicMonitor.Bluetooth.Sender;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

//using nanoFramework.UI.GraphicDrivers;
//using MagicMonitor.Common.SoftWAP;

namespace MagicMonitor
{
    public class Program
    {
        public static void Main()
        {
            //const int backLightPin = Gpio.IO21;
            //const int chipSelect = Gpio.IO15;
            //const int dataCommand = Gpio.IO02;
            //const int reset = -1;
            //const int screenWidth = 320;
            //const int screenHeight = 240;
            //const int delayBetween = 3000;

            //var gpioController = new GpioController();

            //Debug.WriteLine("Setting up Display SPI Bus 2");

            //try
            //{
                //Configuration.SetPinFunction(Gpio.IO12, DeviceFunction.SPI2_MISO);
                //Configuration.SetPinFunction(Gpio.IO13, DeviceFunction.SPI2_MOSI);
                //Configuration.SetPinFunction(Gpio.IO14, DeviceFunction.SPI2_CLOCK);

                //var displaySpiConfig = new SpiConfiguration(2, chipSelect, dataCommand, reset, backLightPin);

                //var graphicDriver = Ili9341.GraphicDriver;

                //var screenConfig = new ScreenConfiguration(1, 1, screenWidth, screenHeight, graphicDriver);

                //var bufferSize = (uint) (DisplayControl.ScreenWidth * DisplayControl.ScreenHeight * 3 / 4);

                //DisplayControl.Initialize(displaySpiConfig, screenConfig, bufferSize);

                ////gpioController.OpenPin(backLightPin, PinMode.Output);
                ////gpioController.Write(backLightPin, PinValue.High);

                //Debug.WriteLine($"init screen initialized");

                //Debug.WriteLine($"Buffer Size is {bufferSize}, IsFullScreenBufferAvailable: {DisplayControl.IsFullScreenBufferAvailable}");
                //Debug.WriteLine("Fullscreen Bitmap");
                //Bitmap fullScreenBitmap = DisplayControl.FullScreen;

                //fullScreenBitmap.Clear();
                //while (true)
                //{
                //    //Debug.WriteLine("Getting Display Font SegoeUIRegular12");
                //    //var displayFont = Resource.GetFont(Resource.FontResources.segoeuiregular12);
                //    Debug.WriteLine("Running WritePoint");
                //    WritePoint wrtPoint = new WritePoint();
                //    Thread.Sleep(delayBetween);

                //    Debug.WriteLine("Running ColorGradient");
                //    ColourGradient colourGradient = new ColourGradient(fullScreenBitmap);
                //    Thread.Sleep(delayBetween);

                //    Debug.WriteLine("Running BouncingBalls");
                //    BouncingBalls bb = new BouncingBalls(fullScreenBitmap);
                //    Thread.Sleep(delayBetween);
                //    //fullScreenBitmap.DrawText("This is text", displayFont, Color.NavajoWhite, 0, 1);

                //}
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine($"Exception: {e.Message}");
            //}

            //var softWapConfiguration = new SoftApConfiguration();
            //SoftWAPService.StartSoftWap(softWapConfiguration, 5);
            //RestApiServer.Start();
            var deviceMac =  BitConverter.ToInt32(NetworkInterface.GetAllNetworkInterfaces()[0].PhysicalAddress, 0);
            Debug.WriteLine($"Device ID: {deviceMac}");
            Debug.WriteLine($"Platform {SystemInfo.Platform} - Target {SystemInfo.TargetName} - OEM {SystemInfo.OEMString}");

            Thread.Sleep(3000);
            CollectorService.StartCollectorService();
            //BluetoothDataSender.Start(deviceMac);
            Debug.WriteLine("Hello from nanoFramework!");

            Thread.Sleep(Timeout.Infinite);
        }


    }
}
