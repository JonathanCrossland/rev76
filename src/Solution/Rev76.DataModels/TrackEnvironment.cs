namespace Rev76.DataModels
{
    public class TrackEnvironment
    {
        public string AirTemperature { get; set; }
        public string AirDensity { get; set; }
        public string RoadTemperature { get; set; }
        public string WindDirection { get; set; }
        public string WindSpeed { get; set; }
        public RainIntensity RainIntensity { get; set; }
        public RainIntensity RainIn10Minutes { get; set; }
        public RainIntensity RainIn30Minutes { get; set; }
        public string Clock { get; set; }
        public float Cloudy { get; set; }


    }
}
