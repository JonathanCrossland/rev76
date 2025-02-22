using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rev76.DataModels.Listeners
{
    public class ACCBroadcastListener : IDisposable
    {
        private static ACCUdpRemoteClient _UDPClient = null;
        public bool Connected { get; set; }  

        public ACCBroadcastListener()
        {
        }
        public async Task Listen(object token)
        {
            
            _UDPClient = new ACCUdpRemoteClient("127.0.0.1", 9000, "Rev76", "asd", "", 100);

            _UDPClient.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) =>
            {
               
                if (connectionSuccess == true)
                {
                    //GameData.Instance.Reset();
                    //_UDPClient.MessageHandler._entryListCars.Clear();

                }
                else
                {
                    GameData.Instance.Reset();
                    _UDPClient.MessageHandler._entryListCars.Clear();
                }

                Connected = connectionSuccess; 

                Trace.WriteLine($"ConnectionStateChanged: {error}");

            };

            _UDPClient.OnRealtimeUpdate += (sender, e) =>
            {

                //if (GameData.Instance.Session.Phase != e.Phase)
                //{
                //    Trace.WriteLine($"{GameData.Instance.Session.Phase} <- {e.Phase}");

                //    if (e.Phase == SessionPhase.PreFormation || e.Phase == SessionPhase.Starting)
                //    {
                //        Trace.WriteLine($"SessionPhase: {e.Phase} Reset");
                //        GameData.Instance.Reset();

                //        _UDPClient.MessageHandler._entryListCars.Clear();
                //        _UDPClient.RequestEntryList();
                //    }

                //    GameData.Instance.Session.Phase = e.Phase;
                //    //_UDPClient.RequestEntryList();

                //}

                GameData.Instance.Session.Phase = e.Phase;

                if (e.Phase == SessionPhase.PreFormation || (e.Phase == SessionPhase.Session))
                {
                    if (_UDPClient.MessageHandler._entryListCars.Count() == 0)
                    {
                        _UDPClient.RequestEntryList();
                    }
                }

                if (GameData.Instance.Track.Cars.TryGetValue(e.FocusedCarIndex, out Car broadcastcar))
                {
                    GameData.Instance.BroadcastCar = broadcastcar;
                }


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
                if (GameData.Instance.Track.Cars.TryGetValue(e.CarIndex, out Car car))
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
                    //car.TrackPosition = e.TrackPosition;
                    car.SplinePosition = e.SplinePosition;
                    car.WorldPosX = e.WorldPosX;
                    car.WorldPosY = e.WorldPosY;
                    car.Yaw = e.Yaw;
                    car.CarLocation = e.CarLocation;
                    car.CupPosition = e.CupPosition;


                    if (!car.LapTimes.ContainsKey(e.Laps))
                    {
                        if (e.Laps >= 0)
                        {
                            e.LastLap.LapNumber = e.Laps;
                            car.LapTimes[e.Laps] = e.LastLap;
                           
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
            try
            {


                foreach (var c in _UDPClient.MessageHandler._entryListCars)
                {
                    if (GameData.Instance.Track.Cars.TryGetValue(c.CarIndex, out Car car))
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

                        car.Position = c.Position;


                        car.TrackPosition = c.TrackPosition;
                        car.SplinePosition = c.SplinePosition;
                        car.WorldPosX = c.WorldPosX;
                        car.WorldPosY = c.WorldPosY;
                        car.Yaw = c.Yaw;
                        car.CarLocation = c.CarLocation;
                        car.CupPosition = c.CupPosition;
                        car.Number = c.RaceNumber;

                        car.LapTimes = new ConcurrentDictionary<int, LapInfo>();

                        //car.Drivers.Clear();
                        for (int i = 0; i < c.Drivers.Count(); i++)
                        {
                            car.Drivers[i] = c.Drivers[i];
                        }

                    }
                    else
                    {
                        car = new Car();
                        car.CarIndex = c.CarIndex;



                       
                        for (int i = 0; i < c.Drivers.Count(); i++)
                        {
                            car.Drivers[i] = c.Drivers[i];
                        }


                        GameData.Instance.Track.Cars[car.CarIndex] = car;
                    }

                    if (GameData.Instance.Session.BestSession == null)
                    {

                        if (c.Position == 0) GameData.Instance.Session.BestSession = c.BestSessionLap;

                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

            //if (GameData.Instance.Track.Cars.Count ==0)
            //{
            //    _UDPClient.RequestEntryList();
            //}
        }

        public void Dispose()
        {
            _UDPClient.Dispose();
        }
    }
}
