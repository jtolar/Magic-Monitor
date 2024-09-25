using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnitsNet;
using UnitsNet.Units;

namespace MagicMonitor.Sensors.Ens160
{
    public class Ens160 : IDisposable
    {
        private static readonly ushort ENS160_DEVICE_ID = 0x0160;

        private readonly I2cDevice device;

        public Temperature Temperature { get; internal set; }
        public RelativeHumidity RelativeHumidity { get; internal set; }

        public VolumeConcentration Co2Concentration { get; internal set; }

        public VolumeConcentration TvocConcentration { get; internal set; }

        public UBAAirQualityIndex AirQualityIndex { get; internal set; }

        public OperatingMode CurrentOperatingMode
        {
            get => (OperatingMode) device.Read8BitsFromRegister((byte) Registers
                .OPMODE);
            set => device.Write8BitsToRegister(Registers.OPMODE, (byte) value);
        }

        /// <summary>
        /// The default I2C address for the peripheral (0x52)
        /// </summary>
        public static byte DefaultI2cAddress => (byte) Addresses.Default;

        /// <summary>
        /// The secondary I2C address for the peripheral (0x53)
        /// ADDR is low
        /// </summary>
        public static byte SecondaryI2cAddress => (byte) Addresses.Address_0x53;

        public Ens160(I2cDevice i2cDevice, byte address = (byte) Addresses.Default)
        {
            device = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            InitializeSensor();

            CurrentOperatingMode = OperatingMode.Standard;
        }

        private void InitializeSensor()
        {
            device.Write8BitsToRegister(Registers.COMMAND, (byte) Commands.NOP, true);
            device.Write8BitsToRegister(Registers.COMMAND, (byte) Commands.CLRGPR, true);

            Thread.Sleep(10);
            Reset();
        }

        public void Reset()
        {
            device.Write8BitsToRegister(Registers.OPMODE, (byte) OperatingMode.Reset);
            Thread.Sleep(10);
        }

        public DeviceStatus GetDeviceStatus()
        {
            var statusByte = device.Read8BitsFromRegister((byte)Registers.DATA_STATUS);
            Debug.WriteLine("----------------");
            Debug.WriteLine($"DevStat: {(uint)statusByte} (0x{statusByte.ToString("X2")})");
            Debug.WriteLine("----------------");

            var retval = (statusByte & 0x0C) >> 2;
            return (DeviceStatus)retval;
        }

        public string GetDeviceStatusName(DeviceStatus devStatus)
        {
            switch (devStatus)
            {
                case DeviceStatus.Normal:
                    return "Normal";
                case DeviceStatus.WarmUp:
                    return "Warmup";
                case DeviceStatus.InitialStartup:
                    return "Initial Startup";
                case DeviceStatus.Invalid:
                    return "Invalid Status";
                default:
                    throw new ArgumentOutOfRangeException(nameof(devStatus));
            }
        }

