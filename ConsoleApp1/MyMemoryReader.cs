using rF2SharedMemoryNet;
using rF2SharedMemoryNet.RF2Data.Enums;
using rF2SharedMemoryNet.RF2Data.Structs;
using System.Runtime.Versioning;

namespace LMUTelemetryTest
{
    [SupportedOSPlatform("windows")]
    public class MyMemoryReader
    {
        private readonly RF2MemoryReader MemoryReader;

        public MyMemoryReader()
        {
            MemoryReader = new RF2MemoryReader(enableDMA: true);
        }

        public VehicleTelemetry? GetPlayerTelemetry()
        {
            var telemetry = MemoryReader.GetTelemetry();
            var scoring = MemoryReader.GetScoring();

            if (telemetry == null || scoring == null)
                return null;

            var playerVehicle = scoring.Value.Vehicles
                .FirstOrDefault(v => (ControlEntity)v.Control == ControlEntity.Player);

            if (playerVehicle.Equals(default))
                return null;

            var playerTelemetry = telemetry.Value.Vehicles
                .FirstOrDefault(v => v.ID == playerVehicle.ID);

            if (playerTelemetry.Equals(default))
                return null;

            return playerTelemetry;
        }

        public void Close()
        {
            MemoryReader.Dispose();
        }
    }
}