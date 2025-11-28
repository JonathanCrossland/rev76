using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        public async Task Listen(CancellationToken token)
        {
           
            _UDPClient = new ACCUdpRemoteClient("127.0.0.1", 9000, "Rev76", "asd", "", 100);
            
            GameData.OnRequestEntryListRefresh = () =>
            {
                if (_UDPClient != null)
                {
                    Trace.WriteLine("Entry list refresh requested via GameData event");
                    _UDPClient.RequestEntryList();
                }
            };
            
            _UDPClient.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) =>
            {
                GameData.Instance.CommandQueue.Enqueue(() =>
                {
                    if (connectionSuccess == false)
                    {
                        GameData.Instance.Reset();
                        _UDPClient.MessageHandler._entryListCars.Clear();
                    }
                    else
                    {
                        // Request initial data on successful connection
                        Trace.WriteLine("Connection successful - requesting initial entry list and track data");
                        _UDPClient.RequestEntryList();
                        _UDPClient.RequestTrackData();
                    }
                    Connected = connectionSuccess;
                    Trace.WriteLine($"ConnectionStateChanged: {error}");
                });
            };

            _UDPClient.OnRealtimeUpdate += (sender, e) =>
            {
                GameData.Instance.PriorityQueue.Enqueue(() =>
                {
                    GameData.Instance.Session.Phase = e.Phase;

                    // Request entry list if we have none, regardless of phase
                    if (_UDPClient.MessageHandler._entryListCars.Count() == 0)
                    {
                        _UDPClient.RequestEntryList();
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
                });
            };

            _UDPClient.OnRealtimeCarUpdate += (sender, e) =>
            {
                GameData.Instance.CommandQueue.Enqueue(() =>
                {
                    if (!GameData.Instance.Track.Cars.ContainsKey(e.CarIndex) || 
                        GameData.Instance.Track.Cars.Count < _UDPClient.MessageHandler._entryListCars.Count)
                    {
                        if (_UDPClient.MessageHandler._entryListCars.Count > 0)
                        {
                            LoadCarsFromEntryList();
                        }
                    }

                    if (!GameData.Instance.Track.Cars.TryGetValue(e.CarIndex, out Car car))
                    {
                        car = new Car();
                        car.LapTimes = new ConcurrentDictionary<int, LapInfo>();
                        car.CarIndex = e.CarIndex;
                        GameData.Instance.Track.Cars[e.CarIndex] = car;
                        
                        // Request entry list when we create a new car to fill in missing data
                        if (_UDPClient.MessageHandler._entryListCars.Count > 0)
                        {
                            _UDPClient.RequestEntryList();
                        }
                    }

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

                    TryAssignBestSession(e.LastLap);

                    if (GameData.Instance.Track.TrackLength == 0)
                    {
                        _UDPClient.RequestTrackData();
                    }
                });
            };

            _UDPClient.OnBroadcastingEvent += (sender, e) =>
            {
                GameData.Instance.CommandQueue.Enqueue(() =>
                {
                    GameData.Instance.Session.EventType = e.Type;
                    if (e.CarData == null) return;

                    if (e.Type == BroadcastingCarEventType.LapCompleted)
                    {
                        TryAssignBestSession(e.CarData.BestSessionLap);
                    }

                    Trace.WriteLine($"BroadcastingEvent: {e.Type.ToString()} | {e.CarId} | {e.Msg}");
                });
            };

            _UDPClient.OnEntrylistUpdate += (sender, e) =>
            {
                GameData.Instance.PriorityQueue.Enqueue(() =>
                {
                    LoadCarsFromEntryList();
                });
            };

            _UDPClient.OnTrackDataUpdate += (sender, e) =>
            {
                GameData.Instance.CommandQueue.Enqueue(() =>
                {
                    GameData.Instance.Track.TrackLength = e.TrackMeters;
                    GameData.Instance.Track.Name = e.TrackName;
                });
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

        private void LoadCarsFromEntryList()
        {
            try
            {
                foreach (var c in _UDPClient.MessageHandler._entryListCars)
                {
                    Car car;
                    bool isNewCar = false;

                    if (!GameData.Instance.Track.Cars.TryGetValue(c.CarIndex, out car))
                    {
                        car = new Car();
                        car.LapTimes = new ConcurrentDictionary<int, LapInfo>();
                        GameData.Instance.Track.Cars[c.CarIndex] = car;
                        isNewCar = true;
                    }

                    car.CarIndex = c.CarIndex;
                    car.Number = c.RaceNumber;
                    car.CarClass = c.CarClass;
                    car.DriverCount = c.DriverCount;

                    for (int i = 0; i < c.Drivers.Count(); i++)
                    {
                        car.Drivers[i] = c.Drivers[i];
                    }

                    if (isNewCar || c.BestSessionLap != null)
                    {
                        car.BestSessionLap = c.BestSessionLap;
                    }

                    // Only update dynamic data for new cars - realtime updates are more accurate
                    if (isNewCar)
                    {
                        car.Laps = c.Laps;
                        car.LastLap = c.LastLap;
                        car.CurrentLap = c.CurrentLap;
                        car.Delta = c.Delta;
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
                    }

                    if (GameData.Instance.Session.BestSession == null)
                    {
                        if (c.Position == 0) GameData.Instance.Session.BestSession = c.BestSessionLap;
                    }

                    TryAssignBestSession(car.BestSessionLap);
                }

                if (GameData.Instance.Track.NumberOfCars != _UDPClient.MessageHandler._entryListCars.Count())
                {
                    GameData.Instance.Track.NumberOfCars = _UDPClient.MessageHandler._entryListCars.Count();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in LoadCarsFromEntryList: {ex}");
            }
        }

        

        public void Dispose()
        {
            _UDPClient?.Dispose();
        }
    }
}