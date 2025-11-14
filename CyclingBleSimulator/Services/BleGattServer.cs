using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using CyclingBleSimulator.Models;

namespace CyclingBleSimulator.Services
{
    public class BleGattServer : IDisposable
    {
        private GattServiceProvider? _cscServiceProvider;
        private GattServiceProvider? _powerServiceProvider;
        private GattServiceProvider? _heartRateServiceProvider;
        private GattServiceProvider? _fitnessMachineServiceProvider;

        private GattLocalCharacteristic? _cscMeasurementCharacteristic;
        private GattLocalCharacteristic? _powerMeasurementCharacteristic;
        private GattLocalCharacteristic? _heartRateMeasurementCharacteristic;
        private GattLocalCharacteristic? _indoorBikeDataCharacteristic;

        private readonly DeviceConfiguration _config;
        private bool _isAdvertising = false;

        // Standard BLE UUIDs for cycling services
        private static readonly Guid CscServiceUuid = Guid.Parse("00001816-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CscMeasurementUuid = Guid.Parse("00002a5b-0000-1000-8000-00805f9b34fb");

        private static readonly Guid PowerServiceUuid = Guid.Parse("00001818-0000-1000-8000-00805f9b34fb");
        private static readonly Guid PowerMeasurementUuid = Guid.Parse("00002a63-0000-1000-8000-00805f9b34fb");

        private static readonly Guid HeartRateServiceUuid = Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb");
        private static readonly Guid HeartRateMeasurementUuid = Guid.Parse("00002a37-0000-1000-8000-00805f9b34fb");

        private static readonly Guid FitnessMachineServiceUuid = Guid.Parse("00001826-0000-1000-8000-00805f9b34fb");
        private static readonly Guid IndoorBikeDataUuid = Guid.Parse("00002ad2-0000-1000-8000-00805f9b34fb");

        public event EventHandler<string>? StatusChanged;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsAdvertising => _isAdvertising;

        public BleGattServer(DeviceConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> StartAdvertisingAsync()
        {
            try
            {
                StatusChanged?.Invoke(this, "Initializing BLE services...");

                // Initialize services based on configuration
                if (_config.EnableCSC)
                {
                    await InitializeCscServiceAsync();
                }

                if (_config.EnablePower)
                {
                    await InitializePowerServiceAsync();
                }

                if (_config.EnableHeartRate)
                {
                    await InitializeHeartRateServiceAsync();
                }

                if (_config.EnableFitnessMachine)
                {
                    await InitializeFitnessMachineServiceAsync();
                }

                _isAdvertising = true;
                StatusChanged?.Invoke(this, $"Advertising as '{_config.DeviceName}'");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to start advertising: {ex.Message}");
                return false;
            }
        }

        public void StopAdvertising()
        {
            try
            {
                _cscServiceProvider?.StopAdvertising();
                _powerServiceProvider?.StopAdvertising();
                _heartRateServiceProvider?.StopAdvertising();
                _fitnessMachineServiceProvider?.StopAdvertising();

                _isAdvertising = false;
                StatusChanged?.Invoke(this, "Stopped advertising");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error stopping advertising: {ex.Message}");
            }
        }

        private async Task InitializeCscServiceAsync()
        {
            var result = await GattServiceProvider.CreateAsync(CscServiceUuid);
            if (result.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create CSC service: {result.Error}");
            }

            _cscServiceProvider = result.ServiceProvider;

            // Create CSC Measurement characteristic
            var parameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Notify,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            var charResult = await _cscServiceProvider.Service.CreateCharacteristicAsync(
                CscMeasurementUuid, parameters);

            if (charResult.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create CSC characteristic: {charResult.Error}");
            }

            _cscMeasurementCharacteristic = charResult.Characteristic;

            // Start advertising
            var advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

            _cscServiceProvider.StartAdvertising(advParameters);
            StatusChanged?.Invoke(this, "CSC Service initialized");
        }

        private async Task InitializePowerServiceAsync()
        {
            var result = await GattServiceProvider.CreateAsync(PowerServiceUuid);
            if (result.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create Power service: {result.Error}");
            }

            _powerServiceProvider = result.ServiceProvider;

            var parameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Notify,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            var charResult = await _powerServiceProvider.Service.CreateCharacteristicAsync(
                PowerMeasurementUuid, parameters);

            if (charResult.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create Power characteristic: {charResult.Error}");
            }

            _powerMeasurementCharacteristic = charResult.Characteristic;

            var advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

            _powerServiceProvider.StartAdvertising(advParameters);
            StatusChanged?.Invoke(this, "Power Service initialized");
        }

        private async Task InitializeHeartRateServiceAsync()
        {
            var result = await GattServiceProvider.CreateAsync(HeartRateServiceUuid);
            if (result.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create Heart Rate service: {result.Error}");
            }

            _heartRateServiceProvider = result.ServiceProvider;

            var parameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Notify,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            var charResult = await _heartRateServiceProvider.Service.CreateCharacteristicAsync(
                HeartRateMeasurementUuid, parameters);

            if (charResult.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create Heart Rate characteristic: {charResult.Error}");
            }

            _heartRateMeasurementCharacteristic = charResult.Characteristic;

            var advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

            _heartRateServiceProvider.StartAdvertising(advParameters);
            StatusChanged?.Invoke(this, "Heart Rate Service initialized");
        }

        private async Task InitializeFitnessMachineServiceAsync()
        {
            var result = await GattServiceProvider.CreateAsync(FitnessMachineServiceUuid);
            if (result.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create Fitness Machine service: {result.Error}");
            }

            _fitnessMachineServiceProvider = result.ServiceProvider;

            var parameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Notify,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            var charResult = await _fitnessMachineServiceProvider.Service.CreateCharacteristicAsync(
                IndoorBikeDataUuid, parameters);

            if (charResult.Error != BluetoothError.Success)
            {
                throw new Exception($"Failed to create Indoor Bike Data characteristic: {charResult.Error}");
            }

            _indoorBikeDataCharacteristic = charResult.Characteristic;

            var advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

            _fitnessMachineServiceProvider.StartAdvertising(advParameters);
            StatusChanged?.Invoke(this, "Fitness Machine Service initialized");
        }

        public async Task UpdateMetricsAsync(CyclingMetrics metrics)
        {
            try
            {
                if (_config.EnableCSC && _cscMeasurementCharacteristic != null)
                {
                    await NotifyCscMeasurementAsync(metrics);
                }

                if (_config.EnablePower && _powerMeasurementCharacteristic != null)
                {
                    await NotifyPowerMeasurementAsync(metrics);
                }

                if (_config.EnableHeartRate && _heartRateMeasurementCharacteristic != null)
                {
                    await NotifyHeartRateMeasurementAsync(metrics);
                }

                if (_config.EnableFitnessMachine && _indoorBikeDataCharacteristic != null)
                {
                    await NotifyIndoorBikeDataAsync(metrics);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error updating metrics: {ex.Message}");
            }
        }

        private async Task NotifyCscMeasurementAsync(CyclingMetrics metrics)
        {
            var writer = new DataWriter();

            // Flags: bit 0 = Wheel Revolution Data Present, bit 1 = Crank Revolution Data Present
            byte flags = 0x03; // Both present
            writer.WriteByte(flags);

            // Cumulative Wheel Revolutions (UINT32)
            writer.WriteUInt32(metrics.WheelRevolutions);

            // Last Wheel Event Time (UINT16) - 1/1024 second resolution
            writer.WriteUInt16(metrics.WheelEventTime);

            // Cumulative Crank Revolutions (UINT16)
            writer.WriteUInt16(metrics.CrankRevolutions);

            // Last Crank Event Time (UINT16) - 1/1024 second resolution
            writer.WriteUInt16(metrics.CrankEventTime);

            await _cscMeasurementCharacteristic!.NotifyValueAsync(writer.DetachBuffer());
        }

        private async Task NotifyPowerMeasurementAsync(CyclingMetrics metrics)
        {
            var writer = new DataWriter();

            // Flags: bit 0-1 = Pedal Power Balance Present (01), others = 0
            ushort flags = 0x0001;
            writer.WriteUInt16(flags);

            // Instantaneous Power (INT16) - Watts
            writer.WriteInt16(metrics.Power);

            // Pedal Power Balance (UINT8) - percentage
            writer.WriteByte(metrics.PedalPowerBalance);

            await _powerMeasurementCharacteristic!.NotifyValueAsync(writer.DetachBuffer());
        }

        private async Task NotifyHeartRateMeasurementAsync(CyclingMetrics metrics)
        {
            var writer = new DataWriter();

            // Flags: bit 0 = HR format (0 = UINT8), bit 4 = RR-Interval present
            byte flags = 0x10;
            writer.WriteByte(flags);

            // Heart Rate Value (UINT8)
            writer.WriteByte(metrics.HeartRate);

            // RR-Intervals (UINT16) - 1/1024 second resolution
            foreach (var rrInterval in metrics.RRIntervals.Take(3)) // Max 3 intervals
            {
                writer.WriteUInt16(rrInterval);
            }

            await _heartRateMeasurementCharacteristic!.NotifyValueAsync(writer.DetachBuffer());
        }

        private async Task NotifyIndoorBikeDataAsync(CyclingMetrics metrics)
        {
            var writer = new DataWriter();

            // Flags: More Data = 0, Average Speed present, Instantaneous Cadence present,
            // Instantaneous Power present, Total Distance present, Resistance Level present
            ushort flags = 0x00C4; // bits 2, 6, 7
            writer.WriteUInt16(flags);

            // Instantaneous Speed (UINT16) - 0.01 km/h resolution
            writer.WriteUInt16((ushort)(metrics.Speed * 100));

            // Instantaneous Cadence (UINT16) - 0.5 rpm resolution
            writer.WriteUInt16((ushort)(metrics.Cadence * 2));

            // Instantaneous Power (INT16) - Watts
            writer.WriteInt16(metrics.Power);

            // Total Distance (UINT24) - meters
            writer.WriteByte((byte)(metrics.Distance & 0xFF));
            writer.WriteByte((byte)((metrics.Distance >> 8) & 0xFF));
            writer.WriteByte((byte)((metrics.Distance >> 16) & 0xFF));

            // Resistance Level (INT16) - unitless
            writer.WriteInt16((short)(metrics.Resistance * 10));

            await _indoorBikeDataCharacteristic!.NotifyValueAsync(writer.DetachBuffer());
        }

        public void Dispose()
        {
            StopAdvertising();

            _cscServiceProvider?.Dispose();
            _powerServiceProvider?.Dispose();
            _heartRateServiceProvider?.Dispose();
            _fitnessMachineServiceProvider?.Dispose();
        }
    }
}
