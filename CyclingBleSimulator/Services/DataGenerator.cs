using System;
using CyclingBleSimulator.Models;

namespace CyclingBleSimulator.Services
{
    public class DataGenerator
    {
        private readonly Random _random = new Random();
        private double _baseSpeed = 25.0; // km/h
        private int _baseCadence = 80; // RPM
        private short _basePower = 200; // Watts
        private byte _baseHeartRate = 140; // BPM
        private double _baseIncline = 0.0;

        private double _speedVariation = 0;
        private double _cadenceVariation = 0;
        private double _powerVariation = 0;
        private double _heartRateVariation = 0;

        private DateTime _sessionStart = DateTime.Now;

        public void GenerateRealisticData(CyclingMetrics metrics, DeviceConfiguration config)
        {
            var elapsed = (DateTime.Now - _sessionStart).TotalSeconds;

            // Simulate varying intensity over time with sine waves
            var intensityCycle = Math.Sin(elapsed / 30.0) * 0.3; // 30-second cycles
            var microVariation = ((_random.NextDouble() - 0.5) * 0.1); // Small random variations

            // Generate Speed with smooth variations
            _speedVariation = _speedVariation * 0.9 + microVariation * 0.1;
            var targetSpeed = _baseSpeed * (1.0 + intensityCycle + _speedVariation);
            metrics.Speed = Math.Clamp(targetSpeed, config.MinSpeed, config.MaxSpeed);

            // Generate Cadence (correlated with speed)
            _cadenceVariation = _cadenceVariation * 0.9 + microVariation * 0.1;
            var targetCadence = _baseCadence * (1.0 + intensityCycle + _cadenceVariation);
            metrics.Cadence = (int)Math.Clamp(targetCadence, config.MinCadence, config.MaxCadence);

            // Generate Power (strongly correlated with speed and cadence)
            _powerVariation = _powerVariation * 0.9 + microVariation * 0.1;
            var targetPower = _basePower * (1.0 + intensityCycle * 1.2 + _powerVariation);
            metrics.Power = (short)Math.Clamp(targetPower, config.MinPower, config.MaxPower);

            // Generate Heart Rate (lags behind power changes)
            _heartRateVariation = _heartRateVariation * 0.95 + microVariation * 0.05; // Slower response
            var targetHR = _baseHeartRate * (1.0 + intensityCycle * 0.4 + _heartRateVariation);
            metrics.HeartRate = (byte)Math.Clamp(targetHR, config.MinHeartRate, config.MaxHeartRate);

            // Update cumulative values
            UpdateCumulativeMetrics(metrics);

            // Generate additional metrics
            GenerateHeartRateVariability(metrics);
            GeneratePowerMetrics(metrics);
        }

        private void UpdateCumulativeMetrics(CyclingMetrics metrics)
        {
            var now = DateTime.Now;

            // Update wheel revolutions based on speed
            var wheelCircumference = 2.1; // meters (700c wheel)
            var timeSinceLastWheel = (now - metrics.LastWheelTime).TotalSeconds;
            if (timeSinceLastWheel > 0)
            {
                var distanceMeters = (metrics.Speed / 3.6) * timeSinceLastWheel; // km/h to m/s
                var revolutions = (uint)(distanceMeters / wheelCircumference);
                metrics.WheelRevolutions += revolutions;

                // Wheel event time in 1/1024 seconds
                var eventTime = (ushort)((timeSinceLastWheel * 1024) % 65536);
                metrics.WheelEventTime = (ushort)((metrics.WheelEventTime + eventTime) % 65536);

                metrics.LastWheelTime = now;
                metrics.Distance += (uint)distanceMeters;
            }

            // Update crank revolutions based on cadence
            var timeSinceLastCrank = (now - metrics.LastCrankTime).TotalSeconds;
            if (timeSinceLastCrank > 0)
            {
                var crankRevs = (ushort)(metrics.Cadence * timeSinceLastCrank / 60.0);
                metrics.CrankRevolutions = (ushort)((metrics.CrankRevolutions + crankRevs) % 65536);

                // Crank event time in 1/1024 seconds
                var eventTime = (ushort)((timeSinceLastCrank * 1024) % 65536);
                metrics.CrankEventTime = (ushort)((metrics.CrankEventTime + eventTime) % 65536);

                metrics.LastCrankTime = now;
            }

            // Update energy
            var timeSinceLastPower = (now - metrics.LastPowerTime).TotalSeconds;
            if (timeSinceLastPower > 0)
            {
                var energyKJ = (uint)(metrics.Power * timeSinceLastPower / 1000.0);
                metrics.AccumulatedEnergy += energyKJ;

                // Rough calorie estimate (1 kJ ≈ 0.239 kcal, with ~25% efficiency)
                metrics.Calories += (ushort)(energyKJ * 0.239 / 0.25);

                metrics.LastPowerTime = now;
            }
        }

        private void GenerateHeartRateVariability(CyclingMetrics metrics)
        {
            // Generate realistic RR intervals (time between heartbeats)
            // Average RR interval = 60000ms / HR
            if (metrics.HeartRate > 0)
            {
                var avgRRInterval = 60000.0 / metrics.HeartRate;

                // Add some variability (HRV)
                var hrv = (_random.NextDouble() - 0.5) * 50; // ±25ms variation
                var rrInterval = (ushort)(avgRRInterval + hrv);

                metrics.RRIntervals.Clear();
                metrics.RRIntervals.Add(rrInterval);

                // Sometimes add a second interval
                if (_random.NextDouble() > 0.5)
                {
                    hrv = (_random.NextDouble() - 0.5) * 50;
                    rrInterval = (ushort)(avgRRInterval + hrv);
                    metrics.RRIntervals.Add(rrInterval);
                }
            }
        }

        private void GeneratePowerMetrics(CyclingMetrics metrics)
        {
            // Pedal power balance (left/right)
            // 50% = balanced, range is typically 45-55%
            metrics.PedalPowerBalance = (byte)(50 + (_random.NextDouble() - 0.5) * 10);

            // Accumulated torque (simplified)
            if (metrics.Cadence > 0)
            {
                var torqueNm = metrics.Power * 60.0 / (2.0 * Math.PI * metrics.Cadence);
                metrics.AccumulatedTorque = (ushort)((metrics.AccumulatedTorque + (ushort)(torqueNm * 32)) % 65536);
            }
        }

        public void SetBaseValues(double speed, int cadence, short power, byte heartRate, double incline)
        {
            _baseSpeed = speed;
            _baseCadence = cadence;
            _basePower = power;
            _baseHeartRate = heartRate;
            _baseIncline = incline;
        }

        public void Reset()
        {
            _sessionStart = DateTime.Now;
            _speedVariation = 0;
            _cadenceVariation = 0;
            _powerVariation = 0;
            _heartRateVariation = 0;
        }
    }
}
