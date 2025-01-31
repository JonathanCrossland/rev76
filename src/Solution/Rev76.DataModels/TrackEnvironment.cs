namespace Rev76.DataModels
{
    public class TrackEnvironment
    {

        

        public string AirTemperature { get; internal set; }
        public string AirDensity { get; internal set; }
        public string RoadTemperature { get; internal set; }
        public string WindDirection { get; internal set; }
        public string WindSpeed { get; internal set; }
        public RainIntensity RainIntensity { get; internal set; }
        public RainIntensity RainIn10Minutes { get; internal set; }
        public RainIntensity RainIn30Minutes { get; internal set; }
        public string Clock { get; set; }
        public float Cloudy { get; set; }


    }
}
