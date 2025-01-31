
using System;

namespace Rev76.DataModels
{
    public static class GameData
    {
        public static TrackEnvironment Weather { get; set; } = new TrackEnvironment();
        public static GameState GameState { get; set; } = new GameState();
        public static Tyres Tyres { get; set; } = new Tyres();
        public static Track Track { get; set; } = new Track();
        public static Car Car { get; set; } = new Car();
        public static Session Session { get; set; } = new Session();

        internal static void Reset()
        {
            TrackEnvironment Weather = new TrackEnvironment();
            GameState GameState = new GameState();
            Tyres Tyres = new Tyres();
            Track Track = new Track();
            Car Car = new Car();
            Session Session = new Session();

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
                return "N/A";

            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

           

            if (timeSpan.TotalMinutes >= 1)
            {
                return string.Format("{0:D}:{1:D2}.{2:D2}",
                    (int)timeSpan.TotalMinutes,
                    timeSpan.Seconds,
                     timeSpan.Milliseconds);
            }
            else
            {
                return string.Format("{0:D2}.{1:D2}",
                    timeSpan.Seconds,
                     timeSpan.Milliseconds);
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
