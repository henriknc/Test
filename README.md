# Cycling BLE Simulator

A comprehensive Bluetooth Low Energy (BLE) cycling device simulator for Windows, built with C# and WPF. This application allows you to simulate multiple cycling devices simultaneously for testing BLE cycling apps and devices.

## Features

### Supported BLE Services

- **Cycling Speed and Cadence (CSC)** - GATT Service UUID: 0x1816
  - Wheel revolution data
  - Crank revolution data
  - Real-time speed and cadence measurements

- **Cycling Power (CP)** - GATT Service UUID: 0x1818
  - Instantaneous power (Watts)
  - Pedal power balance
  - Accumulated torque

- **Heart Rate Monitor (HRM)** - GATT Service UUID: 0x180D
  - Heart rate in BPM
  - RR-interval data (Heart Rate Variability)
  - Energy expenditure

- **Fitness Machine Service (FTM)** - GATT Service UUID: 0x1826
  - Indoor bike data
  - Distance and calories
  - Resistance and incline control

### Key Capabilities

1. **Multiple Device Simulation** - Run multiple virtual cycling devices simultaneously
2. **Realistic Data Generation** - Auto-generates cycling data with natural variations:
   - Smooth speed and cadence changes
   - Power correlated with speed/cadence
   - Heart rate that responds to intensity changes
   - Natural HRV (Heart Rate Variability)

3. **Manual Control Mode** - Full manual control over all metrics:
   - Speed (0-60 km/h)
   - Cadence (0-120 RPM)
   - Power (0-500 Watts)
   - Heart Rate (60-200 BPM)
   - Incline (-10% to +20%)
   - Resistance (0-20 levels)

4. **Hybrid Mode** - Auto-generate mode uses manual values as baseline targets with realistic variations

## Requirements

- Windows 10/11 (build 10.0.22621 or later)
- .NET 8.0 Runtime
- Bluetooth 4.0+ adapter
- Administrator privileges may be required for BLE advertising

## Building the Project

### Using Visual Studio
1. Open `CyclingBleSimulator.sln` in Visual Studio 2022
2. Restore NuGet packages
3. Build the solution (F6)
4. Run the application (F5)

### Using .NET CLI
```bash
dotnet restore
dotnet build
dotnet run --project CyclingBleSimulator/CyclingBleSimulator.csproj
```

## Usage

### Starting a Simulation

1. **Configure the Device**
   - Set a unique device name
   - Enable/disable specific BLE services
   - Choose data mode (auto-generate or manual)

2. **Adjust Values**
   - In auto-generate mode: Set baseline values that the simulator will vary around
   - In manual mode: Set exact values to broadcast

3. **Start Advertising**
   - Click "Start" to begin BLE advertising
   - The device will appear in BLE scanning apps with the configured name

4. **Connect and Test**
   - Use your cycling app (Zwift, TrainerRoad, etc.) to connect
   - Monitor the current metrics in the UI

### Multiple Devices

- Click "Add Device" to create additional virtual devices
- Each device can have different configurations and names
- Use "Start All" / "Stop All" for batch operations
- Switch between devices using the dropdown

### Resetting Metrics

Click "Reset Metrics" to clear cumulative values:
- Distance
- Calories
- Accumulated energy
- Revolution counters

## Project Structure

```
CyclingBleSimulator/
├── Models/
│   ├── CyclingMetrics.cs          # Data model for all cycling metrics
│   └── DeviceConfiguration.cs      # Device settings and configuration
├── Services/
│   ├── BleGattServer.cs           # BLE GATT server implementation
│   └── DataGenerator.cs           # Realistic cycling data generation
├── ViewModels/
│   ├── DeviceViewModel.cs         # Single device view model
│   └── MainViewModel.cs           # Multi-device management
├── App.xaml                       # Application resources
├── MainWindow.xaml                # Main UI layout
└── MainWindow.xaml.cs             # UI code-behind
```

## Technical Details

### BLE Implementation

The simulator uses Windows Runtime APIs (`Windows.Devices.Bluetooth`) to create GATT services and advertise as a BLE peripheral. Each service follows the official Bluetooth SIG specifications:

- **CSC Measurement Characteristic (0x2A5B)**: Contains wheel and crank revolution data with 1/1024 second timestamps
- **Power Measurement Characteristic (0x2A63)**: Instantaneous power with flags for additional data fields
- **Heart Rate Measurement (0x2A37)**: Heart rate value with RR-intervals for HRV
- **Indoor Bike Data (0x2AD2)**: Comprehensive fitness machine data

### Data Generation Algorithm

The realistic data generator uses:
- Sinusoidal intensity cycles (30-second periods)
- Smooth exponential moving average for transitions
- Random micro-variations for natural fluctuation
- Correlated metrics (power increases with speed/cadence)
- Lagging heart rate response to simulate cardiovascular delay

### Cumulative Metrics

Wheel and crank revolutions are calculated based on:
- Wheel: 700c wheel circumference (2.1m)
- Speed-to-distance conversion
- Cadence-to-crank revolution conversion
- Proper timestamp wrapping at 65536 (UINT16 limit)

## Troubleshooting

### BLE Advertising Fails

- Ensure Bluetooth is enabled on your PC
- Check that no other application is using BLE advertising
- Run as Administrator if permission errors occur
- Verify your Bluetooth adapter supports BLE peripheral mode (not all adapters do)

### No Devices Found

- Make sure the simulator shows "Running" status
- Check that enabled services match what your app is scanning for
- Try restarting Bluetooth on your device
- Some apps require specific service combinations

### Connection Drops

- Reduce the number of simultaneous devices
- Check for Bluetooth interference
- Ensure stable USB connection for Bluetooth dongles
- Verify power management isn't disabling Bluetooth

## Known Limitations

- BLE peripheral mode requires specific Bluetooth hardware support
- Maximum number of simultaneous devices depends on BLE hardware
- Some Bluetooth adapters may not support GATT server role
- Requires Windows 10 build 10.0.22621 or later for full BLE API support

## Future Enhancements

Potential features for future versions:
- Save/load workout profiles
- Graph visualization of metrics over time
- ANT+ protocol support
- Custom service/characteristic creator
- Remote control via web interface
- Workout automation and scripting

## License

This project is provided as-is for testing and development purposes.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Acknowledgments

Built using:
- .NET 8.0 and WPF
- Windows Runtime Bluetooth APIs
- Bluetooth SIG GATT specifications
