using System;
using System.Device.Gpio;
using System.Runtime.CompilerServices;
using nanoFramework.Hardware.Esp32;

namespace MagicMonitor.Display.Ili9341
{
    public class SetupDisplay
    {
        private GpioController _gpioController = new GpioController();

        enum MyEnum
        {

        }

        public enum SpiBus
        {
            Spi1,
            Spi2,
            Spi3
        }

        private SetupDisplay(int miso, int mosi, int clock, int chipSelect, int dataCommand, int backlightPin,
            int reset, SpiBus spiBus)
        {
            MisoPin = miso;
            MosiPin = mosi;
            ClockPin = clock;
            ChipSelectPin = chipSelect;
            DataCommandPin = dataCommand;
            BacklightPin = backlightPin;
            ResetPin = reset;

            SetupSpiPins(spiBus);

        }

        private void SetupSpiPins(SpiBus bus)
        {
            switch (bus)
            {
                case SpiBus.Spi2:
                {
                    Configuration.SetPinFunction(MisoPin, DeviceFunction.SPI2_MISO);
                    Configuration.SetPinFunction(MosiPin, DeviceFunction.SPI2_MOSI);
                    Configuration.SetPinFunction(ClockPin, DeviceFunction.SPI2_CLOCK);
                    break;
                }
                default:
                case SpiBus.Spi1:
                {
                    Configuration.SetPinFunction(MisoPin, DeviceFunction.SPI1_MISO);
                    Configuration.SetPinFunction(MosiPin, DeviceFunction.SPI1_MOSI);
                    Configuration.SetPinFunction(ClockPin, DeviceFunction.SPI1_CLOCK);
                    break;
                }
            }
        }

        public int MisoPin { get; private set; }
        public int MosiPin { get; private set; }
        public int ClockPin { get; private set; }
        public int ChipSelectPin { get; private set; }
        public int ResetPin { get; private set; }
        public int DataCommandPin { get; private set; }
        public int BacklightPin { get; private set; }
    }
}