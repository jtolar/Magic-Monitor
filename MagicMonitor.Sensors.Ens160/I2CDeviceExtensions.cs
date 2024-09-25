using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Device.I2c;
using System.Threading;

namespace MagicMonitor.Sensors.Ens160
{
    public static class I2CDeviceExtensions
    {

        internal static void Write8BitsToRegister(this I2cDevice device, byte register, byte value, bool ignoreSleep = false, 
            Endianness endianness = Endianness.LittleEndian)
        {
            _ = device.Write(new SpanByte(new[] { (byte) register, value }));
            if (!ignoreSleep)
                Thread.Sleep(10);
        }

        internal static void Write8BitsToRegister(this I2cDevice device, Registers register, byte value, bool ignoreSleep = false, 
            Endianness endianness = Endianness.LittleEndian)
        {
            Write8BitsToRegister(device, (byte) register, value, ignoreSleep);
        }

        /// <summary>
        /// Write data to a register in the peripheral.
        /// </summary>
        /// <param name="address">Address of the register to write to.</param>
        /// <param name="writeBuffer">A buffer of byte values to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        internal static void Write16BitsToRegister(this I2cDevice device, Registers register, ushort writeBuffer, 
            Endianness endianness = Endianness.LittleEndian)
        {
            Write16BitsToRegister(device, (byte) register, writeBuffer, endianness);
        }

        internal static void Write16BitsToRegister(this I2cDevice device, byte register, ushort writeBuffer,
            Endianness endianness = Endianness.LittleEndian)
        {
            SpanByte outBytes = new byte[3];
            outBytes[0] = register;

            switch (endianness)
            {
                case Endianness.LittleEndian:
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(outBytes.Slice(1), writeBuffer);
                    break;
                }
                case Endianness.BigEndian:
                    BinaryPrimitives.WriteUInt16BigEndian(outBytes.Slice(1), writeBuffer);
                    break;
            }

            device.Write(outBytes);
        }

        /// <summary>
        /// Reads an 8 bit value from a register.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <returns>Value from register.</returns>
        internal static byte Read8BitsFromRegister(this I2cDevice device, byte register)
        {
            device.WriteByte(register);
            byte value = device.ReadByte();
            
            return value;
        }

        /// <summary>
        /// Reads a 16 bit value over I2C.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <param name="endianness">Interpretation of the bytes (big or little endian).</param>
        /// <returns>Value from register.</returns>
        internal static ushort Read16BitsFromRegister(this I2cDevice device, byte register, Endianness endianness = Endianness.LittleEndian)
        {
            SpanByte bytes = new byte[2];

            device.WriteByte(register);
            device.Read(bytes);

            switch (endianness)
            {
                case Endianness.LittleEndian:
                    return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
                case Endianness.BigEndian:
                    return BinaryPrimitives.ReadUInt16BigEndian(bytes);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Reads a 24 bit value over I2C.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <param name="endianness">Interpretation of the bytes (big or little endian).</param>
        /// <returns>Value from register.</returns>
        internal static uint Read24BitsFromRegister(this I2cDevice device, byte register, Endianness endianness = Endianness.LittleEndian)
        {
            SpanByte bytes = new byte[4];

            device.WriteByte(register);
            device.Read(bytes.Slice(1));

            switch (endianness)
            {
                case Endianness.LittleEndian:
                    return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
                case Endianness.BigEndian:
                    return BinaryPrimitives.ReadUInt32BigEndian(bytes);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Specifies the Endianness of a device.
        /// </summary>
        protected internal enum Endianness
        {
            /// <summary>
            /// Indicates little endian.
            /// </summary>
            LittleEndian,

            /// <summary>
            /// Indicates big endian.
            /// </summary>
            BigEndian
        }
    }
}