        public string GetOperatingModeName(OperatingMode mode)
        {
            switch (mode)
            {
                case OperatingMode.Reset:
                    return "Reset";
                case OperatingMode.DeepSleep:
                    return "Deep Sleep";
                case OperatingMode.Idle:
                    return "Idle";
                case OperatingMode.Standard:
                    return "Standard";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
       
        public bool IsNewDataAvailable()
        {
            var value = device.Read8BitsFromRegister((byte) Registers.DATA_STATUS);

            return BitHelpers.GetBitValue(value, 0x02);
        }

        private bool IsNewGPRAvailable()
        {
            var value = device.Read8BitsFromRegister((byte) Registers.DATA_STATUS);

            return BitHelpers.GetBitValue(value, 0x03);
        }

        private void GetTotalVolatileOrganicCompounds()
        {
            var con = device.Read16BitsFromRegister((byte) Registers.DATA_TVOC);
            TvocConcentration = new VolumeConcentration(con, VolumeConcentrationUnit.PartPerBillion);
        }

        private void GetCO2Concentration()
        {
            var con = device.Read16BitsFromRegister((byte) Registers.DATA_ECO2);
            Co2Concentration = new VolumeConcentration(con, VolumeConcentrationUnit.PartPerMillion);
        }

        public void GetAirQualityIndex()
        {
            var value = device.Read8BitsFromRegister((byte) Registers.DATA_AQI);

            AirQualityIndex = (UBAAirQualityIndex) value;
        }

        public void SetCompensationValues(Temperature temperature, RelativeHumidity humidity)
        {
            //CurrentOperatingMode = OperatingMode.Reset;
            SetTemperature(temperature);
            SetHumidity(humidity);
            Thread.Sleep(100);
            //CurrentOperatingMode = OperatingMode.Standard;
            //Thread.Sleep(100);
        }

        /// <summary>
        /// Set ambient temperature
        /// </summary>
        /// <param name="ambientTemperature"></param>
        public void SetTemperature(Temperature ambientTemperature)
        {
            var newTemp =
                Temperature.FromDegreesCelsius(Convert.ToDouble(ambientTemperature.DegreesCelsius.ToString("N1")));
            ushort temp = (ushort) (newTemp.Kelvins * 64);
            Debug.WriteLine($"DegreesCelsius: {newTemp.DegreesCelsius}, tempToWrite: {temp}");
            device.Write16BitsToRegister(Registers.TEMP_IN, temp);
        }

        /// <summary>
        /// Set relative humidity
        /// </summary>
        /// <param name="humidity"></param>
        public void SetHumidity(RelativeHumidity humidity)
        {
            ushort hum = (ushort) (Math.Round(humidity.Percent) * 512);
            Debug.WriteLine($"DegreesCelsius: {humidity.Percent}%, humToWrite: {hum}");
            device.Write16BitsToRegister(Registers.RH_IN, hum);
        }

        /// <summary>
        /// Get the temperature used for calculations - taken from TEMP_IN if supplied
        /// </summary>
        /// <returns>Temperature</returns>
        public Temperature GetTemperature()
        {
            var temp = device.Read16BitsFromRegister((byte) Registers.DATA_T);
            Temperature = Temperature.FromKelvins(temp / 64.0);
            return Temperature;
        }

        /// <summary>
        /// Get the relative humidity used in its calculations -b taken from RH_IN if supplied
        /// </summary>
        /// <returns></returns>
        public RelativeHumidity GetHumidity()
        {
            var hum = device.Read16BitsFromRegister((byte) Registers.DATA_T);
            RelativeHumidity = RelativeHumidity.FromPercent(hum / 512);
            return RelativeHumidity;
        }

        public bool IsConnected
        {
            get
            {
                var devId = GetDeviceID();
                return devId == ENS160_DEVICE_ID;
            }
        }

        /// <summary>
        /// Get the sensor app / firmware version
        /// </summary>
        /// <returns>The major, minor, release values as a tuple of bytes</returns>
        public Version GetFirmwareVersion()
        {
            device.Write8BitsToRegister(Registers.COMMAND, (byte) Commands.GET_APPVER);

            var bytes = new byte[3];
            var major = device.Read8BitsFromRegister((byte) Registers.GPR_READ_4);
            var minor = device.Read8BitsFromRegister((byte) Registers.GPR_READ_5);
            var build = device.Read8BitsFromRegister((byte) Registers.GPR_READ_6);

            return new Version(major, minor, build, 0);
        }

        public void Read()
        {
            GetCO2Concentration();
            GetAirQualityIndex();
            GetTotalVolatileOrganicCompounds();
            GetHumidity();
            GetTemperature();
        }

        /// <summary>
        /// Get the sensor ID from PART_ID register
        /// Default value is 0x0160 (352)
        /// </summary>
        /// <returns>ID as a ushort (2 bytes)</returns>
        
        public ushort GetDeviceID()
        {
            int retVal;
            byte[] tempVal = new byte[2];
            ushort id;

            retVal = device.Read16BitsFromRegister((byte) Registers.PART_ID);

            id = tempVal[0];
            id |= (ushort) (tempVal[1] << 8);

            if (retVal != 0)
                return 0;

            return id;

            //return device.Read16BitsFromRegister((byte) Registers.PART_ID);
        }

        ///// <summary>
        ///// Get the sensor app / firmware version
        ///// </summary>
        ///// <returns>The major, minor, release values as a tuple of bytes</returns>
        //public string GetFirmwareVersion2()
        //{
        //    byte[] span = new[] { (byte)Registers.GPR_READ_4, (byte)Registers.GPR_READ_5,
        //        (byte)Registers.GPR_READ_6 };

        //    device.Write(new SpanByte(new[] { (byte) Registers.COMMAND, (byte) Commands.GET_APPVER }));
        //    device.Read(span);
        //    return $"{(uint) span[0]}.{(uint) span[1]}.{span[2]}";
        //    //return "";
        //}

        /// <summary>
        /// Clears the 10 GPR registers
        /// </summary>
        private void ClearGPRRegisters()
        {
            device.Write8BitsToRegister(Registers.COMMAND, (byte) Commands.CLRGPR);
            Thread.Sleep(10);
        }

        public void Dispose()
        {
            device?.Dispose();
        }
    }
}