using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using MagicMonitor.RestApi;
using nanoFramework.UI;
using nanoFramework.UI.GraphicDrivers;

namespace MagicMonitor
{
    public class Program
    {
        public static void Main()
        {
            const int backLightPin = 21;
            const int chipSelect = 15;
            const int dataCommand = 2;
            const int reset = -1;
            const int screenWidth = 320;
            const int screenHeight = 240;

            var displaySpiConfig = new SpiConfiguration(1, chipSelect, dataCommand, reset, backLightPin);

            var graphicDriver = Ili9341.GraphicDriverWithDefaultManufacturingSettings;

            var screenConfig = new ScreenConfiguration(26, 1, screenWidth, screenHeight, graphicDriver);

            var init = DisplayControl.Initialize(displaySpiConfig, screenConfig, 1024);
            Debug.WriteLine($"init screen initialized");

            ushort[] toSend = new ushort[100];
            var blue = Color.Blue.ToBgr565();
            var red = Color.Red.ToBgr565();
            var green = Color.Green.ToBgr565();
            var white = Color.White.ToBgr565();

            for (int i = 0; i < toSend.Length; i++)
            {
                toSend[i] = blue;
            }

            DisplayControl.Write(0, 0, 10, 10, toSend);

            for (int i = 0; i < toSend.Length; i++)
            {
                toSend[i] = red;
            }

            DisplayControl.Write(69, 0, 10, 10, toSend);

            for (int i = 0; i < toSend.Length; i++)
            {
                toSend[i] = green;
            }

            DisplayControl.Write(0, 149, 10, 10, toSend);

            for (int i = 0; i < toSend.Length; i++)
            {
                toSend[i] = white;
            }

            DisplayControl.Write(69, 149, 10, 10, toSend);

            RestApiServer.Start();

            Debug.WriteLine("Hello from nanoFramework!");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
