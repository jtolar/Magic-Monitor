﻿// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;

namespace MagicMonitor.Bluetooth.Sender.Services
{
    /// <summary>
    /// Device Information Service.
    /// Contains information about the device.
    /// </summary>
    public class DeviceInformationServiceService
    {
        private readonly GattLocalService _deviceInformationService;

        /// <summary>
        /// Create a new Device Information Service using supplied string.
        /// If a string is null the Characteristic will not be included in service.
        /// </summary>
        /// <param name="Manufacturer"></param>
        /// <param name="ModelNumber"></param>
        /// <param name="SerialNumber"></param>
        /// <param name="HardwareRevision"></param>
        /// <param name="FirmwareRevision"></param>
        /// <param name="SoftwareRevision"></param>
        public DeviceInformationServiceService(
            string Manufacturer,
            string ModelNumber = null,
            string SerialNumber = null,
            string HardwareRevision = null,
            string FirmwareRevision = null,
            string SoftwareRevision = null
            )
        {
            GattServiceProviderResult pr = GattServiceProvider.Create(GattServiceUuids.DeviceInformation);
            if (pr.Error != BluetoothError.Success)
            {
                throw new Exception("Unable to create service");
            }

            // Pick up service
            _deviceInformationService = pr.ServiceProvider.Service;

            CreateReadStaticCharacteristic(GattCharacteristicUuids.ManufacturerNameString, Manufacturer);
            CreateReadStaticCharacteristic(GattCharacteristicUuids.ModelNumberString, ModelNumber);
            CreateReadStaticCharacteristic(GattCharacteristicUuids.SerialNumberString, SerialNumber);
            CreateReadStaticCharacteristic(GattCharacteristicUuids.HardwareRevisionString, HardwareRevision);
            CreateReadStaticCharacteristic(GattCharacteristicUuids.FirmwareRevisionString, FirmwareRevision);
            CreateReadStaticCharacteristic(GattCharacteristicUuids.SoftwareRevisionString, SoftwareRevision);
        }

        /// <summary>
        /// Create static Characteristic if not null.
        /// </summary>
        /// <param name="Uuid">Characteristic UUID</param>
        /// <param name="data">string data or null</param>
        private void CreateReadStaticCharacteristic(Guid Uuid, String data)
        {
            if (data != null)
            {
                // Create data buffer
                DataWriter writer = new DataWriter();
                writer.WriteString(data);

                _deviceInformationService.CreateCharacteristic(Uuid, new GattLocalCharacteristicParameters()
                {
                    CharacteristicProperties = GattCharacteristicProperties.Read,
                    StaticValue = writer.DetachBuffer()
                });
            }
        }
    }
}


