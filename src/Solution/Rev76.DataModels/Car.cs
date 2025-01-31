
using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using System;
using System.Collections.Generic;


namespace Rev76.DataModels
{
    public class Car
    {



        public int CarIndex { get; set; }
        public int DriverIndex { get; set; }
        public int Gear { get; set; }
        public float WorldPosX { get; set; }
        public float WorldPosY { get; set; }
        public float Yaw { get; set; }
        public CarLocationEnum CarLocation { get; set; }
        public int Kmh { get; set; }
        public int Position { get; set; }
        public int TrackPosition { get; set; }
        public float SplinePosition { get; set; }
        public int Delta { get; set; }
        public LapInfo BestSessionLap { get; set; } = new LapInfo();
        public LapInfo LastLap { get; set; } = new LapInfo();
        public LapInfo CurrentLap { get; set; } = new LapInfo();
        public int Laps { get; set; }
        public ushort CupPosition { get; set; }
        public byte DriverCount { get; set; }

        public string CarModel { get; set; }
        public int Number { get; set; }
        public FuelTank FuelTank { get; set; } = new FuelTank();

        public List<DriverInfo> Drivers { get; } = new List<DriverInfo>();
        public int GapAhead { get; internal set; }
        public int GapBehind { get; internal set; }

        public List<LapInfo> LapTimes = new List<LapInfo>();
        

        public static float CalculateGap(Car car1,  Car car2, float trackMeters)
        {
            // Convert SplinePosition to absolute positions
            float car1Position = (car1.SplinePosition * trackMeters) + (car1.Laps * trackMeters);
            float car2Position = (car2.SplinePosition * trackMeters) + (car2.Laps * trackMeters);

            // Calculate the absolute distance gap
            return Math.Abs(car1Position - car2Position);
        }

        // Optional: Time gap based on speed
        public static float CalculateTimeGap(Car car1, Car car2, float trackMeters)
        {
            float gap = CalculateGap(car1, car2, trackMeters);

            // Average speed in m/s (convert Kmh to m/s)
            float averageSpeed = (car1.Kmh + car2.Kmh) / 2f / 3.6f;

            return averageSpeed > 0 ? gap / averageSpeed : float.MaxValue;
        }


    }
}
