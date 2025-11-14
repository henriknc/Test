using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CyclingBleSimulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private DeviceViewModel? _selectedDevice;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        public DeviceViewModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            // Add first device by default
            AddDevice();
        }

        public void AddDevice()
        {
            var device = new DeviceViewModel
            {
                DeviceName = $"Cycling Simulator {Devices.Count + 1}"
            };
            Devices.Add(device);

            if (SelectedDevice == null)
            {
                SelectedDevice = device;
            }
        }

        public void RemoveDevice(DeviceViewModel device)
        {
            if (device.IsRunning)
            {
                device.Stop();
            }

            Devices.Remove(device);
            device.Dispose();

            if (SelectedDevice == device)
            {
                SelectedDevice = Devices.Count > 0 ? Devices[0] : null;
            }
        }

        public async Task StartSelectedDeviceAsync()
        {
            if (SelectedDevice != null && !SelectedDevice.IsRunning)
            {
                await SelectedDevice.StartAsync();
            }
        }

        public void StopSelectedDevice()
        {
            if (SelectedDevice != null && SelectedDevice.IsRunning)
            {
                SelectedDevice.Stop();
            }
        }

        public void StartAllDevices()
        {
            foreach (var device in Devices)
            {
                if (!device.IsRunning)
                {
                    _ = device.StartAsync();
                }
            }
        }

        public void StopAllDevices()
        {
            foreach (var device in Devices)
            {
                if (device.IsRunning)
                {
                    device.Stop();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            foreach (var device in Devices)
            {
                device.Dispose();
            }
            Devices.Clear();
        }
    }
}
