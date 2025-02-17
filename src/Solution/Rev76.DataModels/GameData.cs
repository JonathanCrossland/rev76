
using System;
using System.Diagnostics;

namespace Rev76.DataModels
{
    public sealed class GameData
    {
        private static readonly Lazy<GameData> _instance = new Lazy<GameData>(() => new GameData());
        private readonly object _lock = new object();
        public static GameData Instance => _instance.Value;

        public TrackEnvironment Weather { get; set; } = new TrackEnvironment();
        public GameState GameState { get; set; } = new GameState();
        public Tyres Tyres { get; set; } = new Tyres();
        public Track Track { get; set; } = new Track();
        public int PlayerCarIndex { get; set; }

        private Car _meCar;

        public Car MeCar
        {
            get { 

                if (_meCar == null && PlayerCarIndex > 0 && Track.Cars.Count>0)
                {
                    _meCar = GameData.Instance.Track.Cars.Find(c => c.CarIndex == GameData.Instance.PlayerCarIndex);
                }
                return _meCar; 
            
            }
            
        }


        public Car BroadcastCar { get; set; } = new Car();
        public Session Session { get; set; } = new Session();


        public GameData()
        {
            Reset();
        }

        public void Reset()
        {

            lock (_lock)
            {
                Weather = new TrackEnvironment();
                GameState = new GameState();
                Tyres = new Tyres();
                Track = new Track();
                BroadcastCar = new Car();
                Session = new Session();
                Trace.WriteLine($"GameData Reset.");
            }

        }

        public static string GetFormattedLapTime(float milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            string formattedTime = string.Format("{1:D2}:{2:D2}:{3:D2}",
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

            return formattedTime;
        }

        public static string GetFormattedGap(float seconds)
        {
            if (seconds > 3600 || seconds < 0)
                return "";

            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            if (timeSpan.TotalMinutes >= 1)
            {
                return string.Format("{0}:{1:D2}.{2}",
                    (int)timeSpan.TotalMinutes,
                    timeSpan.Seconds,
                    (int)Math.Round(timeSpan.Milliseconds / 100.0));
            }
            else
            {
                return string.Format("{0}.{1}",
                    timeSpan.Seconds,
                    (int)Math.Round(timeSpan.Milliseconds / 100.0));
            }
        }



        public static string GetFormattedClockTime(float milliseconds)
        {
            // Start from midnight (arbitrary date)
            DateTime startOfDay = new DateTime(2023, 3, 1);  // The date doesn't matter, just using it for time calculation

            // Add the milliseconds as an offset to midnight
            DateTime time = startOfDay.AddSeconds(milliseconds);

            // Format the time in 12-hour format with AM/PM
            string formattedTime = time.ToString("h:mm tt");

            return formattedTime;
        }

       
    }
}
