using System;
using System.Net.NetworkInformation;

namespace MagicMonitor.Common
{
    public static class DeviceUtility
    {
        private static byte[] deviceMAC = NetworkInterface.GetAllNetworkInterfaces()[0].PhysicalAddress;

        public static string DeviceName => BitConverter.ToString(deviceMAC);

        public static int DeviceId = BitConverter.ToInt32(deviceMAC, 0);


    }
}