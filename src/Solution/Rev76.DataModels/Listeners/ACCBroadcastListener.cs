
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
            _UDPClient = new ACCUdpRemoteClient("127.0.0.1", 9000, "Broadcast", "asd", "", 10);

            _UDPClient.MessageHandler.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) =>
            {
                Trace.WriteLine($"ConnectionStateChanged: {error}");

            };
            _UDPClient.MessageHandler.OnRealtimeUpdate += (sender, e) =>
            {

                
                GameData.BroadcastCar = GameData.Track.Cars.Find(c => c.CarIndex == e.FocusedCarIndex);
               
                if (GameData.Car.CarIndex != e.FocusedCarIndex)
                {
                    GameData.Broadcasting = true;
                }
                else
                {
                    GameData.Broadcasting = false;
                }

                GameData.Weather.Cloudy = e.Clouds;
                

            };

            _UDPClient.MessageHandler.OnRealtimeCarUpdate += (sender, e) =>
            {

               
                Car car = GameData.Track.Cars.Find(carindex => carindex.CarIndex == e.CarIndex);
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


                if (GameData.Session.BestSession != null)
                {
                    if (e.BestSessionLap != null)
                    {
                        if (e.BestSessionLap.LaptimeMS < GameData.Session.BestSession.LaptimeMS || GameData.Session.BestSession.LaptimeMS == null)
                        {
                            GameData.Session.BestSession = e.BestSessionLap;
                        }
                    }
                }
                else
                {
                    GameData.Session.BestSession = e.BestSessionLap;
                    
                    if (GameData.Track.TrackLength == 0)
                    {
                        _UDPClient.RequestTrackData();
                    }
                }




            };
            _UDPClient.MessageHandler.OnBroadcastingEvent += (sender, e) =>
            {

                //Trace.WriteLine($"BroadcastingEvent: {e.CarId}");
               // GameData.BroadcastCar = GameData.Track.Cars.Find(c => c.CarIndex == e.CarId);
                
                //var x = e.CarData.TrackPosition;
                //Trace.WriteLine($"BroadcastingEvent: {e.}");
            };

            _UDPClient.MessageHandler.OnEntrylistUpdate += (sender, e) =>
            {

                foreach (var c in _UDPClient.MessageHandler._entryListCars)
                {
                    Car car = GameData.Track.Cars.Find(carindex => carindex?.CarIndex == c.CarIndex);
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
                        car.Position = c.Position;
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
                        car.Number = c.RaceNumber;

                        GameData.Track.Cars.Add(car);
                    }

                    if (GameData.Session.BestSession != null && c.BestSessionLap == null)
                    {
                        GameData.Reset();
                    }

                    if (GameData.Session.BestSession != null && c.BestSessionLap != null)
                    {
                        if (GameData.Session.BestSession.LaptimeMS == 0 || c.BestSessionLap.LaptimeMS < GameData.Session.BestSession.LaptimeMS)
                        {
                            GameData.Session.BestSession = c.BestSessionLap;
                        }
                    }

                }

            };

            _UDPClient.MessageHandler.OnTrackDataUpdate += (sender, e) =>
            {

                Trace.WriteLine($"TrackDataUpdate: {e.TrackName} ");
                GameData.Track.TrackLength = e.TrackMeters;
                GameData.Track.Name = e.TrackName;

            };

           

        }


        public void Dispose()
        {

            _UDPClient.Shutdown();
            _UDPClient.Dispose();
            
        }
    }
}
