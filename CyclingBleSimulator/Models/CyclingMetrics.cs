using System;

namespace CyclingBleSimulator.Models
{
    public class CyclingMetrics
    {
        // Cycling Speed and Cadence
        public double Speed { get; set; } // km/h
        public int Cadence { get; set; } // RPM
        public uint WheelRevolutions { get; set; }
        public ushort WheelEventTime { get; set; } // 1/1024 seconds
        public ushort CrankRevolutions { get; set; }
        public ushort CrankEventTime { get; set; } // 1/1024 seconds

        // Cycling Power
        public short Power { get; set; } // Watts
        public byte PedalPowerBalance { get; set; }
        public ushort AccumulatedTorque { get; set; }
        public uint AccumulatedEnergy { get; set; }

        // Heart Rate
        public byte HeartRate { get; set; } // BPM
        public ushort EnergyExpended { get; set; }
        public List<ushort> RRIntervals { get; set; } = new List<ushort>();

        // Fitness Machine (Indoor Trainer)
        public double Incline { get; set; } // Percent grade
        public double Resistance { get; set; } // Resistance level
        public uint Distance { get; set; } // meters
        public ushort Calories { get; set; }

        // Timestamps for calculations
        public DateTime LastWheelTime { get; set; } = DateTime.Now;
        public DateTime LastCrankTime { get; set; } = DateTime.Now;
        public DateTime LastPowerTime { get; set; } = DateTime.Now;
    }
}
