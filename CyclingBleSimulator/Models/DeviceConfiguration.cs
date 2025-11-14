using System;

namespace CyclingBleSimulator.Models
{
    public class DeviceConfiguration
    {
        public string DeviceName { get; set; } = "Cycling Simulator";
        public bool EnableCSC { get; set; } = true;
        public bool EnablePower { get; set; } = true;
        public bool EnableHeartRate { get; set; } = true;
        public bool EnableFitnessMachine { get; set; } = true;

        // Auto-generation settings
        public bool AutoGenerate { get; set; } = true;
        public int UpdateInterval { get; set; } = 1000; // milliseconds

        // Manual control ranges
        public double MinSpeed { get; set; } = 0;
        public double MaxSpeed { get; set; } = 60; // km/h
        public int MinCadence { get; set; } = 0;
        public int MaxCadence { get; set; } = 120; // RPM
        public short MinPower { get; set; } = 0;
        public short MaxPower { get; set; } = 500; // Watts
        public byte MinHeartRate { get; set; } = 60;
        public byte MaxHeartRate { get; set; } = 200; // BPM
        public double MinIncline { get; set; } = -10;
        public double MaxIncline { get; set; } = 20; // Percent
    }
}
