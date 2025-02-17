
using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using System;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rev76.DataModels.Listeners
{
    public class ACCBroadcastListener : IDisposable
    {
        private static ACCUdpRemoteClient _UDPClient = null;
        private readonly object _lock = new object(); 

        public bool Connected { get; set; }  

        

        public ACCBroadcastListener()
        {

        }
        public async Task Listen(object token)
        {
            
            _UDPClient = new ACCUdpRemoteClient("127.0.0.1", 9000, "Rev76", "asd", "", 100);

            _UDPClient.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) =>
            {
                if (connectionId == 0 && connectionSuccess == false)
                {
                    GameData.Instance.Reset();
                    _UDPClient.MessageHandler._entryListCars.Clear();
                }
                else
                {
                    if (Connected == false && connectionSuccess == true)
                    {
                        GameData.Instance.Reset();
                        _UDPClient.MessageHandler._entryListCars.Clear();
                    }
                }
                Connected = connectionSuccess; 

                Trace.WriteLine($"ConnectionStateChanged: {error}");

            };

            _UDPClient.OnRealtimeUpdate += (sender, e) =>
            {
                

                if (GameData.Instance.Session.Phase != e.Phase)
                {
                    Trace.WriteLine($"{GameData.Instance.Session.Phase} <- {e.Phase}");

                    if (e.Phase == SessionPhase.PreSession || e.Phase == SessionPhase.Starting)
                    {
                        GameData.Instance.Reset();
                        _UDPClient.MessageHandler._entryListCars.Clear();
                    }

                    GameData.Instance.Session.Phase = e.Phase;
                    _UDPClient.RequestEntryList();

                }


                GameData.Instance.BroadcastCar = GameData.Instance.Track.Cars.Find(c => c.CarIndex == e.FocusedCarIndex);
               
                if (GameData.Instance.PlayerCarIndex != e.FocusedCarIndex)
                {
                    GameData.Instance.GameState.Broadcasting = true;
                }
                else
                {
                    GameData.Instance.GameState.Broadcasting = false;

                }

                GameData.Instance.Weather.Cloudy = e.Clouds;
                

            };

            _UDPClient.OnRealtimeCarUpdate += (sender, e) =>
            {

                if (GameData.Instance.Track.Cars.Count == 0)
                {
                    LoadCarsFromEntryList();
                }

                Car car = GameData.Instance.Track.Cars.Find(carindex => carindex.CarIndex == e.CarIndex);
                if (car != null)
                {
                    car.BestSessionLap = e.BestSessionLap;
                    car.Laps = e.Laps;
                    car.LastLap = e.LastLap;
                    car.CurrentLap = e.CurrentLap;
                    car.Delta = e.Delta;
                    car.DriverCount = e.DriverCount;
                    car.DriverIndex = e.DriverIndex;
                    car.Gear = e.Gear;
                    car.Kmh = e.Kmh;
                    car.Position = e.Position;
                    car.TrackPosition = e.TrackPosition;
                    car.SplinePosition = e.SplinePosition;
                    car.WorldPosX = e.WorldPosX;
                    car.WorldPosY = e.WorldPosY;
                    car.Yaw = e.Yaw;
                    car.CarLocation = e.CarLocation;
                    car.CupPosition = e.CupPosition;


                    if (!car.LapTimes.Any(lap => lap.LapNumber == e.Laps))
                    {
                        if (e.Laps > 0)
                        {
                            e.LastLap.LapNumber = e.Laps;
                            car.LapTimes.Add(e.LastLap);
                           
                        }

                    }

                }

                TryAssignBestSession(e.LastLap);



                if (GameData.Instance.Track.TrackLength == 0)
                {
                    _UDPClient.RequestTrackData();
                }

            };
            _UDPClient.OnBroadcastingEvent += (sender, e) =>
            {

                GameData.Instance.Session.EventType = e.Type;
                if (e.CarData == null) return;

                if (e.Type == BroadcastingCarEventType.LapCompleted)
                {
                    TryAssignBestSession(e.CarData.BestSessionLap);
                   
                }

                Trace.WriteLine($"BroadcastingEvent: {e.Type.ToString()} | {e.CarId} | {e.Msg}");
                // GameData.Instance.BroadcastCar = GameData.Instance.Track.Cars.Find(c => c.CarIndex == e.CarId);

                //var x = e.CarData.TrackPosition;
                //Trace.WriteLine($"BroadcastingEvent: {e.}");
            };

            _UDPClient.OnEntrylistUpdate += (sender, e) =>
            {
              
                   LoadCarsFromEntryList();
        

            };

            _UDPClient.OnTrackDataUpdate += (sender, e) =>
            {

                GameData.Instance.Track.TrackLength = e.TrackMeters;
                GameData.Instance.Track.Name = e.TrackName;

            };
          

        }

        private static void TryAssignBestSession(LapInfo lapInfo)
        {

            if (lapInfo == null) return;
            
            if (GameData.Instance.Session.BestSession != null)
            {

                if (lapInfo.LaptimeMS < GameData.Instance.Session.BestSession.LaptimeMS)
                {
                    GameData.Instance.Session.BestSession = lapInfo;
                }
             
            }
            else if (lapInfo.LaptimeMS != null && lapInfo.LaptimeMS > 0)
            {
                GameData.Instance.Session.BestSession = lapInfo;
            }
          
        }

        private static void LoadCarsFromEntryList()
        {
            foreach (var c in _UDPClient.MessageHandler._entryListCars)
            {
                Car car = GameData.Instance.Track.Cars.Find(carindex => carindex?.CarIndex == c.CarIndex);
                if (car != null)
                {
                    car.CarIndex = c.CarIndex;
                    car.BestSessionLap = c.BestSessionLap;
                    car.Laps = c.Laps;
                    car.LastLap = c.LastLap;
                    car.CurrentLap = c.CurrentLap;
                    car.Delta = c.Delta;
                    car.DriverCount = c.DriverCount;
                    car.DriverIndex = c.DriverIndex;
                    car.Gear = c.Gear;
                    car.Kmh = c.Kmh;
                    // if (car.CarIndex != GameData.Instance.PlayerCarIndex) {
                    car.Position = c.Position;
                    // }

                    car.TrackPosition = c.TrackPosition;
                    car.SplinePosition = c.SplinePosition;
                    car.WorldPosX = c.WorldPosX;
                    car.WorldPosY = c.WorldPosY;
                    car.Yaw = c.Yaw;
                    car.CarLocation = c.CarLocation;
                    car.CupPosition = c.CupPosition;
                    car.Number = c.RaceNumber;


                    car.Drivers.Clear();
                    foreach (var driver in c.Drivers)
                    {
                        car.Drivers.Add(driver);
                    }

                }
                else
                {
                    car = new Car();
                    car.CarIndex = c.CarIndex;



                    GameData.Instance.Track.Cars.Add(car);
                }

               

                if (GameData.Instance.Session.BestSession == null)
                {

                    if (c.Position == 0) GameData.Instance.Session.BestSession = c.BestSessionLap;
                   
                }

            }
        }

        public void Dispose()
        {
            
           
            _UDPClient.Dispose();
           
            
        }
    }
}
