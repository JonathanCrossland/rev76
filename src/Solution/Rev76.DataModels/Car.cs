
using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using System;
using System.Collections.Concurrent;


namespace Rev76.DataModels
{
    public class Car
    {

        private readonly object _carLocationLock = new object();
        private static readonly object _calculationLock = new object(); // Static lock
        public int CarIndex { get; set; }
        public int DriverIndex { get; set; }
        public int Gear { get; set; }
        public float WorldPosX { get; set; }
        public float WorldPosY { get; set; }
        public float Yaw { get; set; }
       
        private CarLocationEnum _CarLocation;

        public CarLocationEnum CarLocation
        {
            get { return _CarLocation; }
            set {
                lock (_carLocationLock) // Protect the entire setter logic
                {
                    _CarLocation = value;

                    switch (value)
                    {
                        case CarLocationEnum.NONE:
                            InPits = false;
                            break;
                        case CarLocationEnum.Track:
                            InPits = false;
                            IsInAccident = false;
                            break;
                        case CarLocationEnum.Pitlane:
                            InPits = true;
                            break;
                        case CarLocationEnum.PitEntry:
                            InPits = true;
                            break;
                        case CarLocationEnum.PitExit:
                            InPits = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool InPits { get; set; }

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

        public ConcurrentDictionary<int,DriverInfo> Drivers { get; } = new ConcurrentDictionary<int,DriverInfo>();
        public int GapAhead { get; set; }
        public int GapBehind { get; set; }

        public ConcurrentDictionary<int,LapInfo> LapTimes = new ConcurrentDictionary<int,LapInfo>();

        public bool IsInAccident { get; set; }
        public FlagType Flag { get; set; }
        public CarClass CarClass { get; set; }

        public static float CalculateGap(Car car1,  Car car2, float trackMeters)
        {
            lock (_calculationLock)
            {
                // Convert SplinePosition to absolute positions
                float car1Position = (car1.SplinePosition * trackMeters);// + (car1.Laps * trackMeters);
                float car2Position = (car2.SplinePosition * trackMeters);// + (car2.Laps * trackMeters);

                // Calculate the absolute distance gap
                return Math.Abs(car1Position - car2Position);
            }
        }

        // Optional: Time gap based on speed
        public static float CalculateTimeGap(Car car1, Car car2, float trackMeters)
        {
            lock (_calculationLock)
            {
                float gap = CalculateGap(car1, car2, trackMeters);
                if (car1.Kmh == 0) car1.Kmh = car2.Kmh;
                if (car2.Kmh == 0) car2.Kmh = car1.Kmh;
                // Average speed in m/s (convert Kmh to m/s)
                float averageSpeed = (car1.Kmh + car2.Kmh) / 2f / 3.6f;

                return averageSpeed > 0 ? gap / averageSpeed : 0;
            }
        }


    }
}
