using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CyclingBleSimulator.Models;
using CyclingBleSimulator.Services;

namespace CyclingBleSimulator.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly DeviceConfiguration _config;
        private readonly CyclingMetrics _metrics;
        private readonly DataGenerator _dataGenerator;
        private BleGattServer? _bleServer;
        private Timer? _updateTimer;

        private bool _isRunning;
        private string _statusText = "Stopped";
        private bool _autoGenerate = true;

        // Manual control values
        private double _manualSpeed = 25.0;
        private int _manualCadence = 80;
        private short _manualPower = 200;
        private byte _manualHeartRate = 140;
        private double _manualIncline = 0.0;
        private double _manualResistance = 5.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DeviceName
        {
            get => _config.DeviceName;
            set
            {
                _config.DeviceName = value;
                OnPropertyChanged();
            }
        }

        public bool EnableCSC
        {
            get => _config.EnableCSC;
            set
            {
                _config.EnableCSC = value;
                OnPropertyChanged();
            }
        }

        public bool EnablePower
        {
            get => _config.EnablePower;
            set
            {
                _config.EnablePower = value;
                OnPropertyChanged();
            }
        }

        public bool EnableHeartRate
        {
            get => _config.EnableHeartRate;
            set
            {
                _config.EnableHeartRate = value;
                OnPropertyChanged();
            }
        }

        public bool EnableFitnessMachine
        {
            get => _config.EnableFitnessMachine;
            set
            {
                _config.EnableFitnessMachine = value;
                OnPropertyChanged();
            }
        }

        public bool AutoGenerate
        {
            get => _autoGenerate;
            set
            {
                _autoGenerate = value;
                _config.AutoGenerate = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool CanStart => !_isRunning;
        public bool CanStop => _isRunning;

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        // Current metrics display
        public double CurrentSpeed => _metrics.Speed;
        public int CurrentCadence => _metrics.Cadence;
        public short CurrentPower => _metrics.Power;
        public byte CurrentHeartRate => _metrics.HeartRate;
        public uint CurrentDistance => _metrics.Distance;
        public ushort CurrentCalories => _metrics.Calories;

        // Manual controls
        public double ManualSpeed
        {
            get => _manualSpeed;
            set
            {
                _manualSpeed = value;
                OnPropertyChanged();
                if (!_autoGenerate)
                {
                    _metrics.Speed = value;
                }
            }
        }

        public int ManualCadence
        {
            get => _manualCadence;
            set
            {
                _manualCadence = value;
                OnPropertyChanged();
                if (!_autoGenerate)
                {
                    _metrics.Cadence = value;
                }
            }
        }

        public short ManualPower
        {
            get => _manualPower;
            set
            {
                _manualPower = value;
                OnPropertyChanged();
                if (!_autoGenerate)
                {
                    _metrics.Power = value;
                }
            }
        }

        public byte ManualHeartRate
        {
            get => _manualHeartRate;
            set
            {
                _manualHeartRate = value;
                OnPropertyChanged();
                if (!_autoGenerate)
                {
                    _metrics.HeartRate = value;
                }
            }
        }

        public double ManualIncline
        {
            get => _manualIncline;
            set
            {
                _manualIncline = value;
                OnPropertyChanged();
                if (!_autoGenerate)
                {
                    _metrics.Incline = value;
                }
            }
        }

        public double ManualResistance
        {
            get => _manualResistance;
            set
            {
                _manualResistance = value;
                OnPropertyChanged();
                if (!_autoGenerate)
                {
                    _metrics.Resistance = value;
                }
            }
        }

        public DeviceViewModel()
        {
            _config = new DeviceConfiguration
            {
                DeviceName = "Cycling Simulator",
                UpdateInterval = 1000
            };
            _metrics = new CyclingMetrics();
            _dataGenerator = new DataGenerator();
        }

        public async Task StartAsync()
        {
            try
            {
                StatusText = "Starting...";

                _bleServer = new BleGattServer(_config);
                _bleServer.StatusChanged += (s, msg) => UpdateStatus(msg);
                _bleServer.ErrorOccurred += (s, msg) => UpdateStatus($"Error: {msg}");

                var success = await _bleServer.StartAdvertisingAsync();
                if (success)
                {
                    IsRunning = true;
                    _updateTimer = new Timer(UpdateMetrics, null, 0, _config.UpdateInterval);
                    StatusText = $"Running - {_config.DeviceName}";
                }
                else
                {
                    StatusText = "Failed to start";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to start BLE advertising:\n{ex.Message}\n\nMake sure Bluetooth is enabled and you have the necessary permissions.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Stop()
        {
            _updateTimer?.Dispose();
            _updateTimer = null;

            _bleServer?.StopAdvertising();
            _bleServer?.Dispose();
            _bleServer = null;

            IsRunning = false;
            StatusText = "Stopped";
        }

        private void UpdateMetrics(object? state)
        {
            try
            {
                if (_autoGenerate)
                {
                    // Update base values from manual controls for the generator
                    _dataGenerator.SetBaseValues(
                        _manualSpeed,
                        _manualCadence,
                        _manualPower,
                        _manualHeartRate,
                        _manualIncline
                    );

                    _dataGenerator.GenerateRealisticData(_metrics, _config);
                }
                else
                {
                    // Use manual values directly
                    _metrics.Speed = _manualSpeed;
                    _metrics.Cadence = _manualCadence;
                    _metrics.Power = _manualPower;
                    _metrics.HeartRate = _manualHeartRate;
                    _metrics.Incline = _manualIncline;
                    _metrics.Resistance = _manualResistance;

                    // Still need to update cumulative metrics
                    UpdateCumulativeMetrics();
                }

                // Notify BLE clients
                _ = _bleServer?.UpdateMetricsAsync(_metrics);

                // Update UI
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(CurrentSpeed));
                    OnPropertyChanged(nameof(CurrentCadence));
                    OnPropertyChanged(nameof(CurrentPower));
                    OnPropertyChanged(nameof(CurrentHeartRate));
                    OnPropertyChanged(nameof(CurrentDistance));
                    OnPropertyChanged(nameof(CurrentCalories));
                });
            }
            catch (Exception ex)
            {
                UpdateStatus($"Update error: {ex.Message}");
            }
        }

        private void UpdateCumulativeMetrics()
        {
            var now = DateTime.Now;

            // Update wheel revolutions
            var wheelCircumference = 2.1;
            var timeSinceLastWheel = (now - _metrics.LastWheelTime).TotalSeconds;
            if (timeSinceLastWheel > 0)
            {
                var distanceMeters = (_metrics.Speed / 3.6) * timeSinceLastWheel;
                var revolutions = (uint)(distanceMeters / wheelCircumference);
                _metrics.WheelRevolutions += revolutions;
                _metrics.WheelEventTime = (ushort)((_metrics.WheelEventTime + (ushort)(timeSinceLastWheel * 1024)) % 65536);
                _metrics.LastWheelTime = now;
                _metrics.Distance += (uint)distanceMeters;
            }

            // Update crank revolutions
            var timeSinceLastCrank = (now - _metrics.LastCrankTime).TotalSeconds;
            if (timeSinceLastCrank > 0)
            {
                var crankRevs = (ushort)(_metrics.Cadence * timeSinceLastCrank / 60.0);
                _metrics.CrankRevolutions = (ushort)((_metrics.CrankRevolutions + crankRevs) % 65536);
                _metrics.CrankEventTime = (ushort)((_metrics.CrankEventTime + (ushort)(timeSinceLastCrank * 1024)) % 65536);
                _metrics.LastCrankTime = now;
            }
        }

        private void UpdateStatus(string message)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                StatusText = message;
            });
        }

        public void ResetMetrics()
        {
            _metrics.WheelRevolutions = 0;
            _metrics.CrankRevolutions = 0;
            _metrics.Distance = 0;
            _metrics.Calories = 0;
            _metrics.AccumulatedEnergy = 0;
            _dataGenerator.Reset();

            OnPropertyChanged(nameof(CurrentDistance));
            OnPropertyChanged(nameof(CurrentCalories));
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
