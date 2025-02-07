
using Assetto.Data.Broadcasting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rev76.DataModels.Listeners
{
    public class ACCBroadcastListener : IDisposable
    {
        private static ACCUdpRemoteClient _UDPClient = null;


        public ACCBroadcastListener()
        {

        }
        public async Task Listen(object token)
        {
            
            _UDPClient = new ACCUdpRemoteClient("127.0.0.1", 9000, "Rev76", "asd", "", 1);

            _UDPClient.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) =>
            {
                if (connectionSuccess)
                {
                    _UDPClient.RequestEntryList();
                }
                else
                {
                    GameData.Instance.Reset(); 
                }
                Trace.WriteLine($"ConnectionStateChanged: {error}");

            };
            _UDPClient.OnRealtimeUpdate += (sender, e) =>
            {
                if (GameData.Instance.Session.Phase != e.Phase)
                {
                    GameData.Instance.Session.Phase = e.Phase;
                    GameData.Instance.Reset();
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


                if (GameData.Instance.Session.BestSession != null)
                {
                    if (e.BestSessionLap != null)
                    {
                        if (e.BestSessionLap.LaptimeMS < GameData.Instance.Session.BestSession.LaptimeMS || GameData.Instance.Session.BestSession.LaptimeMS == null)
                        {
                            GameData.Instance.Session.BestSession = e.BestSessionLap;
                        }
                    }
                }
                else
                {
                    GameData.Instance.Session.BestSession = e.BestSessionLap;
                    
                    if (GameData.Instance.Track.TrackLength == 0)
                    {
                        _UDPClient.RequestTrackData();
                    }
                }

               


            };
            _UDPClient.OnBroadcastingEvent += (sender, e) =>
            {

                GameData.Instance.Session.EventType = e.Type;

                if (e.Type == BroadcastingCarEventType.Accident)
                {
                    //Car car = GameData.Instance.Track.Cars.Find(c => c.CarIndex == e.CarData.CarIndex);
                    //car.IsInAccident = true;
                }

                Trace.WriteLine($"BroadcastingEvent: {e.Type.ToString()} | {e.CarId} | {e.Msg}");
                // GameData.Instance.BroadcastCar = GameData.Instance.Track.Cars.Find(c => c.CarIndex == e.CarId);

                //var x = e.CarData.TrackPosition;
                //Trace.WriteLine($"BroadcastingEvent: {e.}");
            };

            _UDPClient.OnEntrylistUpdate += (sender, e) =>
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

                    if (GameData.Instance.Session.BestSession != null && c.BestSessionLap == null)
                    {
                        GameData.Instance.Reset();
                    }

                    if (GameData.Instance.Session.BestSession != null && c.BestSessionLap != null)
                    {
                        if (GameData.Instance.Session.BestSession.LaptimeMS == 0 || c.BestSessionLap.LaptimeMS < GameData.Instance.Session.BestSession.LaptimeMS)
                        {
                            GameData.Instance.Session.BestSession = c.BestSessionLap;
                        }
                    }

                }

            };

            _UDPClient.OnTrackDataUpdate += (sender, e) =>
            {

                Trace.WriteLine($"TrackDataUpdate: {e.TrackName} ");
                GameData.Instance.Track.TrackLength = e.TrackMeters;
                GameData.Instance.Track.Name = e.TrackName;

            };

           

        }


        public void Dispose()
        {

            //_UDPClient.ShutdownAsync();
            _UDPClient.Dispose();
            
        }
    }
}
