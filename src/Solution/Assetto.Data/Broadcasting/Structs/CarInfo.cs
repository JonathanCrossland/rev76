using System.Collections.Generic;

namespace Assetto.Data.Broadcasting.Structs
{
    public class CarInfo
    {
        // Static fields
        public ushort CarIndex { get; }
        public byte CarModelType { get; internal set; }
        public CarClass CarClass { get; internal set; } = CarClass.GT3;
        public string TeamName { get; internal set; }
        public int RaceNumber { get; internal set; }
        public byte CupCategory { get; internal set; }
        public int CurrentDriverIndex { get; internal set; }
        public IList<DriverInfo> Drivers { get; } = new List<DriverInfo>();
        public NationalityEnum Nationality { get; internal set; }

      
        public int DriverIndex { get; internal set; }
        public int Gear { get; internal set; }
        public float WorldPosX { get; internal set; }
        public float WorldPosY { get; internal set; }
        public float Yaw { get; internal set; }
        public CarLocationEnum CarLocation { get; internal set; }
        public int Kmh { get; internal set; }
        public int Position { get; internal set; }
        public int TrackPosition { get; internal set; }
        public float SplinePosition { get; internal set; }
        public int Delta { get; internal set; }
        public LapInfo BestSessionLap { get; internal set; }
        public LapInfo LastLap { get; internal set; }
        public LapInfo CurrentLap { get; internal set; }
        public int Laps { get; internal set; }
        public ushort CupPosition { get; internal set; }
        public byte DriverCount { get; internal set; }



        public CarInfo(ushort carIndex)
        {
            CarIndex = carIndex;
        }

        internal void AddDriver(DriverInfo driverInfo)
        {
            Drivers.Add(driverInfo);
        }

        public string GetCurrentDriverName()
        {
            if (CurrentDriverIndex < Drivers.Count)
                return Drivers[CurrentDriverIndex].LastName;
            return "nobody(?)";
        }

        public DriverInfo? GetCurrentDriver()
        {
            if (CurrentDriverIndex < Drivers.Count)
                return Drivers[CurrentDriverIndex];

            return null;
        }

    }

}
