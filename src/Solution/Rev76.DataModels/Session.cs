using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;

namespace Rev76.DataModels
{
    public class Session
    {
        public LapInfo BestSession { get; set; }
        public BroadcastingCarEventType EventType { get; set; }
      


        public SessionPhase Phase { get; set; }

        public SessionType SessionType { get; set; }
        public float SessionTimeLeft { get; set; }

        /// <summary>
        /// In the garage, during a race, getting tires changed/fuel added etc.
        /// value of 1 is true
        /// </summary>
        public bool InPit { get; set; }

        /// <summary>
        /// From cross the pit lane in - to out
        /// value of 1 is true
        /// </summary>
        public bool InPitLane { get; set; }

        public bool IsRacing { get; set; }

        public string TrackStatus { get; set; }


        public FlagType Flag { get; set; }

        public int GlobalYellow { get; set; }
        public int FlagSector1 { get; set; }
        public int FlagSector3 { get; set; }
        public int FlagSector2 { get; set; }
        public int GlobalGreen { get; set; }
        public int GlobalRed { get; set; }
        public int GlobalWhite { get; set; }
        public int GlobalChequered { get; set; }
        public int CompletedLaps { get; set; }
    }
}
